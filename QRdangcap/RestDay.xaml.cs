using QRdangcap.GoogleDatabase;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();

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
            ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "14",
                Contents = "1"
            });
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
            ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "14",
                Contents = "2",
                Contents2 = tmp
            });
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
            ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "14",
                Contents = "3",
                Contents2 = SetMode.SelectedIndex.ToString(),
                ContentStartTime = FromDate.Date.DayOfYear,
                ContentEndTime = ToDate.Date.DayOfYear,
            });
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