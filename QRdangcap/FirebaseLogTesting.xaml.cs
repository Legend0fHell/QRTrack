using Firebase.Database.Query;
using Newtonsoft.Json;
using QRdangcap.GoogleDatabase;
using System;
using System.Net.Http;
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
        public static HttpClient client = new HttpClient();

        public FirebaseLogTesting()
        {
            InitializeComponent();
        }

        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            for (int i = 4; i <= 100; ++i)
            {
                await System.Threading.Tasks.Task.Delay(100);
                DependencyService.Get<IToast>().ShowShort("Đang gửi: " + i.ToString());
                instance.Firebase_SendLog(i);
                var model = new FeedbackModel()
                {
                    Mode = "6",
                    Contents = i.ToString(),
                };
                var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
                var jsonString = JsonConvert.SerializeObject(model);
                var requestContent = new StringContent(jsonString);
                _ = await client.PostAsync(uri, requestContent);
                System.Diagnostics.Debug.WriteLine($"Đã nộp: {i}");
            }
        }
    }
}