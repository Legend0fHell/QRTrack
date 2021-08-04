using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using QRdangcap.GoogleDatabase;
using QRdangcap.LocalDatabase;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            Entry_Username.Text = "";
            Entry_Password.Text = "";
            ClientVer.Text = "Phiên bản: " + GlobalVariables.ClientVersion + " (Dựng lúc " + GlobalVariables.ClientVersionDate.ToString("G") + ")";
            Init();
        }
        int isInstantLogin = 0;
        void Init()
        {
            Entry_Username.Completed += (s, e) => Entry_Password.Focus();
            Entry_Password.Completed += (s, e) => LoginProcedure(s, e);
            isInstantLogin = 0;
        }
        private void InstantLoginProcedure(object sender, System.EventArgs e)
        {
            isInstantLogin = 1;
            LoginProcedure(sender, e);
            isInstantLogin = 0;
        }
        private void InstantLoginProcedureBypass(object sender, System.EventArgs e)
        {
            isInstantLogin = 2;
            LoginProcedure(sender, e);
            isInstantLogin = 0;
        }
        private async void LoginProcedure(object sender, System.EventArgs e)
        {
            // debugging account
            if(isInstantLogin > 0)
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
                    LoginSucceeded();
                    return;
                }
            }

            ActivityIndicator.IsRunning = true;
            Entry_Username.IsReadOnly = true;
            Entry_Password.IsReadOnly = true;

            if(Entry_Username.Text.Length > 0 && Entry_Password.Text.Length>0)
            {
                var client = new HttpClient();
                var model = new FeedbackModel()
                {
                    Mode = "0",
                    Contents = Entry_Username.Text,
                };
                var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
                var jsonString = JsonConvert.SerializeObject(model);
                var requestContent = new StringContent(jsonString);
                var result = await client.PostAsync(uri, requestContent);
                var resultContent = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
                if (response.Status == "SUCCESS")
                {
                    UserData.StudentUsername = Entry_Username.Text;
                    UserData.StudentClass = response.STClass;
                    UserData.StudentFullName = response.STName;
                    UserData.StudentPriv = response.STPriv;
                    UserData.StudentIdDatabase = response.STId;

                    if (response.Message == Entry_Password.Text) LoginSucceeded();
                    else LoginFailed("");
                }
                else LoginFailed("");
            }
            else LoginFailed("");

        }
        private void LoginFailed(string exc)
        {
            ActivityIndicator.IsRunning = false;
            Entry_Username.IsReadOnly = false;
            Entry_Password.IsReadOnly = false;
            DependencyService.Get<IToast>().Show("Thất bại! Tên đăng nhập hoặc mật khẩu không đúng." + exc);
        }
        private async void LoginSucceeded()
        {
            
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            instance.CheckUserTableExist();
            ActivityIndicator.IsRunning = false;
            Entry_Username.Text = "";
            Entry_Password.Text = "";
            Entry_Username.IsReadOnly = false;
            Entry_Password.IsReadOnly = false;

            DependencyService.Get<IToast>().ShowShort("Đăng nhập thành công!");
            await Shell.Current.GoToAsync(state: "//main");
        }
    }
}