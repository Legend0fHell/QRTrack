using Plugin.CloudFirestore.Attributes;
using SQLite;
using System;

namespace QRdangcap.DatabaseModel
{
    public class LogListForm
    {
        // Contains info from GoogleDatabase, use specifically for local database.
        [PrimaryKey, Unique]
        [Id]
        public string Keys { get; set; }

        public int StId { get; set; }
        public string StName => GetInfo(1, StId);
        public string StClass => GetInfo(2, StId);
        public string Mistake { get; set; }
        public long Timestamp { get; set; }
        public DateTime LoginDate => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(Timestamp).ToLocalTime();
        public int ReporterId { get; set; }

        public string RpString => GetInfo(1, ReporterId) + " - " + GetInfo(2, ReporterId);

        public int LoginStatus { get; set; }
        public string LoginColor => (LoginStatus == 1) ? "Green" : "Orange";

        public string GetInfo(int type, int Id)
        {
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            db.CreateTable<UserListForm>();
            if (type == 1) return db.Table<UserListForm>().ElementAt(Id - 4).StName;
            else return db.Table<UserListForm>().ElementAt(Id - 4).StClass;
        }
    }
}