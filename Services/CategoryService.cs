using System.Data;
using PharmaLinkApp.Database;
using PharmaLinkApp.Models;

namespace PharmaLinkApp.Services
{
    /// <summary>
    /// The master medicine category list, maintained by the Super Admin
    /// (requirement 7). A category that is already referenced by a medicine is
    /// never deleted, only deactivated, which keeps every foreign key valid.
    /// </summary>
    public class CategoryService
    {
        private readonly DbHelper _db = new DbHelper();

        public DataTable GetTable(bool activeOnly)
        {
            const string sql = @"
SELECT  c.CategoryId, c.CategoryName, c.Description, c.IsActive,
        (SELECT COUNT(*) FROM Medicines m WHERE m.CategoryId = c.CategoryId) AS MedicineCount
FROM    Categories c
WHERE   (@ActiveOnly = 0 OR c.IsActive = 1)
ORDER BY c.CategoryName;";

            return _db.ExecuteTable(sql, DbHelper.P("@ActiveOnly", activeOnly ? 1 : 0));
        }

        /// <summary>Feeds every category ComboBox in the application.</summary>
        public List<Category> GetActiveList()
        {
            List<Category> list = new List<Category>();
            DataTable table = _db.ExecuteTable(
                "SELECT CategoryId, CategoryName, Description, IsActive FROM Categories WHERE IsActive = 1 ORDER BY CategoryName;");

            foreach (DataRow row in table.Rows)
            {
                list.Add(new Category
                {
                    CategoryId = DbHelper.GetInt(row, "CategoryId"),
                    CategoryName = DbHelper.GetString(row, "CategoryName"),
                    Description = DbHelper.GetString(row, "Description"),
                    IsActive = DbHelper.GetBool(row, "IsActive")
                });
            }
            return list;
        }

        /// <summary>
        /// The category list for the medicine editor: every active category plus
        /// the medicine's current one even if the Super Admin has since
        /// deactivated it. Without that row the ComboBox cannot select the
        /// current value and silently falls back to the first item, so saving
        /// an unrelated change would move the medicine to another category.
        /// Pass 0 when adding a new medicine.
        /// </summary>
        public List<Category> GetListForEditor(int includeCategoryId)
        {
            List<Category> list = new List<Category>();
            DataTable table = _db.ExecuteTable(@"
SELECT CategoryId, CategoryName, Description, IsActive
FROM   Categories
WHERE  IsActive = 1 OR CategoryId = @Include
ORDER BY CategoryName;",
                DbHelper.P("@Include", includeCategoryId));

            foreach (DataRow row in table.Rows)
            {
                list.Add(new Category
                {
                    CategoryId = DbHelper.GetInt(row, "CategoryId"),
                    CategoryName = DbHelper.GetString(row, "CategoryName"),
                    Description = DbHelper.GetString(row, "Description"),
                    IsActive = DbHelper.GetBool(row, "IsActive")
                });
            }
            return list;
        }

        public bool NameExists(string name, int ignoreCategoryId)
        {
            return _db.ExecuteScalarInt(
                "SELECT COUNT(*) FROM Categories WHERE CategoryName = @Name AND CategoryId <> @Id;",
                DbHelper.P("@Name", name.Trim()),
                DbHelper.P("@Id", ignoreCategoryId)) > 0;
        }

        public int Add(string name, string description)
        {
            const string sql = @"
INSERT INTO Categories (CategoryName, Description) VALUES (@Name, @Description);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return _db.ExecuteScalarInt(sql,
                DbHelper.P("@Name", name.Trim()),
                DbHelper.P("@Description", description));
        }

        public bool Update(int categoryId, string name, string description)
        {
            return _db.ExecuteNonQuery(
                "UPDATE Categories SET CategoryName = @Name, Description = @Description WHERE CategoryId = @Id;",
                DbHelper.P("@Name", name.Trim()),
                DbHelper.P("@Description", description),
                DbHelper.P("@Id", categoryId)) == 1;
        }

        /// <summary>Soft delete: existing Medicines rows keep pointing at a valid row.</summary>
        public bool SetActive(int categoryId, bool active)
        {
            return _db.ExecuteNonQuery(
                "UPDATE Categories SET IsActive = @Active WHERE CategoryId = @Id;",
                DbHelper.P("@Active", active ? 1 : 0),
                DbHelper.P("@Id", categoryId)) == 1;
        }

        public bool IsReferenced(int categoryId)
        {
            return _db.ExecuteScalarInt(
                "SELECT COUNT(*) FROM Medicines WHERE CategoryId = @Id;",
                DbHelper.P("@Id", categoryId)) > 0;
        }

        /// <summary>Only allowed when nothing points at the category.</summary>
        public bool Delete(int categoryId)
        {
            if (IsReferenced(categoryId)) return false;
            return _db.ExecuteNonQuery(
                "DELETE FROM Categories WHERE CategoryId = @Id;",
                DbHelper.P("@Id", categoryId)) == 1;
        }
    }
}
