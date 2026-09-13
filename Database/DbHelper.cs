using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PharmaLinkApp.Database
{
    /// <summary>
    /// The one place in the application that knows how to reach SQL Server.
    ///
    /// Every method here takes the SQL text and an array of SqlParameter values.
    /// Nothing is ever concatenated into a query string, which is what keeps the
    /// whole system safe from SQL injection: a customer who types
    /// "'; DROP TABLE Users; --" into the search box searches for that literal
    /// text and finds nothing.
    ///
    /// This is also the one place that translates SqlException into readable
    /// English. Because every query in the application passes through here, a
    /// form that catches Exception and shows ex.Message gets a sentence rather
    /// than a SQL Server error number, and so does the global handler in
    /// Program.cs for the calls that are not wrapped at the call site.
    /// </summary>
    public class DbHelper
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["db"].ConnectionString;

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>Runs a SELECT and returns the whole result set as a DataTable, ready for a DataGridView.</summary>
        public DataTable ExecuteTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                DataTable table = new DataTable();
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(table);
                    }
                }
                catch (SqlException ex)
                {
                    throw new DataAccessException(Describe(ex), ex);
                }
                return table;
            }
        }

        /// <summary>Runs an INSERT, UPDATE or DELETE and returns how many rows it changed.</summary>
        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                try
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw new DataAccessException(Describe(ex), ex);
                }
            }
        }

        /// <summary>Runs a query that returns a single value, for example a COUNT or a SUM.</summary>
        public object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                try
                {
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
                catch (SqlException ex)
                {
                    throw new DataAccessException(Describe(ex), ex);
                }
            }
        }

        public int ExecuteScalarInt(string sql, params SqlParameter[] parameters)
        {
            object value = ExecuteScalar(sql, parameters);
            if (value == null || value == DBNull.Value) return 0;
            return Convert.ToInt32(value);
        }

        public decimal ExecuteScalarDecimal(string sql, params SqlParameter[] parameters)
        {
            object value = ExecuteScalar(sql, parameters);
            if (value == null || value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value);
        }

        public string ExecuteScalarString(string sql, params SqlParameter[] parameters)
        {
            object value = ExecuteScalar(sql, parameters);
            if (value == null || value == DBNull.Value) return string.Empty;
            return value.ToString();
        }

        // -- turning SQL Server error numbers into English ----------------------

        /// <summary>
        /// A readable sentence for any exception a form might catch.
        ///
        /// Services that open their own connection for a transaction (checkout,
        /// pharmacy owner registration, pharmacy delete) do not pass through
        /// ExecuteTable / ExecuteNonQuery, so they call this to get the same
        /// English the rest of the application shows instead of raw SQL text.
        /// </summary>
        public static string Describe(Exception ex)
        {
            if (ex == null) return "Something went wrong, but no further detail is available.";
            if (ex is DataAccessException) return ex.Message;
            if (ex is SqlException sql) return Describe(sql);
            if (ex.InnerException is SqlException inner) return Describe(inner);
            return ex.Message;
        }

        /// <summary>
        /// Maps the SqlException numbers this application can actually provoke
        /// onto a sentence that tells the user what to do next. Anything not
        /// listed falls through to a generic message with the server's own text,
        /// so nothing is ever swallowed silently.
        /// </summary>
        public static string Describe(SqlException ex)
        {
            switch (ex.Number)
            {
                case 53:        // server not found
                case -1:        // connection could not be established
                case 4060:      // cannot open database
                case 18456:     // login failed
                case 40615:     // firewall
                    return "PharmaLink could not reach the database.\r\n\r\n" +
                           "Check that SQL Server is running, that PharmaLinkDB has been created " +
                           "from PharmaLinkDB_Setup.sql, and that the connection string in " +
                           "App.config points at the right server.";

                case -2:        // timeout
                    return "The database took too long to answer. It may be busy - try again in a moment.";

                case 2627:      // PRIMARY KEY / UNIQUE constraint
                case 2601:      // unique index
                    return "That record already exists.\r\n\r\n" +
                           "The database keeps this value unique, so the same entry cannot be " +
                           "saved twice. Change the value and try again.";

                case 547:       // FK or CHECK constraint
                    return ex.Message.Contains("CHECK")
                        ? "One of the values breaks a rule the database enforces, for example a " +
                          "negative price, a commission above 30 percent or a status that is not allowed."
                        : "This record is still referenced by other data, or it points at something " +
                          "that no longer exists, so the database refused the change.\r\n\r\n" +
                          "Where a record has history behind it, deactivate it instead of deleting it.";

                case 515:       // NOT NULL
                    return "A required field was left empty. Fill in every field marked as required and try again.";

                case 8152:      // string truncation (legacy)
                case 2628:      // string truncation
                    return "One of the values is too long for the field it is being saved into. Shorten it and try again.";

                default:
                    return "The database refused the last operation.\r\n\r\n" + ex.Message;
            }
        }

        // -- small helpers so the service classes read cleanly ------------------

        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static int GetInt(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return 0;
            return Convert.ToInt32(row[column]);
        }

        public static decimal GetDecimal(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return 0m;
            return Convert.ToDecimal(row[column]);
        }

        public static string GetString(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return string.Empty;
            return row[column].ToString();
        }

        public static bool GetBool(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return false;
            return Convert.ToBoolean(row[column]);
        }

        public static DateTime GetDate(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return DateTime.MinValue;
            return Convert.ToDateTime(row[column]);
        }
    }
}
