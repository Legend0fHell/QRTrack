using System;
using System.IO;

namespace QRdangcap.DatabaseModel
{
    // There shouldn't be any "Global Variables" in C# but forgive my laziness...
    public static class GlobalVariables
    {
        public static string ClientVersion = "D041";
        public static DateTime ClientVersionDate = new DateTime(2021, 11, 13, 11, 30, 27);
        public static string localLogHistDatabasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "LocalLogHistDatabase.db3");
        public static string localUserDatabasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "UserDatabase.db3");
        public static string FirebaseURL = "https://qrdangcap-default-rtdb.asia-southeast1.firebasedatabase.app/";
        public static bool IsGPSRequired { get; set; }
    }
}