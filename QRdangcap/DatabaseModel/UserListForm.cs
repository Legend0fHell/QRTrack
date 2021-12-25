using QRdangcap.GoogleDatabase;
using SQLite;

namespace QRdangcap.DatabaseModel
{
    public class UserListForm
    {
        // Contains info from GoogleDatabase, use specifically for local database.
        [PrimaryKey, Unique]
        public string Username { get; set; }
        public int StId { get; set; }
        public string StName { get; set; }
        public string UnsignStName => Convert(StName);
        public string StClass { get; set; }
        public string UnsignStClass => Convert(StClass);
        public int Priv { get; set; }
        public int IsHidden { get; set; }
        public int LogStatus { get; set; }
        public int RankingPoint { get; set; }
        public int Ranking { get; set; }
        public string CovidExposure { get; set; }
        public string CovidExposureFrom { get; set; }
        private static readonly RetrieveAllUserDb instance = new RetrieveAllUserDb();
        private string Convert(string str)
        {
            return instance.ConvertToUnsign(str).ToLower();
        }
    }
}