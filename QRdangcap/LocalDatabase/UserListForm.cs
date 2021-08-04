using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using SQLite;

namespace QRdangcap.LocalDatabase
{
    public class UserListForm
    {
        // Contains info from GoogleDatabase, use specifically for local database.
        [PrimaryKey, Unique]
        public int StId { get; set; }
        public string StName { get; set; }
        public string StClass { get; set; }
        public int Priv { get; set; }
    }
}
