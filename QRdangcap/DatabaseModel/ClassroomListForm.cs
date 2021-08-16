using SQLite;

namespace QRdangcap.DatabaseModel
{
    public class ClassroomListForm
    {
        [PrimaryKey, Unique]
        public string ClrName { get; set; }

        public int ClrNoSt { get; set; }
        public int ClrOnTime { get; set; }
        public int ClrLateTime { get; set; }
        public int ClrAbsent { get; set; }
        public int ClrError { get; set; }
        public int ClrNotYet => ClrNoSt - ClrOnTime - ClrLateTime - ClrAbsent;
    }
}