using Newtonsoft.Json;
using QRdangcap.GoogleDatabase;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class LoggingStatsResp
    {
        public int CurLogCnt { get; set; }
    }

    public class LoggingStatsBase
    {
        public string IdentityId { get; set; }
        public string Id2Id { get; set; }
        public string Id3Id { get; set; }
        public int StId { get; set; }
        public int ReporterId { get; set; }
        public string Mistake { get; set; }
        public int LoginStatus { get; set; }
    }

    public class InboundLog : LoggingStatsBase
    {
        public long Timestamp { get; set; }
        public string Keys { get; set; }
    }

    public class OutboundLog : LoggingStatsBase
    {
        [JsonProperty("Timestamp")]
        public ServerTimeStamp TimestampPlaceholder { get; } = new ServerTimeStamp();
    }

    public class ServerTimeStamp
    {
        [JsonProperty(".sv")]
        public string TimestampPlaceholder { get; } = "timestamp";
    }

    public partial class FirebaseLogTesting : ContentPage
    {
        public RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public FirebaseLogTesting()
        {
            InitializeComponent();
        }

        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            for (int i = int.Parse(From.Text); i <= int.Parse(To.Text); ++i)
            {
                await System.Threading.Tasks.Task.Delay(100);
                Sending.Text = $"Đang gửi: {i}";
                instance.Firebase_SendLog(i);
            }
        }
    }
}