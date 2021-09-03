using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using Syncfusion.XForms.PopupLayout;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Other : ContentPage
    {
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

            ClientVerText.Detail = GlobalVariables.ClientVersion + " (dựng lúc: " + GlobalVariables.ClientVersionDate.ToString("G") + ")";
            IsGPSRequired.On = true;
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(UserData.StudentPriv == 0)
            {
                // Hs quèn
                UserCard.IsEnabled = true;
                GenQR.IsEnabled = false;
                SendAbs.IsEnabled = false;
                AbsLog.IsEnabled = false;
                RestDay.IsEnabled = false;
                GPSTest.IsEnabled = true;
                TCClr.IsEnabled = true;
                HistDB.IsEnabled = false;
                HistLocal.IsEnabled = true;
                ReportGene.IsEnabled = false;
                UpdateDBUser.IsEnabled = true;
                DelDBLocal.IsEnabled = true;
                FirebaseLogTesting.IsEnabled = false;
                IsGPSRequired.IsEnabled = false;
            }
            else if (UserData.StudentPriv == 1)
            {
                // XK
                UserCard.IsEnabled = true;
                GenQR.IsEnabled = true;
                SendAbs.IsEnabled = false;
                AbsLog.IsEnabled = false;
                RestDay.IsEnabled = false;
                GPSTest.IsEnabled = true;
                TCClr.IsEnabled = true;
                HistDB.IsEnabled = false;
                HistLocal.IsEnabled = true;
                ReportGene.IsEnabled = true;
                UpdateDBUser.IsEnabled = true;
                DelDBLocal.IsEnabled = true;
                FirebaseLogTesting.IsEnabled = false;
                IsGPSRequired.IsEnabled = false;
            }
            else if (UserData.StudentPriv == 2)
            {
                // GV
                UserCard.IsEnabled = true;
                GenQR.IsEnabled = true;
                SendAbs.IsEnabled = true;
                AbsLog.IsEnabled = true;
                RestDay.IsEnabled = false;
                GPSTest.IsEnabled = true;
                TCClr.IsEnabled = true;
                HistDB.IsEnabled = true;
                HistLocal.IsEnabled = true;
                ReportGene.IsEnabled = true;
                UpdateDBUser.IsEnabled = true;
                DelDBLocal.IsEnabled = true;
                FirebaseLogTesting.IsEnabled = false;
                IsGPSRequired.IsEnabled = true;
            }
            else if (UserData.StudentPriv == 3)
            {
                // QTV
                UserCard.IsEnabled = true;
                GenQR.IsEnabled = true;
                SendAbs.IsEnabled = true;
                AbsLog.IsEnabled = true;
                RestDay.IsEnabled = true;
                GPSTest.IsEnabled = true;
                TCClr.IsEnabled = true;
                HistDB.IsEnabled = true;
                HistLocal.IsEnabled = true;
                ReportGene.IsEnabled = true;
                UpdateDBUser.IsEnabled = true;
                DelDBLocal.IsEnabled = true;
                FirebaseLogTesting.IsEnabled = false;
                IsGPSRequired.IsEnabled = true;
            }
            else if (UserData.StudentPriv > 3)
            {
                // God
                UserCard.IsEnabled = true;
                GenQR.IsEnabled = true;
                SendAbs.IsEnabled = true;
                AbsLog.IsEnabled = true;
                RestDay.IsEnabled = true;
                GPSTest.IsEnabled = true;
                TCClr.IsEnabled = true;
                HistDB.IsEnabled = true;
                HistLocal.IsEnabled = true;
                ReportGene.IsEnabled = true;
                UpdateDBUser.IsEnabled = true;
                DelDBLocal.IsEnabled = true;
                FirebaseLogTesting.IsEnabled = true;
                IsGPSRequired.IsEnabled = true;
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

        private async void UpdateDBUser_Tapped(object sender, EventArgs e)
        {
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            await instance.RetrieveAllUserDatabase();
        }

        private async void Logout_Tapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            UserData.StudentPreIdDatabase = UserData.StudentIdDatabase;
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
                    ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
                    {
                        Mode = "12",
                        Contents = UserData.StudentIdDatabase.ToString(),
                        Contents2 = NewPass1.Text,
                        Contents3 = InitPass.Text
                    });
                    if (response.Status.Equals("SUCCESS"))
                    {
                        DependencyService.Get<IToast>().ShowShort("Đổi mật khẩu thành công.");
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

        private async void AbsLog_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AbsentLog());
        }

        private void DelDBLocal_Tapped(object sender, EventArgs e)
        {
            SQLiteConnection db = new SQLiteConnection(GlobalVariables.localLogHistDatabasePath);
            db.CreateTable<LogListForm>();
            db.DeleteAll<LogListForm>();
            DependencyService.Get<IToast>().ShowShort("Đã xóa!");
        }

        private async void GPSTest_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GPSTesting());
        }

        private void IsGPSRequired_OnChanged(object sender, ToggledEventArgs e)
        {
            GlobalVariables.IsGPSRequired = IsGPSRequired.On;
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
    }
}