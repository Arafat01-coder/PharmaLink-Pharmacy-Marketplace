namespace PharmaLinkApp.Database
{
    /// <summary>
    /// A database failure that has already been turned into a sentence a user
    /// can act on.
    ///
    /// Every SqlException raised inside DbHelper is wrapped in one of these, so
    /// no screen in the application ever shows raw SQL Server text. The original
    /// SqlException is kept as InnerException for debugging.
    /// </summary>
    public class DataAccessException : Exception
    {
        public DataAccessException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
