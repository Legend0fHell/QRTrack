using QRdangcap.GoogleDatabase;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GenQR : ContentPage
    {
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public static DateTime LatestQRTimeChange { get; set; }
        public static DateTime ExpectedQRTimeChange { get; set; }
        public static bool AutoUpdate { get; set; }

        public GenQR()
        {
            InitializeComponent();
            Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (AutoUpdate && DateTime.Now >= ExpectedQRTimeChange) UpdateQR();
                    if (!AutoUpdate)
                    {
                        AutoStat.Text = "Tự động lấy (tắt)";
                        ExpectedChangeInterval.Text = "Không tự động cập nhật mã QR.";
                    }
                    else
                    {
                        AutoStat.Text = "Tự động lấy (bật)";
                        if (!RefreshingView.IsRefreshing)
                        {
                            var Remaining = ExpectedQRTimeChange - DateTime.Now;
                            string RemainingTxt = Remaining.ToString(@"hh\:mm\:ss");
                            ExpectedChangeInterval.Text = "Tạo mã QR mới trong " + RemainingTxt + ".";
                        }
                    }
                });
                return true;
            });
        }

        public void RetrieveAndGenQR(string plainText)
        {
            randomQRCode.Text = plainText;
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            encodedQRCode.Text = instance.Base64Encode(randomQRCode.Text);
            QRCodeImg.BarcodeValue = encodedQRCode.Text;
            QRCodeImg.IsVisible = true;
        }

        public void Button_Clicked(object sender, EventArgs e)
        {
            RetrieveAndGenQR(entryQRCode.Text);
        }

        public async void UpdateQR()
        {
            RefreshingView.IsRefreshing = true;
            ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "1",
            });
            RetrieveAndGenQR("CYB_" + response.Message);
            LatestQRTimeChange = response.DateTimeMessage.ToLocalTime().DateTime;
            ExpectedQRTimeChange = LatestQRTimeChange.AddMinutes(response.Message1);
            RefreshingView.IsRefreshing = false;
        }

        public void Button2_Clicked(object sender, EventArgs e)
        {
            UpdateQR();
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            AutoUpdate = !AutoUpdate;
            UpdateQR();
        }
    }
}