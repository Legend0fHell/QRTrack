using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using QRdangcap.GoogleDatabase;
using System;
using System.Globalization;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;
using QRdangcap.LocalDatabase;
using SQLite;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendAbsent : ContentPage
    {
        int UserIDRead = 0;
        public SendAbsent()
        {
            InitializeComponent();
            ChoseString.Text = "Chọn học sinh...";
            MessagingCenter.Subscribe<Page, UserListForm>(this, "ChoseSTId", (p, resUser) =>
            {
                ChoseString.Text = resUser.StId.ToString() + " " + resUser.StClass + " - " + resUser.StName;
                UserIDRead = resUser.StId;
            });
            FromDate.Date = DateTime.Now;
            ToDate.Date = DateTime.Now;
        }
        public async void Scanner()
        {
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
            {
                PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE },
                AutoRotate = false,
            };
            var ScanView = new ZXingScannerPage(options);

            ScanView.OnScanResult += (result) =>
            {
                ScanView.IsScanning = false;
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopAsync();
                    bool invalidDetect = false;
                    UserIDRead = 0;
                    string decodedQRCode = "";
                    RetrieveAllUserDb instance = new RetrieveAllUserDb();
                    decodedQRCode = instance.Base64Decode(result.Text);
                    for (int i = 0; i < decodedQRCode.Length; ++i)
                    {
                        if (decodedQRCode[i] < 48 || decodedQRCode[i] > 57)
                        {
                            invalidDetect = true;
                            break;
                        }
                        else
                        {
                            UserIDRead *= 10;
                            UserIDRead += decodedQRCode[i] - '0';
                        }
                    }
                    if (invalidDetect)
                    {
                        DisplayAlert("Đọc", "Đọc thất bại! Code QR không hợp lệ!", "OK");
                    }
                    else
                    {
                        ChoseString.Text = instance.RetrieveNameUser(UserIDRead);
                    }
                });
            };

            await Navigation.PushAsync(ScanView);
        }
        public void Button_Clicked(object sender, System.EventArgs e)
        {
            Scanner();

        }
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var ChoosePage = new QueryInfo();
            await Navigation.PushAsync(ChoosePage);
        }
        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if(FromDate.Date > ToDate.Date)
            {
                DependencyService.Get<IToast>().ShowShort("Lỗi: Ngày giờ không hợp lệ!");
                FromDate.Date = DateTime.Now;
                ToDate.Date = DateTime.Now;
                return;
            }
            string QueryName = ChoseString.Text;
            DependencyService.Get<IToast>().ShowShort("Đang gửi: " + QueryName);
            var client = new HttpClient();
            var model = new FeedbackModel()
            {
                Mode = "4",
                Contents = UserIDRead.ToString(),
                ContentStartTime = FromDate.Date.DayOfYear,
                ContentEndTime = ToDate.Date.DayOfYear,
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
                DependencyService.Get<IToast>().ShowShort("OK: " + QueryName);
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Lỗi: " + QueryName);
            }
        }
    }
}