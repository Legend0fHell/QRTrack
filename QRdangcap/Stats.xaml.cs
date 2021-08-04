using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using QRdangcap.GoogleDatabase;
using ZXing.Net.Mobile.Forms;
using System.Globalization;
using QRdangcap.LocalDatabase;
using SQLite;
using System.Diagnostics;
using Xamarin.CommunityToolkit.ObjectModel;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Stats : ContentPage
    {
        public int firstTake = 100;
        public int skip = 100;
        public int intervalSkip = 60;
        public int queryStat = -1;
        public List<LogListForm> resLogList = new List<LogListForm>();
        public ObservableRangeCollection<LogListForm> PrntLogList { get; set; }
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localDatabasePath);
        public Stats()
        {
            InitializeComponent();
            db.CreateTable<LogListForm>();
            PrntLogList = new ObservableRangeCollection<LogListForm>(db.Table<LogListForm>().Reverse().ToList().Take(firstTake));
            LogList.RemainingItemsThreshold = 5;
            LogList.RemainingItemsThresholdReached += LogList_RemainingItemsThresholdReached;
            refreshAll.IsRefreshing = true;
        }
        private async void List_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is LogListForm logIdChose)) return;
            await Navigation.PushAsync(new LogChanger(logIdChose));
            LogList.SelectedItem = null;
            // OnAppearing reloading
        }

        private void LogList_RemainingItemsThresholdReached(object sender, EventArgs e)
        {
            if (resLogList == null)
            {
                resLogList = db.Table<LogListForm>().Reverse().ToList();
            }
            if (queryStat == -1)
            {
                resLogList.AddRange(db.Table<LogListForm>().Reverse().ToList().Skip(skip).Take(intervalSkip));
                skip += intervalSkip;
            }
            int numEntry = resLogList.Count();
            if (numEntry > 0)
            {
                if (skip > numEntry) return;
                PrntLogList.AddRange(resLogList.Skip(skip).Take(intervalSkip));
                skip += intervalSkip;
            }
        }
        private void RetrieveLog_Clicked(object sender, EventArgs e)
        {
            refreshAll.IsRefreshing = true;
        }

        private async void RetrieveLogs()
        {
            
            LogList.ItemsSource = "";
            var client = new HttpClient();
            var model = new FeedbackModel()
            {
                Mode = "3",
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var result = await client.PostAsync(uri, requestContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<LogListForm[]>(resultContent);
            if (response[0].LogId != -1)
            {
                long size = new FileInfo(GlobalVariables.localDatabasePath).Length;
                db.CreateTable<LogListForm>();
                db.DeleteAll<LogListForm>();
                db.InsertAll(response);
                size /= 1024;
                RetrieveLog.Text = response.Length + " mục (" + size + "KB), nhấn để tải lại!";
                resLogList = db.Table<LogListForm>().Reverse().ToList();
                PrntLogList.Clear();
                PrntLogList.AddRange(resLogList.Take(firstTake));
                skip = firstTake;
                queryStat = 1;
                LogList.ItemsSource = PrntLogList;
                
            }
            else RetrieveLog.Text = "Không có dữ liệu! Nhấn để tải lại!";

            refreshAll.IsRefreshing = false;
        }
        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            RetrieveLogs();

        }
    }
}