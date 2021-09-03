using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportGen : ContentPage
    {
        private readonly ReportGenViewModel ViewModel;
        private readonly SQLiteConnection db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public IDisposable Subscriber;

        public ReportGen()
        {
            InitializeComponent();
            ViewModel = new ReportGenViewModel();
            BindingContext = ViewModel;
            ObservableCollection<Folder> nodeImageInfo = new ObservableCollection<Folder>();
            List<UserListForm> ItemsList = new List<UserListForm>();
            ItemsList.AddRange(db.Table<UserListForm>().ToList().Where(x => x.StClass.Equals("Debug")).OrderBy(x => x.StId));
            int FirstST = ItemsList[0].StId;
            for (int i = 0; i < ItemsList.Count; i++)
            {
                Folder tmp = new Folder() { FileName = ItemsList[i].StName, ImageIcon = "ic_action_radio_button_checked.png" };
                tmp.Files = new ObservableCollection<File>();
                nodeImageInfo.Add(tmp);
            }
            ViewModel.Folders = nodeImageInfo;
            string ThisId3Id = instance.To6DigitString("Debug") + "_";
            string ThisId3IdBegin = ThisId3Id + ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0).Subtract(new DateTime(1970, 1, 1)).Ticks) / TimeSpan.TicksPerMillisecond).ToString();
            string ThisId3IdEnd = ThisId3Id + ((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999).Subtract(new DateTime(1970, 1, 1)).Ticks) / TimeSpan.TicksPerMillisecond).ToString();
            Subscriber = fc.Child("Logging").OrderBy("Id3Id").StartAt(ThisId3IdBegin).EndAt(ThisId3IdEnd).AsObservable<LogListForm>().Subscribe(
            x =>
            {
                System.Diagnostics.Debug.WriteLine($" Firebase update): {x.EventType} - {x.Object.StId}");
                if (x.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate && !x.Object.Mistake.Equals("NONE"))
                {
                    DateTime LoggedTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(x.Object.Timestamp).ToLocalTime();
                    ViewModel.Folders[x.Object.StId - FirstST].Files.Add(new File() { FileName = $"{LoggedTime:dd.MM}: {x.Object.Mistake}", ImageIcon = "ic_action_radio_button_unchecked.png" });
                }
            });
        }

        private void TreeView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Changed");
        }

        private void TreeView_Loaded(object sender, Syncfusion.XForms.TreeView.TreeViewLoadedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Loaded");
        }
    }
}