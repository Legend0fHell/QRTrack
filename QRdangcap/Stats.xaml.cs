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
using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.ObjectModel;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Stats : ContentPage
    {
        public ObservableCollection<LogListForm> _LogListFirebase = new ObservableCollection<LogListForm>();
        public ObservableCollection<LogListForm> LogListFirebase
        {
            get { return _LogListFirebase; }
            set
            {
                _LogListFirebase = value;
                OnPropertyChanged();
            }
        }
        public Stats()
        {
            InitializeComponent();
            refreshAll.IsRefreshing = true;
        }
        private async void List_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is LogListForm logIdChose)) return;
            await Navigation.PushAsync(new LogChanger(logIdChose));
            LogList.SelectedItem = null;
            // OnAppearing reloading
        }
        private void RetrieveLog_Clicked(object sender, EventArgs e)
        {
            refreshAll.IsRefreshing = true;
        }

        private void RetrieveLogs()
        {
            FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
            LogListFirebase = fc.Child("Logging").OrderByKey().AsObservable<LogListForm>().AsObservableCollection();
            RetrieveLog.Text = LogListFirebase.Count + " mục, nhấn để tải lại!";
            refreshAll.IsRefreshing = false;
            BindingContext = this;
        }
        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            RetrieveLogs();
        }
    }
}