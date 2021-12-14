using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace QRdangcap
{
    public partial class ParentPage : ContentPage
    {
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public ObservableCollection<UserListForm> Leaderboard { get; set; }
        public UserListForm UrChild { get; set; }

        public ParentPage()
        {
            InitializeComponent();
            Wishes.Text = "Chúc bạn có một ngày vui vẻ!";
            DeviceDate.Text = DateTime.Now.ToString("dddd, dd.MM.yyyy", CultureInfo.CreateSpecificCulture("vi-VN"));
            VerText.Text = GlobalVariables.ClientVersion + " (" + GlobalVariables.ClientVersionDate.ToString("dd.MM") + ")";
            RefreshingView.IsRefreshing = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DeviceClock.Text = DateTime.Now.ToString("HH:mm:ss");
                });
                return true;
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            InitStaticText();
            if (UserData.StudentPreIdDatabase != UserData.StudentIdDatabase)
            {
                UserData.StudentPreIdDatabase = UserData.StudentIdDatabase;
            }
        }

        private DateTime LastBack = DateTime.Now - new TimeSpan(0, 0, 5);

        protected override bool OnBackButtonPressed()
        {
            if (Navigation.NavigationStack.Count == 1)
            {
                if (DateTime.Now - LastBack <= new TimeSpan(0, 0, 5))
                {
                    return base.OnBackButtonPressed();
                }
                else
                {
                    DependencyService.Get<IToast>().ShowShort("Nhấn BACK lần nữa để thoát");
                    LastBack = DateTime.Now;
                }
            }
            return true;
        }

        public void InitStaticText()
        {
            // Limit is 14 characters.
            Greeting.Text = UserData.StudentFullName;
            Details.Text = "Lớp " + UserData.StudentClass + " - " + UserData.SchoolName;
            Priv.Text = "Phụ huynh";
            IsHiddenOrNot.Text = "";
            LoginToday.IsVisible = !UserData.IsTodayOff;
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            List<UserListForm> lmao = db.Table<UserListForm>().ToList();
            db.Dispose();
            UrChild = lmao[-1 * UserData.StudentPriv - 4];
            int ChildLogin = UrChild.LogStatus;
            if (ChildLogin == 0)
            {
                LblStatusToday.Text = $"{UrChild.StName} chưa điểm danh ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Black;
                LblStatusSubString.Text = "";
            }
            else if (ChildLogin == 1)
            {
                LblStatusToday.Text = $"{UrChild.StName} đã điểm danh ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Green;
                LblStatusSubString.Text = "Đã điểm danh đúng giờ";
            }
            else if (ChildLogin == 2)
            {
                LblStatusToday.Text = $"{UrChild.StName} đã điểm danh ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Orange;
                LblStatusSubString.Text = "Đã điểm danh muộn giờ";
            }
            else if (ChildLogin == 3)
            {
                LblStatusToday.Text = $"{UrChild.StName} đã báo nghỉ ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Magenta;
                LblStatusSubString.Text = "";
            }
            RefreshingView.IsRefreshing = false;
        }

        private async void Logout_Clicked(object sender, EventArgs e)
        {
            if (await DisplayActionSheet("Bạn có chắc chắn muốn đăng xuất không?", "Có", "Không") == "Có")
            {
                Preferences.Clear();
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}", true);
                UserData.StudentPreIdDatabase = UserData.StudentIdDatabase;
            }
        }

        private async void RefreshView_Refreshing(object sender, EventArgs e)
        {
            await instance.GetGlobalLogStat();
            InitStaticText();
        }

        private async void UpdateUser_Tapped(object sender, EventArgs e)
        {
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            await instance.RetrieveAllUserDatabase();
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DUserInfo(UrChild));
        }
    }
}