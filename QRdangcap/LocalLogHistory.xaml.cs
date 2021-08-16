using QRdangcap.DatabaseModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocalLogHistory : ContentPage
    {
        public int firstTake = 100;
        public int skip = 100;
        public int intervalSkip = 60;
        public int queryStat = -1;
        public List<LogListForm> resLogList = new List<LogListForm>();
        public ObservableRangeCollection<LogListForm> PrntLogList { get; set; }
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localLogHistDatabasePath);

        public LocalLogHistory()
        {
            InitializeComponent();
            db.CreateTable<LogListForm>();
            refreshAll.IsRefreshing = true;
            PrntLogList = new ObservableRangeCollection<LogListForm>(db.Table<LogListForm>().Reverse().ToList().Take(firstTake));
            LogList.ItemsSource = PrntLogList;
            LogList.RemainingItemsThreshold = 5;
            LogList.RemainingItemsThresholdReached += LogList_RemainingItemsThresholdReached;
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

        private async void List_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is LogListForm logIdChose)) return;
            await Navigation.PushAsync(new LogChanger(logIdChose));
            LogList.SelectedItem = null;
        }

        private readonly Stopwatch excTime = new Stopwatch();

        public void RetrieveTotal()
        {
            excTime.Reset();
            excTime.Start();

            LogList.ItemsSource = "";
            if (db.Table<LogListForm>().Count() > 0)
            {
                long size = new FileInfo(GlobalVariables.localLogHistDatabasePath).Length;
                size /= 1024;
                RetrieveLog.Text = db.Table<LogListForm>().Count() + " mục (" + size + "KB), nhấn để tải lại!";
                resLogList = db.Table<LogListForm>().Reverse().ToList();
                PrntLogList = new ObservableRangeCollection<LogListForm>();
                PrntLogList.AddRange(resLogList.Take(firstTake));
                skip = firstTake;
                queryStat = 1;
                LogList.ItemsSource = PrntLogList;
            }
            else RetrieveLog.Text = "Không có dữ liệu! Nhấn để tải lại!";
            excTime.Stop();
            refreshAll.IsRefreshing = false;
        }

        private void RetrieveLog_Clicked(object sender, EventArgs e)
        {
            refreshAll.IsRefreshing = true;
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            RetrieveTotal();
        }
    }
}