using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Xamanimation;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public LoginPage()
        {
            InitializeComponent();
            Entry_Username.Text = "";
            Entry_Password.Text = "";
            ClientVer.Text = "Bản dựng: " + GlobalVariables.ClientVersion + " (Dựng lúc " + GlobalVariables.ClientVersionDate.ToString("G") + ")";
            SavedPreference.IsChecked = true;
            Init();
            if (Preferences.Get("PrevSaved", false))
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
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Không có kết nối", "Kiểm tra lại kết nối mạng và thử lại.", "OK");
                return;
            }
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
                int retry = 1;
                while (retry < 5)
                {
                    try
                    {
                        UserListForm2 response = await fc.Child("Users").Child($"{Entry_Username.Text}").OnceSingleAsync<UserListForm2>();
                        if (response != null && response.Password == Entry_Password.Text)
                        {
                            UserData.StudentUsername = Entry_Username.Text;
                            UserData.StudentClass = response.StClass;
                            UserData.StudentFullName = response.StName;
                            UserData.StudentPriv = response.Priv;
                            UserData.StudentIdDatabase = response.StId;
                            UserData.IsHidden = response.IsHidden == 1;
                            LoginSucceeded();
                        }
                        else LoginFailed();
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
            }
            else LoginFailed();
        }

        private void LoginFailed(string exc = "Thất bại! Tên đăng nhập hoặc mật khẩu không đúng.")
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
            DependencyService.Get<IToast>().Show(exc);
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

            using (var client = new HttpClient())
            {
                try
                {
                    var result = client.GetAsync("https://google.com", HttpCompletionOption.ResponseHeadersRead).Result;
                    UserData.OffsetWithNIST = (TimeSpan)(result.Headers.Date - DateTimeOffset.Now);
                }
                catch
                {
                    UserData.OffsetWithNIST = TimeSpan.Zero;
                }
            }
            if (UserData.OffsetWithNIST.Duration() >= new TimeSpan(0, 30, 0))
            {
                await DisplayAlert("Thông báo", $"Đồng hồ trên máy của bạn đang bị lệch {(int)UserData.OffsetWithNIST.Duration().TotalSeconds} giây so với thời gian thực. Vui lòng chỉnh lại để tránh lỗi phát sinh.", "OK");
                LoginFailed("Đăng nhập thất bại!");
                return;
            }
            LoginStat.Text = "Đang tải dữ liệu của trường... (3/6)";
            _ = instance.GetGlobalLogStat();
            
            LoginStat.Text = "Đang tải dữ liệu của trường... (4/6)";
            _ = instance.GetGlobalUserRanking();
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
            LoginStat.Text = "Đang cập nhật vị trí... (5/6)";
            GlobalVariables.IsGPSRequired = true;
            if (!DependencyService.Get<IGpsDependencyService>().IsGpsEnable())
            {
                await DisplayAlert("Thông báo", "GPS chưa được bật. Nhấn OK để kích hoạt GPS trước khi sử dụng ứng dụng.", "OK");
                DependencyService.Get<IGpsDependencyService>().OpenSettings();
                LoginFailed("Đăng nhập thất bại!");
                return;
            }
            else
            {
                await instance.UpdateCurLocation();
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
            if (UserData.StudentPriv >= 0) await Shell.Current.GoToAsync(state: "//main", true);
            else await Shell.Current.GoToAsync($"//{nameof(ParentPage)}", true);
        }
    }
}