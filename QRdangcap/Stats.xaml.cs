using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.LocalDatabase;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Stats : ContentPage
    {
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
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
            LogListFirebase = fc.Child("Logging").OrderByKey().LimitToLast(10).AsObservable<LogListForm>().AsObservableCollection();
            //LogListFirebase = new ObservableCollection<LogListForm>(LogListFirebase2.OrderByDescending(x => x.Timestamp));
            RetrieveLog.Text = _LogListFirebase.Count + " mục, nhấn để tải lại!";
            refreshAll.IsRefreshing = false;
            BindingContext = this;
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            RetrieveLogs();
        }
    }
}