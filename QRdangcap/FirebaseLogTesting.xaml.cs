using Firebase.Database;
using Firebase.Database.Query;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UserListForm2 : UserListForm
    {
        public string Password { get; set; }
        public int Score { get; set; }
    }
    public partial class FirebaseLogTesting : ContentPage
    {
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
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
                instance.Firebase_SendLog(i, Error.Substring(0, NoError.Next(0, 3)));
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            for (int i = int.Parse(From.Text); i <= int.Parse(To.Text); ++i)
            {
                await System.Threading.Tasks.Task.Delay(100);
                Sending.Text = $"Đang gửi: {i}";

                Random NoError = new Random();
                string Error = "NONE";
                if (NoError.NextDouble() <= 0.04)
                {
                    if (NoError.NextDouble() <= 0.05) Error = "Quên thẻ;Sai đồng phục";
                    else
                    {
                        if (NoError.NextDouble() <= 0.7) Error = "Quên thẻ";
                        else Error = "Sai đồng phục";
                    }
                }
                if (NoError.NextDouble() <= 0.03)
                {
                    instance.Firebase_SendLog(i, Error, false, true, false, true, DateMani.Date.AddHours(7).AddSeconds(NoError.Next(1900, 2300)));
                }
                else instance.Firebase_SendLog(i, Error, false, true, false, true, DateMani.Date.AddHours(7).AddSeconds(NoError.Next(0, 1780)));
            }
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            await fc.Child("Users").DeleteAsync();
            UserListForm2[] response = (UserListForm2[])await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "72",
            }, true, "UserListForm2[]");
            foreach (UserListForm2 i in response)
            {
                await fc.Child("Users").Child($"{i.Username}").PutAsync(new {
                    i.StId,
                    i.Username,
                    i.Password,
                    i.StName,
                    i.StClass,
                    i.Priv,
                    i.IsHidden,
                    i.Score,
                    CovidExposure = "-",
                    CovidExposureFrom = "Không"
                });
            }
        }
    }
}