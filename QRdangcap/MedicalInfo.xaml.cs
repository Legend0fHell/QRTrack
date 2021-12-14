using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MedicalInfo : ContentPage
    {
        public ObservableRangeCollection<UserListForm> ItemsList { get; set; }
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
        public List<UserListForm> filteredItem = new List<UserListForm>();
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        private readonly MedicalInfoViewModel ViewModel;
        public IDisposable Subscriber;
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);

        public MedicalInfo()
        {
            InitializeComponent();
            ViewModel = new MedicalInfoViewModel();
            BindingContext = ViewModel;
            DatePicked.MaximumDate = DateTime.Now;
            DatePicked.Date = DateTime.Now;
            refreshAll.IsRefreshing = true;
        }

        protected override bool OnBackButtonPressed()
        {
            try
            {
                Subscriber.Dispose();
            }
            catch
            {
                // smth
            }
            return base.OnBackButtonPressed();
        }

        public void Init()
        {
            ItemsList = new ObservableRangeCollection<UserListForm>();
            ItemsList.Clear();
            ViewModel.DisplayList = new ObservableRangeCollection<UserListForm>();
            ViewModel.DisplayList.Clear();
            Subscriber = fc.Child("Personal").Child($"{DatePicked.Date.Year}_{DatePicked.Date.DayOfYear}").OrderBy("Medical").EqualTo(1).AsObservable<MedicalListForm>().Subscribe(
                x =>
                {
                    System.Diagnostics.Debug.WriteLine($" Firebase update: {x.EventType} - {x.Key}");
                    ItemsList.Add(db.Table<UserListForm>().ToList().Where(y => y.StId.ToString() == x.Key).FirstOrDefault());
                    ViewModel.DisplayList.Add(db.Table<UserListForm>().ToList().Where(y => y.StId.ToString() == x.Key).FirstOrDefault());
                }
            );
            refreshAll.IsRefreshing = false;
        }

        private void Test_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = NameQuery.Text;
            string classSearchTerm = ClassQuery.Text;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(classSearchTerm))
            {
                classSearchTerm = string.Empty;
            }
            searchTerm = searchTerm.ToLower(CultureInfo.CreateSpecificCulture("vi-VN"));
            classSearchTerm = classSearchTerm.ToLower(CultureInfo.CreateSpecificCulture("vi-VN"));
            List<UserListForm> UserList = new List<UserListForm>(ItemsList);
            filteredItem = UserList;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredItem = UserList.Where(x => x.StName.ToLower(CultureInfo.CreateSpecificCulture("vi-VN")).Contains(searchTerm)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(classSearchTerm))
            {
                filteredItem = filteredItem.Where(x => x.StClass.ToLower(CultureInfo.CreateSpecificCulture("vi-VN")).Contains(classSearchTerm)).ToList();
            }
            ViewModel.DisplayList.Clear();
            ViewModel.DisplayList.AddRange(filteredItem);
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            try
            {
                Subscriber.Dispose();
            }
            catch
            {
                // smth
            }
            Init();
        }

        private async void MyCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is UserListForm userIdChose)) return;
            await Navigation.PushAsync(new DUserInfo(userIdChose));
            myCollectionView.SelectedItem = null;
        }

        private void DatePicked_DateSelected(object sender, DateChangedEventArgs e)
        {
            refreshAll.IsRefreshing = true;
        }
    }
}