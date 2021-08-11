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
using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.ViewModel;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    // Today status.
    public partial class DUserInfo : ContentPage
    {
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public UserListForm StChose;
        readonly DUserInfoViewModel ViewModel;

        public DUserInfo(UserListForm St)
        {
            InitializeComponent();
            FilterMode.ItemsSource = new List<string>() { "Tất cả", "HS ĐD Đúng giờ", "HS ĐD Muộn giờ" };
            ChoseStName.Text = St.StName;
            ChoseStClass.Text = St.StClass;
            StChose = St;
            ViewModel = new DUserInfoViewModel();
            BindingContext = ViewModel;
            FilterMode.SelectedIndex = 0;
        }
        public void UpdateLog()
        {
            ViewModel.LogListFirebase.Clear();
            if (FilterMode.SelectedIndex == 0)
            {
                int cntLoaded = 0;
                IDisposable Subscriber = fc.Child("Logging").OrderBy("StId").EqualTo(StChose.StId)
                    .LimitToLast(5).AsObservable<LogListForm>().Subscribe(
                    x => {
                        int index = ViewModel.LogListFirebase.IndexOf(x.Object);
                        if (x.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                        {
                            if (index < 0)
                            {
                                ViewModel.LogListFirebase.Insert(0, x.Object);
                                ++cntLoaded;
                            }
                            else
                            {
                                ViewModel.LogListFirebase[index] = x.Object;
                            }
                        }
                        else
                        {
                            ViewModel.LogListFirebase.RemoveAt(index);
                            --cntLoaded;
                        }
                        ViewModel.RetrieveLog = $"Đã tải {cntLoaded}/?? mục. (Dữ liệu cập nhật tự động)";
                        LogList.ScrollTo(1);
                    }
                );
            }
            else
            {
                int cntLoaded = 0;
                IDisposable Subscriber = fc.Child("Logging").OrderBy("Id2Id").EqualTo(StChose.StId.ToString() + "_" + FilterMode.SelectedIndex.ToString())
                    .LimitToLast(5).AsObservable<LogListForm>().Subscribe(
                    x => {
                        int index = ViewModel.LogListFirebase.IndexOf(x.Object);
                        if (x.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                        {
                            if (index < 0)
                            {
                                ViewModel.LogListFirebase.Insert(0, x.Object);
                                ++cntLoaded;
                            }
                            else
                            {
                                ViewModel.LogListFirebase[index] = x.Object;
                            }
                        }
                        else
                        {
                            ViewModel.LogListFirebase.RemoveAt(index);
                            --cntLoaded;
                        }
                        ViewModel.RetrieveLog = $"Đã tải {cntLoaded}/?? mục. (Dữ liệu cập nhật tự động)";
                        LogList.ScrollTo(1);
                    }
                );
            }
           
        }
        private async void List_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is LogListForm logIdChose)) return;
            await Navigation.PushAsync(new LogChanger(logIdChose));
            LogList.SelectedItem = null;
        }
        private void FilterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLog();
        }
    }
}