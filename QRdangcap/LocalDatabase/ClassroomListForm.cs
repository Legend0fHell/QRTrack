using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace QRdangcap.LocalDatabase
{
    public class ClassroomListForm
    {
        [PrimaryKey, Unique]
        public string ClrName { get; set; }
        public int ClrNoSt { get; set; }
        public int ClrOnTime { get; set; }
        public int ClrLateTime { get; set; }
        public int ClrAbsent { get; set; }
        public int ClrNotYet => ClrNoSt - ClrOnTime - ClrLateTime - ClrAbsent;
    }
}
