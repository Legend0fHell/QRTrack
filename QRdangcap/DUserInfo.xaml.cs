using Firebase.Database;
using Firebase.Database.Query;
using Plugin.CloudFirestore;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    // Today status.
    public partial class DUserInfo : ContentPage
    {
        //public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public UserListForm StChose;
        private readonly DUserInfoViewModel ViewModel;
        public IListenerRegistration Subscriber;
        public bool FirstTime = true;
        public int cntLoaded = 0;
        public int Step = 10;
        public int ExpectLoadedLog { get; set; }

        public DUserInfo(UserListForm St)
        {
            InitializeComponent();
            FilterMode.ItemsSource = new List<string>() { "Tất cả", "HS ĐD Đúng giờ", "HS ĐD Muộn giờ" };
            ChoseStName.Text = St.StName;
            ChoseStClass.Text = St.StClass;
            StChose = St;
            ViewModel = new DUserInfoViewModel();
            BindingContext = ViewModel;
            if (UserData.StudentPriv > 0) ViewModel.IsEditAllowed = true;
            else ViewModel.IsEditAllowed = false;
            FilterMode.SelectedIndex = 0;
        }

        public LogListForm InboundLogConv(InboundLog inbound)
        {
            return new LogListForm()
            {
                Keys = inbound.Keys,
                StId = inbound.StId,
                Mistake = inbound.Mistake,
                Timestamp = (inbound.Timestamp.ToDateTime().Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerMillisecond,
                ReporterId = inbound.ReporterId,
                LoginStatus = inbound.LoginStatus,
            };
        }
        public void UpdateList(IQuerySnapshot snapshot, int StartingPoint)
        {
            foreach (var documentChange in snapshot.DocumentChanges)
            {
                LogListForm documentAdded = InboundLogConv(documentChange.Document.ToObject<InboundLog>());
                int index = ViewModel.LogListFirebase.IndexOf(documentAdded);
                switch (documentChange.Type)
                {
                    case DocumentChangeType.Added:
                        if (index >= 0) break;
                        ViewModel.LogListFirebase.Add(documentAdded);
                        ++cntLoaded;
                        LogList.ScrollTo(Math.Max(StartingPoint - 1, 0));
                        break;
                    case DocumentChangeType.Modified:
                        int index2 = ViewModel.LogListFirebase.IndexOf(ViewModel.LogListFirebase.Where(x => x.Keys == documentAdded.Keys).FirstOrDefault());
                        if (index2 >= 0) ViewModel.LogListFirebase[index2] = documentAdded;
                        break;
                    case DocumentChangeType.Removed:
                        int index3 = ViewModel.LogListFirebase.IndexOf(ViewModel.LogListFirebase.Where(x => x.Keys == documentAdded.Keys).FirstOrDefault());
                        if (index3 >= 0) ViewModel.LogListFirebase.RemoveAt(index3);
                        --cntLoaded;
                        break;
                }
                ViewModel.RetrieveLog = $"Đã tải {cntLoaded} mục gần đây nhất.";
                ViewModel.IsVisi = cntLoaded >= ExpectLoadedLog;
            }
        }
        public void UpdateLog(int StartingPoint = 0)
        {
            if (FilterMode.SelectedIndex == 0)
            {
                if (StartingPoint == 0)
                {
                    Subscriber = CrossCloudFirestore.Current
                   .Instance
                   .Collection("logging")
                   .WhereEqualsTo("StId", StChose.StId)
                   .OrderBy("Timestamp", true)
                   .LimitTo(Step)
                   .AddSnapshotListener((snapshot, error) =>
                   {
                       if (snapshot != null)
                       {
                           UpdateList(snapshot, StartingPoint);
                       }
                   });
                }
                else
                {
                    Subscriber = CrossCloudFirestore.Current
                    .Instance
                    .Collection("logging")
                    .WhereEqualsTo("StId", StChose.StId)
                    .OrderBy("Timestamp", true)
                    .StartAfter(new Timestamp(ViewModel.LogListFirebase.Last().LoginDate))
                    .LimitTo(Step)
                    .AddSnapshotListener((snapshot, error) =>
                    {
                        if (snapshot != null)
                        {
                            UpdateList(snapshot, StartingPoint);
                        }
                    });
                }

            }
            else
            {
                if (StartingPoint == 0)
                {
                    Subscriber = CrossCloudFirestore.Current
                   .Instance
                   .Collection("logging")
                   .WhereEqualsTo("StId", StChose.StId)
                   .WhereEqualsTo("LoginStatus", FilterMode.SelectedIndex)
                   .OrderBy("Timestamp", true)
                   .LimitTo(Step)
                   .AddSnapshotListener((snapshot, error) =>
                   {
                       if (snapshot != null)
                       {
                           UpdateList(snapshot, StartingPoint);
                       }
                   });
                }
                else
                {
                    Subscriber = CrossCloudFirestore.Current
                    .Instance
                    .Collection("logging")
                    .WhereEqualsTo("StId", StChose.StId)
                    .WhereEqualsTo("LoginStatus", FilterMode.SelectedIndex)
                    .OrderBy("Timestamp", true)
                    .StartAfter(new Timestamp(ViewModel.LogListFirebase.Last().LoginDate))
                    .LimitTo(Step)
                    .AddSnapshotListener((snapshot, error) =>
                    {
                        if (snapshot != null)
                        {
                            UpdateList(snapshot, StartingPoint);
                        }
                    });
                }
            }
            FirstTime = false;
        }

        private async void List_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is LogListForm logIdChose)) return;
            if (ViewModel.IsEditAllowed) await Navigation.PushAsync(new LogChanger(logIdChose));
            LogList.SelectedItem = null;
        }

        private void FilterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            cntLoaded = 0;
            ExpectLoadedLog = Step;
            ViewModel.LogListFirebase.Clear();
            if (!FirstTime) Subscriber.Dispose();
            UpdateLog();
        }

        private void LoadMoreData_Clicked(object sender, EventArgs e)
        {
            ExpectLoadedLog += Step;
            UpdateLog(ViewModel.LogListFirebase.Count);
        }
    }
}