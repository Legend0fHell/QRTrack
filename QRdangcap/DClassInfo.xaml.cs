using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    internal class BarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case 1:
                    return Brush.Green;

                case 2:
                    return Brush.Orange;

                case 3:
                    return Brush.Magenta;

                default:
                    return Brush.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class DClassInfo : ContentPage
    {
        private readonly SQLiteConnection db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public ObservableRangeCollection<UserListForm> ItemsList { get; set; }
        public int globalSortStrat = -1;
        public string globalClrCheck = "";
        public bool ForcedReload { get; set; }
        public bool UpdateStat { get; set; }

        public DClassInfo(string Clr)
        {
            InitializeComponent();
            Checking();
            async void Checking()
            {
                await instance.CheckUserTableExist();
            }
            db.CreateTable<UserListForm>();
            string[] SortingMode = { "Tên", "ID" };
            globalClrCheck = Clr;
            ChoseClass.Text = Clr;
            FilterMode.ItemsSource = new List<string>() { "Tất cả", "HS Chưa ĐD", "HS Đúng giờ", "HS Muộn giờ", "HS Báo nghỉ" };
            FilterMode.SelectedIndex = 0;
            SortMode.ItemsSource = SortingMode.ToList();
            SortMode.SelectedIndex = 1;
            UpdateStat = true;
            RefreshAll.IsRefreshing = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshAll.IsRefreshing = true;
        }

        private async void ClrList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is UserListForm UsrUserChose)
            {
                var ChoosePage = new DUserInfo(UsrUserChose);
                await Navigation.PushAsync(ChoosePage);
            }
            ClrList.SelectedItem = null;
        }

        public void UpdateSumLog(int SortStrat)
        {
            if (SortStrat == globalSortStrat && !ForcedReload)
            {
                RefreshAll.IsRefreshing = false;
                return;
            }
            ForcedReload = false;
            ItemsList = new ObservableRangeCollection<UserListForm>();
            int DesiredStat = FilterMode.SelectedIndex - 1;
            if (SortStrat == 0)
            {
                ItemsList.AddRange(db.Table<UserListForm>().ToList().Where(x => x.StClass.Equals(globalClrCheck) && (DesiredStat <= -1 || x.LogStatus == DesiredStat) && x.IsHidden == 0)
                    .OrderBy(x => x.StName));
            }
            else
            {
                ItemsList.AddRange(db.Table<UserListForm>().ToList().Where(x => x.StClass.Equals(globalClrCheck) && (DesiredStat <= -1 || x.LogStatus == DesiredStat) && x.IsHidden == 0)
                    .OrderBy(x => x.StId));
            }
            globalSortStrat = SortStrat;
            ClrList.ItemsSource = ItemsList;
            RefreshAll.IsRefreshing = false;
        }

        private async void RefreshAll_Refreshing(object sender, EventArgs e)
        {
            if (UpdateStat) await instance.GetGlobalLogStat();
            UpdateStat = false;
            ForcedReload = true;
            UpdateSumLog(SortMode.SelectedIndex);
        }

        private void SortMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateStat = false;
            RefreshAll.IsRefreshing = true;
        }

        private void LoginStatUpdate_Clicked(object sender, EventArgs e)
        {
            UpdateStat = true;
            RefreshAll.IsRefreshing = true;
        }

        private void FilterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (globalSortStrat != -1)
            {
                UpdateStat = false;
                ForcedReload = true;
                RefreshAll.IsRefreshing = true;
            }
        }
    }
}