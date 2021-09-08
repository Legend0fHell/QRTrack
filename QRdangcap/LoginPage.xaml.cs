using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using System.Threading.Tasks;
using Xamanimation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();

        public LoginPage()
        {
            InitializeComponent();
            Entry_Username.Text = "";
            Entry_Password.Text = "";
            ClientVer.Text = "Phiên bản: " + GlobalVariables.ClientVersion + " (Dựng lúc " + GlobalVariables.ClientVersionDate.ToString("G") + ")";
            Init();
            if(Preferences.Get("PrevSaved", false))
            {
                Entry_Username.Text = Preferences.Get("SavedUser", "");
                Entry_Password.Text = Preferences.Get("SavedPass", "");
                LoginProcedure(null, null);
            }
        }

        private int isInstantLogin = 0;

        private void Init()
        {
            Entry_Username.Completed += (s, e) => Entry_Password.Focus();
            Entry_Password.Completed += (s, e) => LoginProcedure(s, e);
            isInstantLogin = 0;
        }

        private void InstantLoginProcedure(object sender, System.EventArgs e)
        {
            isInstantLogin = 1;
            LoginProcedure(sender, e);
        }

        private void InstantLoginProcedureBypass(object sender, System.EventArgs e)
        {
            isInstantLogin = 2;
            LoginProcedure(sender, e);
        }

        private async void LoginProcedure(object sender, System.EventArgs e)
        {
            LoginStat.Text = "Đang đăng nhập... (1/6)";
            AnimateField();
            void AnimateField()
            {
                EntryField.Animate(new TranslateToAnimation()
                {
                    TranslateY = -100,
                    Duration = "300",
                });
                EntryField.Animate(new FadeToAnimation()
                {
                    Opacity = 0,
                    Duration = "100",
                });
                LoginBtn.Animate(new FadeToAnimation()
                {
                    Opacity = 0,
                    Duration = "100",
                });
                LoginBtn.Animate(new ScaleToAnimation()
                {
                    Duration = "500",
                    Scale = 0.1
                });
                LoginStat.Animate(new FadeToAnimation()
                {
                    Opacity = 100,
                    Duration = "100",
                });
                ActivityIndicator.IsRunning = true;
                ActivityIndicator.Animate(new FadeToAnimation()
                {
                    Opacity = 100,
                    Duration = "100",
                });
            }
            // dirty hack to hopefully got animation preloaded
            await Task.Delay(500);
            if (isInstantLogin > 0)
            {
                Entry_Username.Text = "InstantLoginAdmin";
                Entry_Password.Text = "LoliIsLoveLoliIsLife";
                if (isInstantLogin > 1)
                {
                    UserData.StudentUsername = Entry_Username.Text;
                    UserData.StudentClass = "Debug";
                    UserData.StudentFullName = "Admin (Debug) (Bypassed)";
                    UserData.StudentPriv = 9;
                    UserData.StudentIdDatabase = 9;
                    UserData.IsHidden = true;
                    LoginSucceeded();
                    return;
                }
            }
            Entry_Username.IsReadOnly = true;
            Entry_Password.IsReadOnly = true;

            if (Entry_Username.Text.Length > 0 && Entry_Password.Text.Length > 0)
            {
                ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
                {
                    Mode = "0",
                    Contents = Entry_Username.Text,
                    Contents2 = Entry_Password.Text,
                });
                if (response.Status == "SUCCESS")
                {
                    if (response.Message == "OK")
                    {
                        UserData.StudentUsername = Entry_Username.Text;
                        UserData.StudentClass = response.STClass;
                        UserData.StudentFullName = response.STName;
                        UserData.StudentPriv = response.STPriv;
                        UserData.StudentIdDatabase = response.STId;
                        UserData.IsHidden = response.Message1 == 1;
                        LoginSucceeded();
                    }
                    else LoginFailed("");
                }
                else LoginFailed("");
            }
            else LoginFailed("");
        }

        private void LoginFailed(string exc)
        {
            AnimateField();
            void AnimateField()
            {
                EntryField.Animate(new FadeToAnimation()
                {
                    Duration = "500",
                    Opacity = 100,
                });
                EntryField.Animate(new TranslateToAnimation()
                {
                    TranslateY = 0,
                    Duration = "150",
                });
                LoginBtn.Animate(new FadeToAnimation()
                {
                    Opacity = 100,
                    Duration = "100",
                });
                LoginBtn.Animate(new ScaleToAnimation()
                {
                    Duration = "100",
                    Scale = 1
                });
                LoginStat.Animate(new FadeToAnimation()
                {
                    Opacity = 0,
                    Duration = "100",
                });
                ActivityIndicator.IsRunning = false;
                ActivityIndicator.Animate(new FadeToAnimation()
                {
                    Opacity = 0,
                    Duration = "100",
                });
            }
            ActivityIndicator.IsRunning = false;
            Entry_Username.IsReadOnly = false;
            Entry_Password.IsReadOnly = false;
            DependencyService.Get<IToast>().Show("Thất bại! Tên đăng nhập hoặc mật khẩu không đúng." + exc);
        }

        private async void LoginSucceeded()
        {
            if (SavedPreference.IsChecked)
            {
                Preferences.Set("SavedUser", Entry_Username.Text);
                Preferences.Set("SavedPass", Entry_Password.Text);
                Preferences.Set("PrevSaved", true);
            }
            else Preferences.Clear();
            isInstantLogin = 0;
            LoginStat.Text = "Đang tải dữ liệu của trường... (2/6)";
            await instance.CheckUserTableExist();
            LoginStat.Text = "Đang tải dữ liệu của trường... (3/6)";
            UserData.NoUserRanked = await instance.GetGlobalUserRanking();
            LoginStat.Text = "Đang tải dữ liệu của trường... (4/6)";
            await instance.GetGlobalLogStat();
            ResponseModel response2 = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "18",
                Contents = UserData.StudentIdDatabase.ToString(),
            });
            UserData.SchoolLat = response2.Latitude;
            UserData.SchoolLon = response2.Longitude;
            UserData.SchoolDist = response2.Distance;
            LoginStat.Text = "Đang cập nhật vị trí... (5/6)";
            GlobalVariables.IsGPSRequired = true;
            await instance.UpdateCurLocation();
            UserData.StartTime = response2.StartTime;
            UserData.EndTime = response2.EndTime;
            UserData.LateTime = response2.LateTime;
            if (response2.Message == "0") UserData.IsUserLogin = 0;
            else if (response2.Message == "1") UserData.IsUserLogin = 1;
            else if (response2.Message == "2") UserData.IsUserLogin = 2;
            else if (response2.Message == "3") UserData.IsUserLogin = 3;
            else if (response2.Message == "-1")
            {
                UserData.IsUserLogin = 0;
                UserData.IsTodayOff = true;
            }
            LoginStat.Text = "Đang kiểm tra cập nhật mới... (6/6)";
            instance.CheckUpdates();
            LoginStat.Text = "Đăng nhập thành công! (6/6)";
            Entry_Username.Text = "";
            Entry_Password.Text = "";
            Entry_Username.IsReadOnly = false;
            Entry_Password.IsReadOnly = false;
            AnimateField();
            void AnimateField()
            {
                EntryField.Animate(new FadeToAnimation()
                {
                    Duration = "500",
                    Opacity = 100,
                });
                EntryField.Animate(new TranslateToAnimation()
                {
                    TranslateY = 0,
                    Duration = "150",
                });
                LoginBtn.Animate(new FadeToAnimation()
                {
                    Opacity = 100,
                    Duration = "100",
                });
                LoginBtn.Animate(new ScaleToAnimation()
                {
                    Duration = "100",
                    Scale = 1
                });
                LoginStat.Animate(new FadeToAnimation()
                {
                    Opacity = 0,
                    Duration = "100",
                });
                ActivityIndicator.IsRunning = false;
                ActivityIndicator.Animate(new FadeToAnimation()
                {
                    Opacity = 0,
                    Duration = "100",
                });
            }
            DependencyService.Get<IToast>().ShowShort("Đăng nhập thành công!");
            await Shell.Current.GoToAsync(state: "//main");
        }
    }
}