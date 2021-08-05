using Plugin.CloudFirestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QRdangcap.LocalDatabase;
using QRdangcap.GoogleDatabase;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class LoggingStatsResp {
        public int CurLogCnt { get; set; }
    }
    
    public class LoggingStatsBase
    {
        public int StId { get; set; }
        public int ReporterId { get; set; }
        public string Mistake { get; set; }
        public int LoginStatus { get; set; }
    }
    public class InboundLog : LoggingStatsBase
    {
        public long Timestamp { get; set; }
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
        public FirebaseLogTesting()
        {
            InitializeComponent();
        }
        private async void Button_Clicked(object sender, EventArgs e)
        {
            var logStatRef = CrossCloudFirestore.Current.Instance.Collection("logging-stats").Document("All");
            var curLogIdDoc = await logStatRef.GetAsync();
            LoggingStatsResp curLogId = curLogIdDoc.ToObject<LoggingStatsResp>();
            var insRef = CrossCloudFirestore.Current.Instance.Collection("logging")
                .Document((curLogId.CurLogCnt + 1).ToString());
            await CrossCloudFirestore.Current.Instance
                .Batch()
                .Set(insRef, new LogListFormSpec()
                {
                    LoginDate = FieldValue.ServerTimestamp,
                    StId = UserData.StudentIdDatabase,
                    ReporterId = UserData.StudentIdDatabase,
                    Mistake = "NONE",
                    LoginStatus = 1,
                })
                .Update(logStatRef, "CurLogCnt", curLogId.CurLogCnt + 1)
                .CommitAsync();
            System.Diagnostics.Debug.WriteLine("RESULT: Add complete");
        }
        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
            var lmao = new OutboundLog
            {
                ReporterId = UserData.StudentIdDatabase,
                Mistake = "NONE",
                LoginStatus = 1,
            };
            var keyy = await fc
                .Child("Logging")
                .PostAsync(lmao);
            System.Diagnostics.Debug.WriteLine($"Key generated: {keyy.Key}");
            InboundLog curLog = await fc.Child("Logging").Child(keyy.Key).OnceSingleAsync<InboundLog>();
            DateTime CurDateTime = new DateTime(curLog.Timestamp, DateTimeKind.Local);
            TimeSpan CurTime = new TimeSpan(CurDateTime.Hour, CurDateTime.Minute, CurDateTime.Second);
            if(CurTime >= UserData.StartTime && CurTime <= UserData.EndTime)
            {
                if(CurTime > UserData.LateTime)
                {
                    curLog.LoginStatus = 2;
                    await fc.Child("Logging").Child(keyy.Key).PutAsync(curLog);
                }
            }
        }
    }
}