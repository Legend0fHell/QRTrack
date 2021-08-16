using SQLite;
using System;

namespace QRdangcap.DatabaseModel
{
    public class AbsentLogForm
    {
        public int LogId { get; set; }
        public int StId { get; set; }
        public string StName => GetInfo(1, StId);
        public string StClass => GetInfo(2, StId);
        public int ContentStartDate { get; set; }
        public DateTime DateCSD => new DateTime(DateTime.Now.Year - 1, 12, 31).AddDays(ContentStartDate);
        public string StringCSD => DateCSD.ToString("dd.MM.yyyy");
        public int ContentEndDate { get; set; }
        public DateTime DateCED => new DateTime(DateTime.Now.Year - 1, 12, 31).AddDays(ContentEndDate);
        public string StringCED => DateCED.ToString("dd.MM.yyyy");
        public int ReporterId { get; set; }
        public string RpString => GetInfo(1, ReporterId) + " - " + GetInfo(2, ReporterId);

        public string GetInfo(int type, int Id)
        {
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            db.CreateTable<UserListForm>();
            if (type == 1) return db.Table<UserListForm>().ElementAt(Id - 4).StName;
            else return db.Table<UserListForm>().ElementAt(Id - 4).StClass;
        }
    }
}