using System.Data;
using System.Text;
using PharmaLinkApp.Database;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// Every reporting query in the application.
    ///
    /// One revenue rule applies to every figure here, so no two screens can
    /// disagree:
    ///  - only orders whose Status is Confirmed or Delivered count as sales
    ///    (Placed orders may still be refused, Cancelled ones never happened);
    ///  - gross sales = SUM(OrderItems.Subtotal), so the delivery charge, which
    ///    the pharmacy passes on to the rider, is excluded;
    ///  - commission = SUM(Orders.CommissionAmount), the amount frozen on each
    ///    order at checkout, so changing a rate never rewrites past figures.
    ///
    /// The interesting query is <see cref="GetEarnings"/>: the Super Admin runs
    /// it with pharmacyId = 0 and sees every shop, the pharmacy owner runs the
    /// same query with his own PharmacyId and sees only his own row. One query,
    /// two role scopes, which is the clearest demonstration of the data
    /// isolation rule in the system.
    /// </summary>
    public class ReportService
    {
        private readonly DbHelper _db = new DbHelper();

        // =====================================================================
        //  EARNINGS AND COMMISSION   (JOIN + GROUP BY + SUM + AVG + COUNT)
        // =====================================================================

        /// <summary>
        /// Requirements 5 and 13. Pass pharmacyId = 0 for the platform wide
        /// report, or a real PharmacyId for one owner's own report.
        ///
        /// status = "" means the revenue statuses (Confirmed + Delivered); any
        /// other value reports exactly that one status, for example "Placed" to
        /// see what is still waiting.
        ///
        /// Order level and item level figures are aggregated in separate derived
        /// tables because the join to OrderItems multiplies the order rows,
        /// which would inflate any SUM taken from the order header.
        /// AverageItemPrice is the average unit price across the order lines.
        /// </summary>
        public DataTable GetEarnings(int pharmacyId, DateTime fromDate, DateTime toDate, string area, string status)
        {
            const string sql = @"
SELECT  ph.PharmacyId,
        ph.PharmacyName,
        ph.Area,
        ord.TotalOrders,
        itm.UnitsSold,
        itm.GrossSales,
        ord.PlatformCommission,
        CAST(itm.GrossSales - ord.PlatformCommission AS DECIMAL(12,2)) AS NetEarnings,
        itm.AverageItemPrice,
        ph.CommissionRate
FROM    Pharmacies ph
        -- one row per pharmacy, counted at order level so the commission
        -- frozen on each order header is summed exactly once
        INNER JOIN (SELECT  o.PharmacyId,
                            COUNT(*)                                       AS TotalOrders,
                            CAST(SUM(o.CommissionAmount) AS DECIMAL(12,2)) AS PlatformCommission
                    FROM    Orders o
                    WHERE   o.OrderDate BETWEEN @FromDate AND @ToDate
                      AND   ((@Status = '' AND o.Status IN ('Confirmed', 'Delivered')) OR o.Status = @Status)
                    GROUP BY o.PharmacyId) ord ON ord.PharmacyId = ph.PharmacyId
        -- and one row per pharmacy at line item level for the item figures
        INNER JOIN (SELECT  o.PharmacyId,
                            SUM(oi.Quantity)                          AS UnitsSold,
                            CAST(SUM(oi.Subtotal) AS DECIMAL(12,2))   AS GrossSales,
                            CAST(AVG(oi.UnitPrice) AS DECIMAL(10,2))  AS AverageItemPrice
                    FROM    Orders o
                            INNER JOIN OrderItems oi ON oi.OrderId = o.OrderId
                    WHERE   o.OrderDate BETWEEN @FromDate AND @ToDate
                      AND   ((@Status = '' AND o.Status IN ('Confirmed', 'Delivered')) OR o.Status = @Status)
                    GROUP BY o.PharmacyId) itm ON itm.PharmacyId = ph.PharmacyId
WHERE   (@PharmacyId = 0  OR ph.PharmacyId = @PharmacyId)
  AND   (@Area       = '' OR ph.Area       = @Area)
ORDER BY itm.GrossSales DESC;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@PharmacyId", pharmacyId),
                DbHelper.P("@FromDate", fromDate.Date),
                DbHelper.P("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1)),
                DbHelper.P("@Area", area ?? ""),
                DbHelper.P("@Status", status ?? ""));
        }

        /// <summary>
        /// Requirement 13, the detail behind the tiles: who bought what, on which
        /// date and at what price, for one pharmacy only. Confirmed and Delivered
        /// orders only, matching the tiles above it.
        /// </summary>
        public DataTable GetSalesDetail(int pharmacyId, DateTime fromDate, DateTime toDate, int medicineId)
        {
            const string sql = @"
SELECT  o.OrderId, o.OrderDate, u.FullName AS Customer,
        m.MedicineName, m.Strength, oi.Quantity, oi.UnitPrice, oi.Subtotal,
        o.PaymentMethod, o.Status
FROM    Orders o
        INNER JOIN OrderItems oi ON oi.OrderId    = o.OrderId
        INNER JOIN Medicines  m  ON m.MedicineId  = oi.MedicineId
        INNER JOIN Users      u  ON u.UserId      = o.CustomerId
WHERE   o.PharmacyId = @PharmacyId
  AND   o.Status IN ('Confirmed', 'Delivered')
  AND   o.OrderDate BETWEEN @FromDate AND @ToDate
  AND   (@MedicineId = 0 OR m.MedicineId = @MedicineId)
ORDER BY o.OrderDate DESC, o.OrderId;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@PharmacyId", pharmacyId),
                DbHelper.P("@FromDate", fromDate.Date),
                DbHelper.P("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1)),
                DbHelper.P("@MedicineId", medicineId));
        }

        /// <summary>
        /// The headline figures on the pharmacy owner's earnings screen, for
        /// Confirmed and Delivered orders in the date range.
        ///
        /// medicineId = 0 reports the whole shop, and commission is the exact
        /// SUM of the frozen Orders.CommissionAmount. When one medicine is
        /// chosen, an order's commission has to be shared out among its lines:
        /// each line carries Subtotal / ItemsTotal of that order's commission,
        /// which adds back up to the order's figure across all of its lines.
        /// </summary>
        public void GetEarningsTotals(int pharmacyId, DateTime fromDate, DateTime toDate, int medicineId,
                                      out decimal grossSales, out decimal commission,
                                      out decimal netEarnings, out int unitsSold)
        {
            const string sql = @"
SELECT  ISNULL(SUM(oi.Subtotal), 0)            AS GrossSales,
        ISNULL(SUM(oi.Quantity), 0)            AS UnitsSold,
        CASE WHEN @MedicineId = 0
             THEN (SELECT ISNULL(SUM(o2.CommissionAmount), 0)
                   FROM   Orders o2
                   WHERE  o2.PharmacyId = @PharmacyId
                     AND  o2.Status IN ('Confirmed', 'Delivered')
                     AND  o2.OrderDate BETWEEN @FromDate AND @ToDate)
             ELSE CAST(ISNULL(SUM(oi.Subtotal * o.CommissionAmount / NULLIF(o.ItemsTotal, 0)), 0) AS DECIMAL(12,2))
        END                                    AS Commission
FROM    Orders o
        INNER JOIN OrderItems oi ON oi.OrderId = o.OrderId
WHERE   o.PharmacyId = @PharmacyId
  AND   o.Status IN ('Confirmed', 'Delivered')
  AND   o.OrderDate BETWEEN @FromDate AND @ToDate
  AND   (@MedicineId = 0 OR oi.MedicineId = @MedicineId);";

            DataTable table = _db.ExecuteTable(sql,
                DbHelper.P("@PharmacyId", pharmacyId),
                DbHelper.P("@FromDate", fromDate.Date),
                DbHelper.P("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1)),
                DbHelper.P("@MedicineId", medicineId));

            grossSales = 0m; commission = 0m; unitsSold = 0;
            if (table.Rows.Count > 0)
            {
                grossSales = DbHelper.GetDecimal(table.Rows[0], "GrossSales");
                commission = DbHelper.GetDecimal(table.Rows[0], "Commission");
                unitsSold = DbHelper.GetInt(table.Rows[0], "UnitsSold");
            }
            netEarnings = grossSales - commission;
        }

        // =====================================================================
        //  SUPER ADMIN REPORTS
        // =====================================================================

        /// <summary>
        /// Requirement 6. Ratings sit on medicines, not on pharmacies, so the
        /// average has to be built by joining three tables and grouping back up
        /// to the pharmacy. HAVING is the right clause because the condition is
        /// on the aggregate itself, and the second condition,
        /// COUNT(ReviewId) >= @MinReviews, is a deliberate fairness rule: one
        /// angry customer should not be enough to put a shop on the suspension
        /// list. Hidden reviews are left out.
        /// </summary>
        public DataTable GetLowRatedPharmacies(decimal ratingThreshold, int minimumReviews)
        {
            const string sql = @"
SELECT  ph.PharmacyId, ph.PharmacyName, ph.Area,
        u.FullName AS OwnerName, u.Phone AS OwnerPhone,
        COUNT(r.ReviewId)                                          AS TotalReviews,
        CAST(AVG(CAST(r.Rating AS DECIMAL(4,2))) AS DECIMAL(4,2))  AS AverageRating,
        ph.Status
FROM    Pharmacies ph
        INNER JOIN Users     u ON u.UserId     = ph.OwnerId
        INNER JOIN Medicines m ON m.PharmacyId = ph.PharmacyId
        INNER JOIN Reviews   r ON r.MedicineId = m.MedicineId
WHERE   r.IsHidden = 0
GROUP BY ph.PharmacyId, ph.PharmacyName, ph.Area, u.FullName, u.Phone, ph.Status
HAVING  AVG(CAST(r.Rating AS DECIMAL(4,2))) < @Threshold
   AND  COUNT(r.ReviewId) >= @MinReviews
ORDER BY AverageRating ASC;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@Threshold", ratingThreshold),
                DbHelper.P("@MinReviews", minimumReviews));
        }

        /// <summary>
        /// Which parts of the city are worth expanding into: confirmed and
        /// delivered orders grouped by pharmacy area, with HAVING dropping areas
        /// that have not crossed a meaningful revenue figure yet. Revenue is item
        /// sales (delivery excluded), summed per order first so the commission
        /// on the order header is not multiplied by the number of lines.
        /// </summary>
        public DataTable GetRevenueByArea(decimal minimumRevenue)
        {
            const string sql = @"
SELECT  ph.Area,
        COUNT(DISTINCT ph.PharmacyId)                    AS PharmaciesInArea,
        COUNT(o.OrderId)                                 AS Orders,
        CAST(SUM(it.ItemSales) AS DECIMAL(12,2))         AS Revenue,
        CAST(SUM(o.CommissionAmount) AS DECIMAL(12,2))   AS CommissionEarned
FROM    Pharmacies ph
        INNER JOIN Orders o ON o.PharmacyId = ph.PharmacyId
        INNER JOIN (SELECT OrderId, SUM(Subtotal) AS ItemSales
                    FROM   OrderItems
                    GROUP BY OrderId) it ON it.OrderId = o.OrderId
WHERE   o.Status IN ('Confirmed', 'Delivered')
GROUP BY ph.Area
HAVING  SUM(it.ItemSales) > @MinRevenue
ORDER BY Revenue DESC;";

            return _db.ExecuteTable(sql, DbHelper.P("@MinRevenue", minimumRevenue));
        }

        /// <summary>
        /// The tiles and header figures on the Super Admin dashboard. Orders,
        /// revenue (item sales) and commission follow the revenue rule above;
        /// customers counts Active customer accounts only.
        /// </summary>
        public void GetPlatformTotals(out int pharmacies, out int pendingPharmacies, out int customers,
                                      out int orders, out decimal revenue, out decimal commission)
        {
            const string sql = @"
SELECT
    (SELECT COUNT(*) FROM Pharmacies WHERE Status = 'Approved')                          AS ApprovedPharmacies,
    (SELECT COUNT(*) FROM Pharmacies WHERE Status = 'Pending')                           AS PendingPharmacies,
    (SELECT COUNT(*) FROM Users      WHERE UserType = 'Customer' AND Status = 'Active')  AS Customers,
    (SELECT COUNT(*) FROM Orders     WHERE Status IN ('Confirmed', 'Delivered'))         AS Orders,
    (SELECT ISNULL(SUM(oi.Subtotal), 0)
     FROM   OrderItems oi INNER JOIN Orders o ON o.OrderId = oi.OrderId
     WHERE  o.Status IN ('Confirmed', 'Delivered'))                                      AS Revenue,
    (SELECT ISNULL(SUM(CommissionAmount), 0) FROM Orders
     WHERE  Status IN ('Confirmed', 'Delivered'))                                        AS Commission;";

            DataTable table = _db.ExecuteTable(sql);
            DataRow row = table.Rows[0];

            pharmacies = DbHelper.GetInt(row, "ApprovedPharmacies");
            pendingPharmacies = DbHelper.GetInt(row, "PendingPharmacies");
            customers = DbHelper.GetInt(row, "Customers");
            orders = DbHelper.GetInt(row, "Orders");
            revenue = DbHelper.GetDecimal(row, "Revenue");
            commission = DbHelper.GetDecimal(row, "Commission");
        }

        /// <summary>
        /// The four tiles on the pharmacy owner's dashboard. orders, revenue
        /// (item sales) and commission cover Confirmed + Delivered orders;
        /// pendingOrders counts the Placed orders still waiting for the owner.
        /// </summary>
        public void GetPharmacyTotals(int pharmacyId, out int orders, out decimal revenue,
                                      out decimal commission, out int pendingOrders)
        {
            const string sql = @"
SELECT
    (SELECT COUNT(*) FROM Orders WHERE PharmacyId = @Id AND Status IN ('Confirmed', 'Delivered')) AS Orders,
    (SELECT ISNULL(SUM(oi.Subtotal), 0)
     FROM   OrderItems oi INNER JOIN Orders o ON o.OrderId = oi.OrderId
     WHERE  o.PharmacyId = @Id AND o.Status IN ('Confirmed', 'Delivered'))                      AS Revenue,
    (SELECT ISNULL(SUM(CommissionAmount), 0) FROM Orders
     WHERE  PharmacyId = @Id AND Status IN ('Confirmed', 'Delivered'))                          AS Commission,
    (SELECT COUNT(*) FROM Orders WHERE PharmacyId = @Id AND Status = 'Placed')                  AS PendingOrders;";

            DataTable table = _db.ExecuteTable(sql, DbHelper.P("@Id", pharmacyId));
            DataRow row = table.Rows[0];

            orders = DbHelper.GetInt(row, "Orders");
            revenue = DbHelper.GetDecimal(row, "Revenue");
            commission = DbHelper.GetDecimal(row, "Commission");
            pendingOrders = DbHelper.GetInt(row, "PendingOrders");
        }

        /// <summary>The best selling medicines (confirmed and delivered orders), shown on both dashboards.</summary>
        public DataTable GetTopSellingMedicines(int pharmacyId, int topN)
        {
            const string sql = @"
SELECT  TOP (@TopN)
        m.MedicineName, m.Strength, ph.PharmacyName,
        SUM(oi.Quantity) AS UnitsSold,
        SUM(oi.Subtotal) AS Revenue
FROM    OrderItems oi
        INNER JOIN Orders     o  ON o.OrderId     = oi.OrderId
        INNER JOIN Medicines  m  ON m.MedicineId  = oi.MedicineId
        INNER JOIN Pharmacies ph ON ph.PharmacyId = m.PharmacyId
WHERE   o.Status IN ('Confirmed', 'Delivered')
  AND   (@PharmacyId = 0 OR m.PharmacyId = @PharmacyId)
GROUP BY m.MedicineName, m.Strength, ph.PharmacyName
ORDER BY UnitsSold DESC;";

            return _db.ExecuteTable(sql,
                DbHelper.P("@TopN", topN),
                DbHelper.P("@PharmacyId", pharmacyId));
        }

        // =====================================================================
        //  CSV EXPORT
        // =====================================================================

        /// <summary>
        /// Writes any DataTable to a CSV file so a report can be reconciled
        /// against bank settlements outside the application.
        ///
        /// Text cells that start with = + - @ (or a tab / carriage return) are
        /// prefixed with an apostrophe, because Excel and LibreOffice treat such
        /// a cell as a formula: a customer named "=HYPERLINK(...)" must arrive in
        /// the spreadsheet as text, not run. Numeric columns are written as they
        /// are, so a negative number stays a number.
        /// </summary>
        public static bool ExportToCsv(DataTable table, string filePath, out string message)
        {
            try
            {
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    builder.Append(Escape(NeutraliseFormula(table.Columns[i].ColumnName)));
                    if (i < table.Columns.Count - 1) builder.Append(',');
                }
                builder.AppendLine();

                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        string text = row[i] == DBNull.Value ? "" : row[i].ToString();
                        if (!IsNumeric(table.Columns[i].DataType)) text = NeutraliseFormula(text);
                        builder.Append(Escape(text));
                        if (i < table.Columns.Count - 1) builder.Append(',');
                    }
                    builder.AppendLine();
                }

                File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
                message = "Exported " + table.Rows.Count + " row(s) to " + filePath;
                return true;
            }
            catch (Exception ex)
            {
                message = "The file could not be written: " + ex.Message;
                return false;
            }
        }

        private static bool IsNumeric(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
                   type == typeof(decimal) || type == typeof(double) || type == typeof(float);
        }

        private static string NeutraliseFormula(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            char first = value[0];
            return first == '=' || first == '+' || first == '-' || first == '@' || first == '\t' || first == '\r'
                ? "'" + value
                : value;
        }

        private static string Escape(string value)
        {
            if (value == null) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
