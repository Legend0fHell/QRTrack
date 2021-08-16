using SQLite;

namespace QRdangcap.DatabaseModel
{
    public class UserListForm
    {
        // Contains info from GoogleDatabase, use specifically for local database.
        [PrimaryKey, Unique]
        public int StId { get; set; }
        public string StName { get; set; }
        public string StClass { get; set; }
        public int Priv { get; set; }
        public int LogStatus { get; set; }
    }
}