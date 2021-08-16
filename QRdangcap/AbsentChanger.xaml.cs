using Newtonsoft.Json;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AbsentChanger : ContentPage
    {
        public static HttpClient client = new HttpClient();
        public AbsentLogForm globalLogList = new AbsentLogForm();

        public AbsentChanger(AbsentLogForm logList)
        {
            InitializeComponent();
            globalLogList = logList;
            ChoseLogId.Text = logList.LogId.ToString();
            ChoseString.Text = logList.StId.ToString() + " " + logList.StClass + " - " + logList.StName;
            FromDate.Date = logList.DateCSD;
            ToDate.Date = logList.DateCED;
        }

        public async void Button_Clicked_1(object sender, System.EventArgs e)
        {
            string QueryName = ChoseString.Text;
            DependencyService.Get<IToast>().ShowShort("Đang sửa: " + QueryName);
            var model = new FeedbackModel()
            {
                Mode = "15",
                Contents = globalLogList.LogId.ToString(),
                Contents2 = UserData.StudentIdDatabase.ToString(),
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
                DependencyService.Get<IToast>().ShowShort("Sửa thành công: " + QueryName);
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Thất bại (Không tồn tại): " + QueryName);
                await DisplayAlert("wd", response.Message, "OK");
            }
            await Navigation.PopAsync();
        }

        public async void Button_Clicked_2(object sender, System.EventArgs e)
        {
            string QueryName = ChoseString.Text;
            DependencyService.Get<IToast>().ShowShort("Đang xóa: " + QueryName);
            var model = new FeedbackModel()
            {
                Mode = "16",
                Contents = globalLogList.LogId.ToString(),
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