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

        public GenQR()
        {
            InitializeComponent();
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

        public async void Button2_Clicked(object sender, EventArgs e)
        {
            RefreshingView.IsRefreshing = true;
            ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "1",
            });
            RetrieveAndGenQR("CYB_" + response.Message);
            RefreshingView.IsRefreshing = false;
        }
    }
}