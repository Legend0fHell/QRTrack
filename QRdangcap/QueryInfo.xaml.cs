using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        public int firstTake = 50;
        public int skip = 50;
        public int intervalSkip = 30;
        public int queryStat = -1;
        public bool ForcedReload { get; set; }
        public QueryInfo()
        {
            InitializeComponent();
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            instance.CheckUserTableExist();
            refreshAll.IsRefreshing = true;
        }

        public void Init()
        {
            ItemsList = new ObservableRangeCollection<UserListForm>();
            ItemsList.Clear();
            ItemsList.AddRange(db.Table<UserListForm>().ToList().Take(firstTake));
            myCollectionView.ItemsSource = ItemsList;
            myCollectionView.RemainingItemsThreshold = 5;
            myCollectionView.RemainingItemsThresholdReached += MyCollectionView_RemainingItemsThresholdReached;
            BindingContext = this;
            refreshAll.IsRefreshing = false;
        }

        private void MyCollectionView_RemainingItemsThresholdReached(object sender, EventArgs e)
        {
            if (filteredItem == null)
            {
                filteredItem = db.Table<UserListForm>().ToList();
            }
            if (queryStat == -1)
            {
                ItemsList.AddRange(db.Table<UserListForm>().ToList().Skip(skip).Take(intervalSkip));
                skip += intervalSkip;
            }
            int numEntry = filteredItem.Count();
            if (numEntry > 0)
            {
                if (skip > numEntry) return;
                ItemsList.AddRange(filteredItem.Skip(skip).Take(intervalSkip));
                skip += intervalSkip;
            }
        }

        private void Test_TextChanged(object sender, TextChangedEventArgs e)
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
            searchTerm = searchTerm.ToLower(CultureInfo.CreateSpecificCulture("vi-VN"));
            classSearchTerm = classSearchTerm.ToLower(CultureInfo.CreateSpecificCulture("vi-VN"));

            List<UserListForm> UserList = new List<UserListForm>(db.Table<UserListForm>().ToList());
            filteredItem = UserList;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredItem = UserList.Where(x => x.StName.ToLower(CultureInfo.CreateSpecificCulture("vi-VN")).Contains(searchTerm)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(classSearchTerm))
            {
                filteredItem = filteredItem.Where(x => x.StClass.ToLower(CultureInfo.CreateSpecificCulture("vi-VN")).Contains(classSearchTerm)).ToList();
            }
            queryStat = 0;
            if (filteredItem.Count > 0) queryStat = 1;
            ItemsList.Clear();

            ItemsList.AddRange(filteredItem.Take(firstTake));
            skip = firstTake;
            MyCollectionView_RemainingItemsThresholdReached(sender, e);
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            Init();
        }

        private async void MyCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is UserListForm userIdChose)) return;
            MessagingCenter.Send<Page, UserListForm>(this, "ChoseSTId", userIdChose);
            if (Navigation.NavigationStack.Count > 1) await Navigation.PopAsync();
            else
            {
                var ChoosePage = new DUserInfo(userIdChose);
                await Navigation.PushAsync(ChoosePage);
            }
            myCollectionView.SelectedItem = null;
        }
        private async void LoginStatUpdate_Clicked(object sender, EventArgs e)
        {
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            await instance.GetGlobalLogStat();
            ForcedReload = true;
            refreshAll.IsRefreshing = true;
        }
    }
}