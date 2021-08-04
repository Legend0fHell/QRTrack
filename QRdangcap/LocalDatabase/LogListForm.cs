using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using SQLite;

namespace QRdangcap.LocalDatabase
{
    public class LogListForm
    {
        // Contains info from GoogleDatabase, use specifically for local database.
        [PrimaryKey, Unique]
        public int LogId { get; set; }
        public int StId { get; set; }
        public string StName => GetInfo(1, StId);
        public string StClass => GetInfo(2, StId);
        public int Priv => GetInfoInt(StId);
        public int ReporterId { get; set; }
        public string RpString => GetInfo(1, ReporterId) + " - " + GetInfo(2, ReporterId);
        public string Mistake { get; set; }
        public DateTimeOffset LoginDate { get; set; }
        public string LoginTimeFromDate => LoginDate.ToLocalTime().ToString("HH:mm:ss");
        public string LoginDateFromDate => LoginDate.ToLocalTime().ToString("ddd, dd.MM", CultureInfo.CreateSpecificCulture("vi-VN"));
        public int LoginStatus { get; set; }
        public string LoginColor => (LoginStatus == 1) ? "Green" : "Orange";
        public string GetInfo(int type, int Id)
        {
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            db.CreateTable<UserListForm>();
            if (type == 1) return db.Table<UserListForm>().ElementAt(Id - 4).StName;
            else return db.Table<UserListForm>().ElementAt(Id - 4).StClass;
        }
        public int GetInfoInt(int Id)
        {
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            db.CreateTable<UserListForm>();
            return db.Table<UserListForm>().ElementAt(Id - 4).Priv;
        }
    }
}
