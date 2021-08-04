using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.IO;

namespace QRdangcap.LocalDatabase
{
    // There shouldn't be any "Global Variables" in C# but forgive my laziness...
    public static class GlobalVariables
    {
        public static string ClientVersion = "A025";
        public static DateTime ClientVersionDate = new DateTime(2021, 8, 2, 23, 42, 10);
        public static string localDatabasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "LogListDatabase.db3");
        public static string localLogHistDatabasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "LocalLogHistDatabase.db3");
        public static string localUserDatabasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "UserDatabase.db3");
        public static bool IsGPSRequired { get; set; }
    }
}
