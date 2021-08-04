using System;
using System.Collections.Generic;
using System.Text;

namespace QRdangcap.GoogleDatabase
{
    // There shouldn't be any "Global Variables" in C# but forgive my laziness...
    public static class UserData
    {
        public static string StudentUsername { get; set; }
        public static string StudentFullName { get; set; }
        public static string StudentClass { get; set; }
        public static int StudentPriv { get; set; }
        public static int StudentIdDatabase { get; set; }
        public static int StudentPreIdDatabase { get; set; }
        public static bool IsStudentChose { get; set; }
        public static int StudentChoseId { get; set; }
        public static double SchoolLat { get; set; }
        public static double SchoolLon { get; set; }
        public static double SchoolDist { get; set; }
        public static bool IsAtSchool { get; set; }
    }
}
