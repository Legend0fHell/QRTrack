using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QueryInfo : ContentPage
    {
        public ObservableRangeCollection<UserListForm> ItemsList { get; set; }
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
        public List<UserListForm> filteredItem = new List<UserListForm>();
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public int queryStat = -1;
        public bool UpdateStat { get; set; }
        private readonly Stopwatch SinceLastQuery = new Stopwatch();
        public bool Locked { get; set; }
        public QueryInfo()
        {
            InitializeComponent();
            SinceLastQuery.Start();
            Locked = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (SinceLastQuery.IsRunning && SinceLastQuery.ElapsedMilliseconds > 300)
                    {
                        SinceLastQuery.Stop();
                        if (!Locked) UpdateQuery();
                    }
                });
                return true;
            });
            Checking();
            async void Checking()
            {
                await instance.CheckUserTableExist();
            }
            UpdateStat = true;
            refreshAll.IsRefreshing = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            refreshAll.IsRefreshing = true;
        }

        protected override bool OnBackButtonPressed()
        {
            if (Navigation.NavigationStack.Count == 1)
            {
                _ = Shell.Current.GoToAsync($"//main/MainPage", true);
                return true;
            }
            else return base.OnBackButtonPressed();
        }

        public void Init()
        {
            Locked = true;
            ItemsList = new ObservableRangeCollection<UserListForm>();
            ItemsList.Clear();
            ItemsList.AddRange(db.Table<UserListForm>().ToList().Where(x => x.IsHidden == 0));
            myCollectionView.ItemsSource = ItemsList;
            Locked = false;
            BindingContext = this;
            refreshAll.IsRefreshing = false;
        }

        private void Test_TextChanged(object sender, TextChangedEventArgs e)
        {
            SinceLastQuery.Restart();
        }
        private void UpdateQuery()
        {
            string searchTerm = NameQuery.Text;
            string classSearchTerm = ClassQuery.Text;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(classSearchTerm))
            {
                classSearchTerm = string.Empty;
            }
            searchTerm = instance.ConvertToUnsign(searchTerm).ToLower();
            classSearchTerm = instance.ConvertToUnsign(classSearchTerm).ToLower();

            List<UserListForm> UserList = new List<UserListForm>(db.Table<UserListForm>().ToList().Where(x => x.IsHidden == 0));
            filteredItem = UserList;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredItem = UserList.Where(x => x.UnsignStName.Contains(searchTerm)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(classSearchTerm))
            {
                filteredItem = filteredItem.Where(x => x.UnsignStClass.Contains(classSearchTerm)).ToList();
            }
            queryStat = 0;
            if (filteredItem.Count > 0) queryStat = 1;
            ItemsList.Clear();
            ItemsList.AddRange(filteredItem);
        }
        private async void RefreshView_Refreshing(object sender, EventArgs e)
        {
            if (UpdateStat)
            {
                await instance.GetGlobalLogStat();
            }
            Init();
            UpdateQuery();
            UpdateStat = false;
        }

        private async void MyCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is UserListForm userIdChose)) return;
            MessagingCenter.Send<Page, UserListForm>(this, "ChoseSTId", userIdChose);
            if (Navigation.NavigationStack.Count > 1) await Navigation.PopAsync();
            else await Navigation.PushAsync(new DUserInfo(userIdChose));
            myCollectionView.SelectedItem = null;
        }

        private void LoginStatUpdate_Clicked(object sender, EventArgs e)
        {
            UserData.ForcedStatusUpdate = true;
            UpdateStat = true;
            refreshAll.IsRefreshing = true;
        }
    }
}