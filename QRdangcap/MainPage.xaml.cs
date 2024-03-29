﻿using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace QRdangcap
{
    public partial class MainPage : ContentPage
    {
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public ObservableCollection<UserListForm> Leaderboard { get; set; }
        public int LoadState = 0;
        public MainPage()
        {
            InitializeComponent();
            Wishes.Text = "Chúc bạn có một ngày vui vẻ!";
            DeviceDate.Text = DateTime.Now.ToString("dddd, dd.MM.yyyy", CultureInfo.CreateSpecificCulture("vi-VN"));
            VerText.Text = GlobalVariables.ClientVersion + " (" + GlobalVariables.ClientVersionDate.ToString("dd.MM") + ")";
            RefreshingView.IsRefreshing = true;
            
            MessagingCenter.Subscribe<Application, int>(this, "LoadCompletion", (p, comp) =>
            {
                LoadState++;
                InitStaticText();
                if(LoadState == 2) RefreshingView.IsRefreshing = false;
            });
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
            UserRankingPointLbl.Text = UserData.UserRankingPoint.ToString();
            UserRankingLbl.Text = UserData.UserRanking.ToString() + "/" + UserData.NoUserRanked;
            SQLiteConnection db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            Leaderboard = new ObservableCollection<UserListForm>(db.Table<UserListForm>().ToList().OrderByDescending(x => x.RankingPoint).Where(x => x.IsHidden == 0).Take(10));
            LeaderboardView.ItemsSource = Leaderboard;
            db.Dispose();
            if (UserData.StudentPriv == 0) Priv.Text = "Học sinh";
            else if (UserData.StudentPriv == 1) Priv.Text = "Xung kích";
            else if (UserData.StudentPriv == 2) Priv.Text = "Giáo viên";
            else if (UserData.StudentPriv == 3) Priv.Text = "Quản trị viên";
            else Priv.Text = "Quản trị HT";
            IsHiddenOrNot.Text = "";
            LoginToday.IsVisible = !UserData.IsTodayOff;
            PersonalRanking.IsVisible = true;
            if (UserData.IsHidden)
            {
                IsHiddenOrNot.Text = "Người dùng ẩn";
                LoginToday.IsVisible = false;
                PersonalRanking.IsVisible = false;
            }
            if (UserData.StudentPriv > 1)
            {
                AdminTab.IsVisible = true;
            }
            else
            {
                AdminTab.IsVisible = false;
            }

            if (UserData.IsUserLogin == 0)
            {
                LblStatusToday.Text = "Bạn chưa điểm danh ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Black;
                LblStatusSubString.Text = "Nhấn để điểm danh!";
            }
            else if (UserData.IsUserLogin == 1)
            {
                LblStatusToday.Text = "Bạn đã điểm danh ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Green;
                LblStatusSubString.Text = "Đã điểm danh đúng giờ";
            }
            else if (UserData.IsUserLogin == 2)
            {
                LblStatusToday.Text = "Bạn đã điểm danh ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Orange;
                LblStatusSubString.Text = "Đã điểm danh muộn giờ";
            }
            else if (UserData.IsUserLogin == 3)
            {
                LblStatusToday.Text = "Bạn đã báo nghỉ ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Magenta;
                LblStatusSubString.Text = "";
            }
            
        }

        private async void Logout_Clicked(object sender, EventArgs e)
        {
            if (await DisplayActionSheet("Bạn có chắc chắn muốn đăng xuất không?", "Có", "Không") == "Có")
            {
                Preferences.Clear();
                LoadState = 0;
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}", true);
                UserData.StudentPreIdDatabase = UserData.StudentIdDatabase;
            }
        }

        public void Button_Clicked(object sender, System.EventArgs e)
        {
            if (UserData.IsUserLogin > 0)
            {
                DisplayAlert("Điểm danh", "Điểm danh thất bại! Bạn đã điểm danh trước đó!", "OK");
            }
            else Scanner();
        }

        public async void Scanner()
        {
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
            {
                PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE },
                AutoRotate = false
            };
            var ScanView = new ZXingScannerPage(options);
            string QRRandCode = "";
            ScanView.OnScanResult += (result) =>
            {
                ScanView.IsScanning = false;
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopAsync();
                    bool sepDetect = false, invalidDetect = false;
                    string decodedQRCode = "";

                    decodedQRCode = instance.Base64Decode(result.Text);
                    if (decodedQRCode.Length != 11) invalidDetect = true;
                    else
                    {
                        for (int i = 0; i < decodedQRCode.Length; ++i)
                        {
                            if (decodedQRCode[i] == '_')
                            {
                                if (sepDetect)
                                {
                                    invalidDetect = true;
                                    break;
                                }
                                else sepDetect = true;
                                continue;
                            }
                            else if (decodedQRCode[i] < 48 || (decodedQRCode[i] > 57 && decodedQRCode[i] < 65) || (decodedQRCode[i] > 90))
                            {
                                invalidDetect = true;
                                break;
                            }
                            if (sepDetect) QRRandCode += decodedQRCode[i];
                        }
                    }
                    if (!sepDetect || invalidDetect)
                    {
                        DisplayAlert("Điểm danh thất bại!", "Code QR không hợp lệ!", "OK");
                    }
                    else if ((!UserData.IsAtSchool && GlobalVariables.IsGPSRequired)
                        || (UserData.IsAtSchool && DateTime.Now >= UserData.LastGPSUpdate.AddMinutes(5) && GlobalVariables.IsGPSRequired))
                    {
                        LocationTmpUpdating();
                        async void LocationTmpUpdating()
                        {
                            if (!DependencyService.Get<IGpsDependencyService>().IsGpsEnable())
                            {
                                await DisplayAlert("Thông báo", "GPS chưa được bật. Nhấn OK để kích hoạt GPS trước khi sử dụng.", "OK");
                                DependencyService.Get<IGpsDependencyService>().OpenSettings();
                                return;
                            }
                            else
                            {
                                await instance.UpdateCurLocation();
                            }
                            if (UserData.IsLastTimeMock)
                            {
                                await DisplayAlert("Điểm danh thất bại!", "Phát hiện GPS đang bị làm giả!", "OK");
                            }
                            else
                            {
                                if (!UserData.IsAtSchool) await DisplayAlert("Điểm danh thất bại!", "Bạn đang ở ngoài trường! (nếu hệ thống sai, hãy thử lại)", "OK");
                                else SendData();
                            }
                        }
                    }
                    else
                    {
                        SendData();
                    }
                });
            };
            async void SendData()
            {
                ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
                {
                    Mode = "2",
                    Contents = QRRandCode,
                    Contents2 = UserData.StudentIdDatabase.ToString(),
                });
                string Reason = "";
                if (response.Message1 == 1)
                {
                    Reason = "Bạn đã điểm danh đúng giờ!";
                    UserData.IsUserLogin = 1;
                }
                else if (response.Message1 == 2)
                {
                    Reason = "Bạn đã điểm danh muộn!";
                    UserData.IsUserLogin = 2;
                }
                else if (response.Message1 == 0) Reason = "Code QR đã cũ hoặc không hợp lệ!";
                else if (response.Message1 == -1) Reason = "Bạn điểm danh ngoài khoảng thời gian quy định!";
                else Reason = "Bạn đã điểm danh trước đó!";
                if (response.Status == "SUCCESS")
                {
                    instance.Firebase_SendLog(UserData.StudentIdDatabase, "NONE", false, false);
                    await instance.GetGlobalUserRanking();
                    await DisplayAlert("Điểm danh thành công!", Reason, "OK");
                    RefreshingView.IsRefreshing = true;
                }
                else
                {
                    await DisplayAlert("Điểm danh thất bại!", Reason, "OK");
                }
            }
            await Navigation.PushAsync(ScanView);
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            InitStaticText();
            RefreshingView.IsRefreshing = false;
        }

        private async void UpdateUser_Tapped(object sender, EventArgs e)
        {
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            await instance.RetrieveAllUserDatabase();
        }

        private async void C00_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserCard());
        }

        private async void C01_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TimetablePage());
        }

        private async void C02_Tapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//main/QueryInfo", true);
        }

        private async void C10_Tapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//main/RealStats", true);
        }

        private async void C11_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LeaderboardFull());
        }

        private async void C12_Tapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//main/Other", true);
        }

        private async void C20_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GenQR());
        }

        private async void C21_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SendAbsent());
        }

        private async void C22_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReportGen());
        }

        private async void C30_Tapped(object sender, EventArgs e)
        {
            if (UserData.StudentPriv > 2) await Navigation.PushAsync(new RestDay());
            else DependencyService.Get<IToast>().ShowShort("Chức năng bị khóa.");
        }

        private async void C31_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Stats());
        }

        private void C32_Tapped(object sender, EventArgs e)
        {
            DependencyService.Get<IToast>().ShowShort("Chức năng sắp ra mắt");
        }

        public async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LeaderboardFull());
        }
    }
}