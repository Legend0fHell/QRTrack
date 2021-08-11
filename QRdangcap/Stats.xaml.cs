using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.LocalDatabase;
using QRdangcap.ViewModel;
using System;
using System.Collections.Generic;
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
        readonly StatsViewModel ViewModel;
        public Stats()
        {
            InitializeComponent();
            FilterMode.ItemsSource = new List<string>() { "Tất cả", "HS ĐD Đúng giờ", "HS ĐD Muộn giờ" };
            ViewModel = new StatsViewModel();
            BindingContext = ViewModel;
            FilterMode.SelectedIndex = 0;
        }

        private async void List_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is LogListForm logIdChose)) return;
            await Navigation.PushAsync(new LogChanger(logIdChose));
            LogList.SelectedItem = null;
        }
        private void RetrieveLogs()
        {
            ViewModel.LogListFirebase.Clear();
            if (FilterMode.SelectedIndex == 0)
            {
                int cntLoaded = 0;
                IDisposable Subscriber = fc.Child("Logging").OrderByKey().LimitToLast(10).AsObservable<LogListForm>().Subscribe(
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
                IDisposable Subscriber = fc.Child("Logging").OrderBy("LoginStatus").EqualTo(FilterMode.SelectedIndex)
                    .LimitToLast(10).AsObservable<LogListForm>().Subscribe(
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

        private void FilterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            RetrieveLogs();
        }
    }
}