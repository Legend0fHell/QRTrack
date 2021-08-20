using Newtonsoft.Json;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.ComponentModel;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Other : ContentPage
    {
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
            CurAcc = "Tên: " + UserData.StudentFullName + ", ID: " + UserData.StudentIdDatabase.ToString();
            ClientVerText.Detail = GlobalVariables.ClientVersion + " (dựng lúc: " + GlobalVariables.ClientVersionDate.ToString("G") + ")";
            BindingContext = this;
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

        private async void ChangePassword_Tapped(object sender, EventArgs e)
        {
            string NewPassword = await DisplayPromptAsync("Nhập mật khẩu mới:", "Chỉ là PoC, mọi thứ có thể thay đổi.");
            if (NewPassword != null)
            {
                var client = new HttpClient();
                var model = new FeedbackModel()
                {
                    Mode = "12",
                    Contents = UserData.StudentIdDatabase.ToString(),
                    Contents2 = NewPassword
                };
                var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
                var jsonString = JsonConvert.SerializeObject(model);
                var requestContent = new StringContent(jsonString);
                var result = await client.PostAsync(uri, requestContent);
                var resultContent = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
                if (response.Status.Equals("SUCCESS"))
                {
                    DependencyService.Get<IToast>().ShowShort("Đổi mật khẩu thành công.");
                }
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
    }
}