using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using Syncfusion.XForms.PopupLayout;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Other : ContentPage
    {
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public string _CurAcc = "Không có thông tin!";

        public string CurAcc
        {
            get => _CurAcc;
            set
            {
                _CurAcc = value;
                OnPropertyChanged(nameof(CurAcc));
            }
        }

        public Other()
        {
            InitializeComponent();
            // OnAppearing

            ClientVerText_Detail.Text = GlobalVariables.ClientVersion + " (dựng lúc: " + GlobalVariables.ClientVersionDate.ToString("G") + ")";
            IsGPSRequired_Switch.IsToggled = true;
            BindingContext = this;
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (UserData.StudentPriv == 0)
            {
                // Hs
                UserCard.IsVisible = true;
                GenQR.IsVisible = false;
                SendAbs.IsVisible = false;
                GPSTest.IsVisible = true;
                Medical.IsVisible = false;
                TimetablePage.IsVisible = true;
                TeachSubstitute.IsVisible = true;
                RestView.IsVisible = false;
                TCClr.IsVisible = true;
                HistDB.IsVisible = false;
                HistLocal.IsVisible = true;
                ReportGene.IsVisible = false;
                UpdateDBUser.IsVisible = true;
                DelDBLocal.IsVisible = true;
                FirebaseLogTesting.IsVisible = false;
                IsGPSRequired.IsVisible = false;
            }
            else if (UserData.StudentPriv == 1)
            {
                // XK
                UserCard.IsVisible = true;
                GenQR.IsVisible = true;
                SendAbs.IsVisible = false;
                GPSTest.IsVisible = true;
                Medical.IsVisible = true;
                TimetablePage.IsVisible = true;
                TeachSubstitute.IsVisible = true;
                RestView.IsVisible = false;
                TCClr.IsVisible = true;
                HistDB.IsVisible = false;
                HistLocal.IsVisible = true;
                ReportGene.IsVisible = true;
                UpdateDBUser.IsVisible = true;
                DelDBLocal.IsVisible = true;
                FirebaseLogTesting.IsVisible = false;
                IsGPSRequired.IsVisible = false;
            }
            else if (UserData.StudentPriv == 2)
            {
                // GV
                UserCard.IsVisible = true;
                GenQR.IsVisible = true;
                SendAbs.IsVisible = true;
                GPSTest.IsVisible = true;
                Medical.IsVisible = true;
                TimetablePage.IsVisible = true;
                TeachSubstitute.IsVisible = true;
                RestView.IsVisible = false;
                TCClr.IsVisible = true;
                HistDB.IsVisible = true;
                HistLocal.IsVisible = true;
                ReportGene.IsVisible = true;
                UpdateDBUser.IsVisible = true;
                DelDBLocal.IsVisible = true;
                FirebaseLogTesting.IsVisible = false;
                IsGPSRequired.IsVisible = true;
            }
            else if (UserData.StudentPriv == 3)
            {
                // QTV
                UserCard.IsVisible = true;
                GenQR.IsVisible = true;
                SendAbs.IsVisible = true;
                GPSTest.IsVisible = true;
                Medical.IsVisible = true;
                TimetablePage.IsVisible = true;
                TeachSubstitute.IsVisible = true;
                RestView.IsVisible = true;
                TCClr.IsVisible = true;
                HistDB.IsVisible = true;
                HistLocal.IsVisible = true;
                ReportGene.IsVisible = true;
                UpdateDBUser.IsVisible = true;
                DelDBLocal.IsVisible = true;
                FirebaseLogTesting.IsVisible = false;
                IsGPSRequired.IsVisible = true;
            }
            else if (UserData.StudentPriv > 3)
            {
                // God
                UserCard.IsVisible = true;
                GenQR.IsVisible = true;
                SendAbs.IsVisible = true;
                GPSTest.IsVisible = true;
                Medical.IsVisible = true;
                TimetablePage.IsVisible = true;
                TeachSubstitute.IsVisible = true;
                RestView.IsVisible = true;
                TCClr.IsVisible = true;
                HistDB.IsVisible = true;
                HistLocal.IsVisible = true;
                ReportGene.IsVisible = true;
                UpdateDBUser.IsVisible = true;
                DelDBLocal.IsVisible = true;
                FirebaseLogTesting.IsVisible = true;
                IsGPSRequired.IsVisible = true;
            }
            CurAcc = "Tên: " + UserData.StudentFullName + ", ID: " + UserData.StudentIdDatabase.ToString();
        }

        private async void GenQR_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GenQR());
        }

        private async void TCClr_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DSchoolInfo(DateTime.Now.DayOfYear, DateTime.Now.DayOfYear));
        }

        private async void HistDB_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Stats());
        }

        private async void HistLocal_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LocalLogHistory());
        }

        private async void RestView_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RestView());
        }

        private async void UpdateDBUser_Tapped(object sender, EventArgs e)
        {
            DependencyService.Get<IToast>().ShowShort("Đang cập nhật dữ liệu...");
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            await instance.RetrieveAllUserDatabase();
            await instance.GetGlobalLogStat();
            await instance.GetGlobalUserRanking();
            int retry = 1;
            while (retry < 5)
            {
                try
                {
                    ResponseModel response2 = await fc.Child("SchoolCfg").OnceSingleAsync<ResponseModel>();
                    UserData.SchoolLat = response2.Latitude;
                    UserData.SchoolLon = response2.Longitude;
                    UserData.SchoolDist = response2.Distance;
                    UserData.StartTime = TimeSpan.FromSeconds(response2.Message1);
                    UserData.EndTime = TimeSpan.FromSeconds(response2.Message3);
                    UserData.LateTime = TimeSpan.FromSeconds(response2.Message2);
                }
                catch (Exception ex)
                {
                    ++retry;
                    DependencyService.Get<IToast>().ShowShort($"Lỗi: Không thể kết nối với csdl. Đang thử lại {retry}/5");
                    Debug.WriteLine($"Khởi tạo lỗi ({ex.Message}):, thử lại {retry}/5");
                    await Task.Delay(retry * 1000);
                    continue;
                }
                break;
            }
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
            DependencyService.Get<IToast>().ShowShort("Cập nhật thành công.");
        }

        private async void Logout_Tapped(object sender, EventArgs e)
        {
            if (await DisplayActionSheet("Bạn có chắc chắn muốn đăng xuất không?", "Có", "Không") == "Có")
            {
                Preferences.Clear();
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}", true);
                UserData.StudentPreIdDatabase = UserData.StudentIdDatabase;
            }
        }

        private void UpdateCheck_Tapped(object sender, EventArgs e)
        {
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            instance.CheckUpdates();
        }

        private void ChangePassword_Tapped(object sender, EventArgs e)
        {
            //string NewPassword = await DisplayPromptAsync("Nhập mật khẩu mới:", "Chỉ là PoC, mọi thứ có thể thay đổi.");
            SfPopupLayout OverwritePopup = new SfPopupLayout();
            OverwritePopup.PopupView.HeaderTitle = "Đổi mật khẩu";
            OverwritePopup.PopupView.AppearanceMode = AppearanceMode.TwoButton;
            OverwritePopup.PopupView.AcceptButtonText = "Xác nhận";
            OverwritePopup.PopupView.DeclineButtonText = "Hủy";
            OverwritePopup.PopupView.AnimationMode = AnimationMode.Fade;
            OverwritePopup.PopupView.PopupStyle.OverlayColor = Color.Black;
            OverwritePopup.PopupView.PopupStyle.OverlayOpacity = 0.35;
            OverwritePopup.BackgroundColor = new Color(230, 230, 230);
            OverwritePopup.PopupView.HeightRequest = 320;
            Entry InitPass = new Entry { Placeholder = "Nhập mật khẩu cũ...", HorizontalOptions = LayoutOptions.FillAndExpand, IsPassword = true };
            Entry NewPass1 = new Entry { Placeholder = "Nhập mật khẩu mới...", HorizontalOptions = LayoutOptions.FillAndExpand, IsPassword = true };
            Entry NewPass2 = new Entry { Placeholder = "Nhập lại mật khẩu mới...", HorizontalOptions = LayoutOptions.FillAndExpand, IsPassword = true };
            DataTemplate contentTemplateView = new DataTemplate(() =>
            {
                StackLayout popupContent = new StackLayout()
                {
                    Margin = new Thickness(10, 10),
                    Children =
                    {
                        new Label {Text = "Nhập mật khẩu cũ:", Margin = new Thickness(10,0,0,-10)},
                        InitPass,
                        new Label {Text = "Nhập mật khẩu mới:", Margin = new Thickness(10,0,0,-10)},
                        NewPass1,
                        new Label {Text = "Nhập lại mật khẩu mới:", Margin = new Thickness(10,0,0,-10)},
                        NewPass2,
                    }
                };
                return popupContent;
            });
            OverwritePopup.PopupView.ContentTemplate = contentTemplateView;
            OverwritePopup.ClosePopupOnBackButtonPressed = true;
            OverwritePopup.PopupView.AcceptCommand = new Command(PopupAcp);
            OverwritePopup.PopupView.DeclineCommand = new Command(PopupDecl);
            OverwritePopup.Show();
            async void PopupAcp()
            {
                if (NewPass1.Text != null && NewPass1.Text.Equals(NewPass2.Text))
                {
                    UserListForm2 response = await fc.Child("Users").Child($"{UserData.StudentUsername}").OnceSingleAsync<UserListForm2>();
                    if(response.Password.Equals(InitPass.Text))
                    {
                        bool Operation = true;
                        try
                        {
                            await fc.Child("Users").Child($"{UserData.StudentUsername}").PatchAsync(new { Password = NewPass1.Text });
                        }
                        catch(Exception ex)
                        {
                            Operation = false;
                            DependencyService.Get<IToast>().ShowShort($"Thất bại: Có lỗi xảy ra. {ex}");
                        }
                        if(Operation) DependencyService.Get<IToast>().ShowShort("Đổi mật khẩu thành công.");
                    }
                    else
                    {
                        DependencyService.Get<IToast>().ShowShort("Thất bại: Mật khẩu không hợp lệ.");
                    }
                }
                else
                {
                    DependencyService.Get<IToast>().ShowShort("Thất bại: Mật khẩu không hợp lệ.");
                }
                return;
            }
            void PopupDecl()
            {
                return;
            }
        }

        private async void SendAbs_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SendAbsent());
        }

        private async void RestDay_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RestDay());
        }

        private async void DelDBLocal_Tapped(object sender, EventArgs e)
        {
            if (await DisplayActionSheet("Bạn có chắc chắn muốn xóa dữ liệu không?", "Có", "Không") == "Có")
            {
                SQLiteConnection db = new SQLiteConnection(GlobalVariables.localLogHistDatabasePath);
                db.CreateTable<LogListForm>();
                db.DeleteAll<LogListForm>();
                DependencyService.Get<IToast>().ShowShort("Đã xóa!");
            }
        }

        private async void GPSTest_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GPSTesting());
        }

        private void IsGPSRequired_OnChanged(object sender, ToggledEventArgs e)
        {
            GlobalVariables.IsGPSRequired = IsGPSRequired_Switch.IsToggled;
        }

        private async void FirebaseLogTesting_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FirebaseLogTesting());
        }

        private async void ReportGene_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReportGen());
        }

        private async void UserCard_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserCard());
        }

        private async void Timetable_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Timetable());
        }

        private async void Medical_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MedicalInfo());
        }

        private async void TimetablePage_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TimetablePage());
        }

        private async void TeachSubstitute_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TeachSubstitute());
        }

        private async void SetCovid_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SetCovid());
        }
    }
}