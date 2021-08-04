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
using QRdangcap.LocalDatabase;
using ZXing.Net.Mobile.Forms;
using System.Globalization;

using SQLite;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Xamarin.CommunityToolkit.ObjectModel;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    // Today status.
    public partial class DUserInfo : ContentPage
    {
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localDatabasePath);
        public UserListForm StChose;
        public DUserInfo(UserListForm St)
        {
            InitializeComponent();
            ChoseStName.Text = St.StName;
            ChoseStClass.Text = St.StClass;
            StChose = St;
            UpdateLog();
        }
        public void UpdateLog()
        {
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            instance.RetrieveAllLogDatabase();
            db.CreateTable<LogListForm>();
            LogList.ItemsSource = db.Table<LogListForm>().ToList().Where(x => x.StId.Equals(StChose.StId)).Reverse();
        }
        private async void List_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is LogListForm logIdChose)) return;
            await Navigation.PushAsync(new LogChanger(logIdChose));
            LogList.SelectedItem = null;
        }
    }
}