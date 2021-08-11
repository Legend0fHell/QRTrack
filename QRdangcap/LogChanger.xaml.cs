﻿using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using QRdangcap.GoogleDatabase;
using QRdangcap.LocalDatabase;
using SQLite;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogChanger : ContentPage
    {
        public LogListForm globalLogList = new LogListForm();
        public FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public static HttpClient client = new HttpClient();
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localLogHistDatabasePath);

        public LogChanger(LogListForm logList)
        {
            InitializeComponent();
            globalLogList = logList;
            ChoseLogId.Text = logList.Keys;
            ChoseString.Text = logList.StId.ToString() + " " + logList.StClass + " - " + logList.StName;
            StMistake.Text = logList.Mistake.Equals("NONE") ? "" : logList.Mistake;
            OnTime.IsChecked = false;
            LateTime.IsChecked = false;
            if (logList.LoginStatus == 1) OnTime.IsChecked = true;
            else if (logList.LoginStatus == 2) LateTime.IsChecked = true;
            StTime.Text = logList.LoginDate.ToString("HH:mm:ss dd.MM.yyyy");
        }

        public async void Button_Clicked_1(object sender, System.EventArgs e)
        {
            if(string.IsNullOrEmpty(StMistake.Text) || string.IsNullOrWhiteSpace(StMistake.Text))
            {
                StMistake.Text = "NONE";
            }
            InboundLog NewLog = new InboundLog()
            {
                StId = globalLogList.StId,
                Keys = globalLogList.Keys,
                ReporterId = UserData.StudentIdDatabase,
                Mistake = StMistake.Text,
                Timestamp = globalLogList.Timestamp,
                LoginStatus = OnTime.IsChecked ? 1 : 2,
                Id2Id = globalLogList.StId.ToString() + "_" + (OnTime.IsChecked ? 1 : 2).ToString(),
            };
            string QueryName = ChoseString.Text;
            await fc.Child("Logging").Child(globalLogList.Keys).PutAsync(NewLog);
            var model = new FeedbackModel()
            {
                Mode = "13",
                Contents = globalLogList.StId.ToString(),
                Contents2 = globalLogList.LoginDate.DayOfYear.ToString(),
                Contents3 = OnTime.IsChecked ? "1" : "2",
                Contents4 = globalLogList.Mistake.Equals("NONE") ? "0" : "1",
            };
            db.CreateTable<LogListForm>();
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            _ = await client.PostAsync(uri, requestContent);
            DependencyService.Get<IToast>().ShowShort("Sửa thành công: " + QueryName);

            await Navigation.PopAsync();
        }

        public async void Button_Clicked_2(object sender, System.EventArgs e)
        {
            string QueryName = ChoseString.Text;
            await fc.Child("Logging").Child(globalLogList.Keys).DeleteAsync();
            var model = new FeedbackModel()
            {
                Mode = "13",
                Contents = globalLogList.StId.ToString(),
                Contents2 = globalLogList.LoginDate.DayOfYear.ToString(),
                Contents3 = "",
                Contents4 = "",
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            _ = await client.PostAsync(uri, requestContent);
            DependencyService.Get<IToast>().ShowShort("Xóa thành công: " + QueryName);
            await Navigation.PopAsync();
        }
    }
}