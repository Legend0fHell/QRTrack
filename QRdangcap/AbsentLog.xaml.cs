using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AbsentLog : ContentPage
    {
        public RetrieveAllUserDb instance = new RetrieveAllUserDb();

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
            AbsentLogForm[] response = (AbsentLogForm[])await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "17",
            }, true, "AbsentLogForm[]", 5, "Không có dữ liệu!");
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