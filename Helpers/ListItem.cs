namespace PharmaLinkApp.Helpers
{
    /// <summary>
    /// One entry in a ComboBox that is filled from a database row: it carries
    /// the record's id and shows only the text a person should read.
    ///
    /// Several lists used to add strings such as "3 - Seclo 20mg" and cut the
    /// id back off the front when an entry was chosen, which put database ids
    /// in front of customers. Adding a ListItem instead keeps the id out of
    /// sight and removes the string parsing.
    /// </summary>
    public sealed class ListItem
    {
        public ListItem(int id, string text)
        {
            Id = id;
            Text = text ?? string.Empty;
        }

        public int Id { get; }
        public string Text { get; }

        /// <summary>ComboBox controls display this.</summary>
        public override string ToString() => Text;
    }
}
