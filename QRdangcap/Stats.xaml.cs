using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Stats : ContentPage
    {
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        private readonly StatsViewModel ViewModel;
        public IDisposable Subscriber;
        public bool FirstTime = true;
        public int cntLoaded = 0;
        public int Step = 10;
        public int ExpectLoadedLog { get; set; }

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
            if (UserData.StudentPriv > 0) await Navigation.PushAsync(new LogChanger(logIdChose));
            LogList.SelectedItem = null;
        }

        private void RetrieveLogs(int StartingPoint = 0)
        {
            if (FilterMode.SelectedIndex == 0)
            {
                Subscriber = fc.Child("Logging").OrderByKey()
                    .EndAt(ViewModel.LogListFirebase.Count > 0 ? ViewModel.LogListFirebase.Last().Keys : "ZZZZZZZZZZZZZZZZZZZZ")
                    .LimitToLast(Step).AsObservable<LogListForm>().Subscribe(
                    x =>
                    {
                        int index = ViewModel.LogListFirebase.IndexOf(x.Object);
                        System.Diagnostics.Debug.WriteLine($" Firebase update at 1 (index {index}): {x.EventType} - {x.Object}");
                        if (x.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                        {
                            if (index < 0)
                            {
                                // got this weird error but ok heres the hack
                                if (StartingPoint - 1 >= 0)
                                {
                                    if (ViewModel.LogListFirebase[StartingPoint - 1].Keys != x.Key)
                                    {
                                        ViewModel.LogListFirebase.Insert(StartingPoint, x.Object);
                                        ++cntLoaded;
                                    }
                                }
                                else
                                {
                                    ViewModel.LogListFirebase.Insert(StartingPoint, x.Object);
                                    ++cntLoaded;
                                }
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
                        LogList.ScrollTo(StartingPoint + 1);
                        ViewModel.RetrieveLog = $"Đã tải {cntLoaded}/?? mục. (Dữ liệu cập nhật tự động)";

                        ViewModel.IsVisi = (cntLoaded >= ExpectLoadedLog);
                    }
                );
            }
            else
            {
                RetrieveAllUserDb instance = new RetrieveAllUserDb();
                string ConvStId = FilterMode.SelectedIndex.ToString();
                string ThisStartIdenId = ConvStId + "_0000_0000000000000";
                string ThisEndInitIdenId = ConvStId + "_9999_9999999999999";
                Subscriber = fc.Child("Logging").OrderBy("Id2Id").StartAt(ThisStartIdenId)
                    .EndAt(ViewModel.LogListFirebase.Count == 0 ? ThisEndInitIdenId : ConvStId + "_"
                    + instance.To4DigitString(ViewModel.LogListFirebase.Last().StId) + "_" + ViewModel.LogListFirebase.Last().Timestamp)
                    .LimitToLast(Step).AsObservable<LogListForm>().Subscribe(
                    x =>
                    {
                        int index = ViewModel.LogListFirebase.IndexOf(x.Object);

                        if (x.Object.LoginStatus != FilterMode.SelectedIndex)
                        {
                            System.Diagnostics.Debug.WriteLine($" Firebase update (index {index}): {x.EventType} - DETECTED WRONG TYPE, DELETING..");
                            ViewModel.LogListFirebase.RemoveAt(index);
                            --cntLoaded;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($" Firebase update (index {index}): {x.EventType} - {x.Object.LoginStatus}");
                            if (x.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                            {
                                if (index < 0)
                                {
                                    // got this weird error but ok heres the hack
                                    if (StartingPoint - 1 >= 0)
                                    {
                                        if (ViewModel.LogListFirebase[StartingPoint - 1].Keys != x.Key)
                                        {
                                            ViewModel.LogListFirebase.Insert(StartingPoint, x.Object);
                                            ++cntLoaded;
                                        }
                                    }
                                    else
                                    {
                                        ViewModel.LogListFirebase.Insert(StartingPoint, x.Object);
                                        ++cntLoaded;
                                    }
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
                        }
                        LogList.ScrollTo(StartingPoint + 1);
                        ViewModel.RetrieveLog = $"Đã tải {cntLoaded}/?? mục. (Dữ liệu cập nhật tự động)";
                        ViewModel.IsVisi = (cntLoaded >= ExpectLoadedLog);
                    }
                );
            }
            FirstTime = false;
        }

        private void FilterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            cntLoaded = 0;
            ExpectLoadedLog = Step;
            ViewModel.LogListFirebase.Clear();
            if (!FirstTime) Subscriber.Dispose();
            RetrieveLogs();
        }

        private void LoadMoreData_Clicked(object sender, EventArgs e)
        {
            ExpectLoadedLog += Step;
            RetrieveLogs(ViewModel.LogListFirebase.Count);
        }
    }
}