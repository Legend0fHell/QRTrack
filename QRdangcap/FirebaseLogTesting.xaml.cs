using Newtonsoft.Json;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
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
        public int StId { get; set; }
        public int ReporterId { get; set; }
        public string Mistake { get; set; }
        public int NoMistake { get; set; }
        public string StClass { get; set; }
        public int LoginStatus { get; set; }
    }

    public class InboundLog : LoggingStatsBase
    {
        [Id]
        public string Keys { get; set; }
        [ServerTimestamp(CanReplace = false)]
        public Timestamp Timestamp { get; set; }
        
    }

    public partial class FirebaseLogTesting : ContentPage
    {
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();

        public FirebaseLogTesting()
        {
            InitializeComponent();
        }

        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            string Error = ";;;;;;;";
            for (int i = int.Parse(From.Text); i <= int.Parse(To.Text); ++i)
            {
                await System.Threading.Tasks.Task.Delay(100);
                Sending.Text = $"Đang gửi: {i}";

                Random NoError = new Random();
                instance.Firebase_SendLog(i, Error.Substring(0, NoError.Next(0,3)));
            }
        }
    }
}