namespace PharmaLinkApp
{
    /// <summary>
    /// Holds the identity of the person who is logged in.
    ///
    /// PharmacyId is the important one: every Admin side query in the
    /// application carries "WHERE PharmacyId = @PharmacyId" and reads the
    /// value from here, which is what stops one pharmacy owner from ever
    /// seeing another owner's medicines, orders or earnings.
    /// It is 0 for a SuperAdmin and for a Customer, because neither of them
    /// owns a shop.
    /// </summary>
    public static class UserSession
    {
        public static int UserId;
        public static string FullName = "";
        public static string Email = "";
        public static string UserType = "";      // 'SuperAdmin' | 'Admin' | 'Customer'
        public static int PharmacyId;            // 0 when the user is not a pharmacy owner
        public static string PharmacyName = "";

        public static bool IsSuperAdmin => UserType == "SuperAdmin";
        public static bool IsAdmin => UserType == "Admin";
        public static bool IsCustomer => UserType == "Customer";

        public static void Clear()
        {
            UserId = 0;
            FullName = "";
            Email = "";
            UserType = "";
            PharmacyId = 0;
            PharmacyName = "";
        }
    }
}
