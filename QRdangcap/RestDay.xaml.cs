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
    public class WeekItem
    {
        public string WeekDay { get; set; }
        public bool IsChecked { get; set; }
    }

    public partial class RestDay : ContentPage
    {
        public List<WeekItem> WeekList = new List<WeekItem>();
        public static HttpClient client = new HttpClient();
        public RestDay()
        {
            InitializeComponent();
            SetMode.ItemsSource = new List<string>()
            {
                "Ngày học", "Ngày nghỉ"
            };
            SetMode.SelectedIndex = 0;
            InitWeek();
        }
        private async void InitWeek()
        {
            var model = new FeedbackModel()
            {
                Mode = "14",
                Contents = "1"
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var result = await client.PostAsync(uri, requestContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
            string WeekStatus = response.Message;
            WeekStatus += WeekStatus[0];
            for (int i = 0; i < 7; ++i)
            {
                string name;
                if (i < 6) name = "T" + (i + 2).ToString();
                else name = "CN";
                WeekItem tmp = new WeekItem()
                {
                    WeekDay = name,
                    IsChecked = WeekStatus[i + 1] == '1'
                };
                WeekList.Add(tmp);
            }
            RestInWeek.ItemsSource = WeekList;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            string tmp = "";
            for (int i = 0; i < 6; ++i)
            {
                tmp += WeekList[i].IsChecked ? '1' : '0';
            }
            tmp = (WeekList[6].IsChecked ? '1' : '0') + tmp;
            var model = new FeedbackModel()
            {
                Mode = "14",
                Contents = "2",
                Contents2 = tmp
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var result = await client.PostAsync(uri, requestContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
            if (response.Status == "SUCCESS")
            {
                DependencyService.Get<IToast>().ShowShort("Thành công!");
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Có lỗi xảy ra.");
            }
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if (FromDate.Date > ToDate.Date)
            {
                DependencyService.Get<IToast>().ShowShort("Lỗi: Ngày giờ không hợp lệ!");
                FromDate.Date = DateTime.Now;
                ToDate.Date = DateTime.Now;
                return;
            }
            DependencyService.Get<IToast>().ShowShort("Đang đặt...");
            var model = new FeedbackModel()
            {
                Mode = "14",
                Contents = "3",
                Contents2 = SetMode.SelectedIndex.ToString(),
                ContentStartTime = FromDate.Date.DayOfYear,
                ContentEndTime = ToDate.Date.DayOfYear,
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var resultQR = await client.PostAsync(uri, requestContent);
            var resultContent = await resultQR.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
            if (response.Status == "SUCCESS")
            {
                DependencyService.Get<IToast>().ShowShort("Thành công!");
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Có lỗi xảy ra.");
            }
        }
    }

}