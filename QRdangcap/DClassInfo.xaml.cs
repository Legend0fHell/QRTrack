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
using QRdangcap.LocalDatabase;
using ZXing.Net.Mobile.Forms;
using System.Globalization;

using SQLite;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Xamarin.CommunityToolkit.ObjectModel;
namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    // TODO: Indicator of users logged in or not (today) || or Stats number (range of day)
    public partial class DClassInfo : ContentPage
    {
        readonly SQLiteConnection db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
        public ObservableRangeCollection<UserListForm> ItemsList { get; set; }
        public int globalSortStrat = -1;
        public string globalClrCheck = "";
        public DClassInfo(string Clr)
        {
            InitializeComponent();
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            instance.CheckUserTableExist();
            db.CreateTable<UserListForm>();
            string[] SortingMode = { "Tên", "ID" };
            globalClrCheck = Clr;
            ChoseClass.Text = Clr;
            SortMode.ItemsSource = SortingMode.ToList();
            SortMode.SelectedIndex = 0;
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
            if (SortStrat == globalSortStrat)
            {
                RefreshAll.IsRefreshing = false;
                return;
            }
            ItemsList = new ObservableRangeCollection<UserListForm>();
            if (SortStrat == 0)
            {
                ItemsList.AddRange(db.Table<UserListForm>().ToList().Where(x => x.StClass.Equals(globalClrCheck)).OrderBy(x => x.StName));
            }
            else
            {
                ItemsList.AddRange(db.Table<UserListForm>().ToList().Where(x => x.StClass.Equals(globalClrCheck)).OrderBy(x => x.StId));
            }
            globalSortStrat = SortStrat;
            ClrList.ItemsSource = ItemsList;
            RefreshAll.IsRefreshing = false;
        }
        private void RefreshAll_Refreshing(object sender, EventArgs e)
        {
            UpdateSumLog(SortMode.SelectedIndex);
        }

        private void SortMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshAll.IsRefreshing = true;
        } 
    }
}