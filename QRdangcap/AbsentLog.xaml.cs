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
    public partial class AbsentLog : ContentPage
    {
        public static HttpClient client = new HttpClient();
        public AbsentLog()
        {
            InitializeComponent();
            refreshAll.IsRefreshing = true;
        }
        private void RetrieveLog_Clicked(object sender, EventArgs e)
        {
            refreshAll.IsRefreshing = true;
        }
        private async void RetrieveAbsent()
        {
            var model = new FeedbackModel()
            {
                Mode = "17",
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var resultQR = await client.PostAsync(uri, requestContent);
            var resultContent = await resultQR.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<AbsentLogForm[]>(resultContent);
            LogList.ItemsSource = response.Reverse();
            refreshAll.IsRefreshing = false;
        }
        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            RetrieveAbsent();

        }

        private async void LogList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is AbsentLogForm logIdChose)) return;
            await Navigation.PushAsync(new AbsentChanger(logIdChose));
            LogList.SelectedItem = null;
        }
    }
}