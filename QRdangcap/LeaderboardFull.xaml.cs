using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LeaderboardFull : ContentPage
    {
        public ObservableCollection<UserListForm> Leaderboard { get; set; }
        public RetrieveAllUserDb instance = new RetrieveAllUserDb();

        public LeaderboardFull()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            refreshAll.IsRefreshing = true;
        }

        public void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            refreshAll.IsRefreshing = true;
        }

        private async void RefreshAll_Refreshing(object sender, EventArgs e)
        {
            UserData.NoUserRanked = await instance.GetGlobalUserRanking();
            SQLiteConnection db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            Leaderboard = new ObservableCollection<UserListForm>(db.Table<UserListForm>().ToList().OrderByDescending(x => x.RankingPoint));
            LeaderboardView.ItemsSource = Leaderboard;
            UserRankingPointLbl.Text = UserData.UserRankingPoint.ToString();
            UserRankingLbl.Text = UserData.UserRanking.ToString() + "/" + UserData.NoUserRanked;
            db.Dispose();
            refreshAll.IsRefreshing = false;
        }
    }
}