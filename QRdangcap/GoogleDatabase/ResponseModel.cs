using System;
using System.Collections.Generic;
using System.Text;

namespace QRdangcap.GoogleDatabase
{
    public class ResponseModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int Message1 { get; set; }
        public int Message2 { get; set; }
        public int Message3 { get; set; }
        public string STName { get; set; }
        public string STClass { get; set; }
        public int STPriv { get; set; }
        public int STId { get; set; }
        public DateTimeOffset DateTimeMessage { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan LateTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Distance { get; set; }
    }
}
