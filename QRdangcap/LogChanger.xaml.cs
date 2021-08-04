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
using ZXing.Net.Mobile.Forms;
using System.Globalization;
using QRdangcap.LocalDatabase;
using SQLite;
using System.Diagnostics;
using Xamarin.CommunityToolkit.ObjectModel;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogChanger : ContentPage
    {
        public LogListForm globalLogList = new LogListForm();
        public LogChanger(LogListForm logList)
        {
            InitializeComponent();
            globalLogList = logList;
            ChoseLogId.Text = logList.LogId.ToString();
            ChoseString.Text = logList.StId.ToString() + " " + logList.StClass + " - " + logList.StName;
            StMistake.Text = logList.Mistake;
            OnTime.IsChecked = false;
            LateTime.IsChecked = false;
            if (logList.LoginStatus == 1) OnTime.IsChecked = true;
            else if (logList.LoginStatus == 2) LateTime.IsChecked = true;
            StTime.Text = logList.LoginDate.ToLocalTime().ToString("HH:mm:ss dd.MM.yyyy");
        }

        public async void Button_Clicked_1(object sender, System.EventArgs e)
        {
            string QueryName = ChoseString.Text;
            DependencyService.Get<IToast>().ShowShort("Đang sửa: " + QueryName);
            var client = new HttpClient();
            var model = new FeedbackModel()
            {
                Mode = "13",
                Contents = globalLogList.LogId.ToString(),
                Contents2 = UserData.StudentIdDatabase.ToString(),
                Contents3 = OnTime.IsChecked ? "1" : "2",
                Contents4 = StMistake.Text,
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var resultQR = await client.PostAsync(uri, requestContent);
            var resultContent = await resultQR.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
            if(response.Status == "SUCCESS")
            {
                DependencyService.Get<IToast>().ShowShort("Sửa thành công: " + QueryName);
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Thất bại (" +(response.Message == "-1" ? "Đã xóa trước đó" : "Không tồn tại") + "): " + QueryName);
            }
            await Navigation.PopAsync();
        }
        public async void Button_Clicked_2(object sender, System.EventArgs e)
        {
            string QueryName = ChoseString.Text;
            DependencyService.Get<IToast>().ShowShort("Đang xóa: " + QueryName);
            var client = new HttpClient();
            var model = new FeedbackModel()
            {
                Mode = "10",
                Contents = globalLogList.LogId.ToString(),
                Contents2 = UserData.StudentIdDatabase.ToString(),
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var resultQR = await client.PostAsync(uri, requestContent);
            var resultContent = await resultQR.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
            if (response.Status == "SUCCESS")
            {
                DependencyService.Get<IToast>().ShowShort("Xóa thành công: " + QueryName);
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Thất bại (Không tồn tại): " + QueryName);
            }
            await Navigation.PopAsync();
        }
    }
}