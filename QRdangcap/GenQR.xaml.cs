using Newtonsoft.Json;
using QRdangcap.GoogleDatabase;
using System;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GenQR : ContentPage
    {
        public static HttpClient client = new HttpClient();

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
            var model = new FeedbackModel()
            {
                Mode = "1",
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var resultQR = await client.PostAsync(uri, requestContent);
            var resultContent = await resultQR.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
            RetrieveAndGenQR("CYB_" + response.Message);
            RefreshingView.IsRefreshing = false;
        }
    }
}