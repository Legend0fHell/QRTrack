using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using SQLite;
using Plugin.CloudFirestore;
namespace QRdangcap.LocalDatabase
{
    public class LogListFormSpec
    {
        // Contains info from GoogleDatabase, use specifically for local database.
        [PrimaryKey, Unique]
        public int LogId { get; set; }
        public int StId { get; set; }
        public int ReporterId { get; set; }
        public string Mistake { get; set; }
        public FieldValue LoginDate { get; set; }
        public int LoginStatus { get; set; }
    }
}
