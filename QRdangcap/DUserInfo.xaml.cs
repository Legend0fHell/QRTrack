using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using System;
using System.Collections.Generic;
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
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public UserListForm StChose;
        private readonly DUserInfoViewModel ViewModel;
        public IDisposable Subscriber;
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

        public void UpdateLog(int StartingPoint = 0)
        {
            if (FilterMode.SelectedIndex == 0)
            {
                RetrieveAllUserDb instance = new RetrieveAllUserDb();
                string ConvStId = instance.To4DigitString(StChose.StId);
                string ThisStartIdenId = ConvStId + "_0000_0000";
                string ThisEndInitIdenId = ConvStId + "_9999_9999";
                Subscriber = fc.Child("Logging").OrderBy("IdentityId").StartAt(ThisStartIdenId)
                    .EndAt(ViewModel.LogListFirebase.Count == 0 ? ThisEndInitIdenId : ConvStId + "_" +
                    ViewModel.LogListFirebase.Last().LoginDate.Year.ToString() + "_" + instance.To4DigitString(ViewModel.LogListFirebase.Last().LoginDate.DayOfYear))
                    .LimitToLast(Step).AsObservable<LogListForm>().Subscribe(
                    x =>
                    {
                        int index = ViewModel.LogListFirebase.IndexOf(x.Object);
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
                                        LogList.ScrollTo(StartingPoint + 1);
                                    }
                                }
                                else
                                {
                                    ViewModel.LogListFirebase.Insert(StartingPoint, x.Object);
                                    ++cntLoaded;
                                    LogList.ScrollTo(StartingPoint + 1);
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
                        ViewModel.RetrieveLog = $"Đã tải {cntLoaded}/?? mục. (Dữ liệu cập nhật tự động)";

                        ViewModel.IsVisi = (cntLoaded >= ExpectLoadedLog);
                    }
                );
            }
            else
            {
                RetrieveAllUserDb instance = new RetrieveAllUserDb();
                string ConvStId = FilterMode.SelectedIndex.ToString() + "_" + instance.To4DigitString(StChose.StId);
                string ThisStartIdenId = ConvStId + "_0000000000000";
                string ThisEndInitIdenId = ConvStId + "_9999999999999";
                Subscriber = fc.Child("Logging").OrderBy("Id2Id").StartAt(ThisStartIdenId)
                    .EndAt(ViewModel.LogListFirebase.Count == 0 ? ThisEndInitIdenId : ConvStId + "_" + ViewModel.LogListFirebase.Last().Timestamp)
                    .LimitToLast(Step).AsObservable<LogListForm>().Subscribe(
                    x =>
                    {
                        int index = ViewModel.LogListFirebase.IndexOf(x.Object);
                        if (x.Object.LoginStatus != FilterMode.SelectedIndex)
                        {
                            Debug.WriteLine($" Firebase update (index {index}): {x.EventType} - DETECTED WRONG TYPE, DELETING..");
                            ViewModel.LogListFirebase.RemoveAt(index);
                            --cntLoaded;
                        }
                        else
                        {
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
                                            LogList.ScrollTo(StartingPoint + 1);
                                        }
                                    }
                                    else
                                    {
                                        ViewModel.LogListFirebase.Insert(StartingPoint, x.Object);
                                        ++cntLoaded;
                                        LogList.ScrollTo(StartingPoint + 1);
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
                        ViewModel.RetrieveLog = $"Đã tải {cntLoaded}/?? mục. (Dữ liệu cập nhật tự động)";
                        ViewModel.IsVisi = (cntLoaded >= ExpectLoadedLog);
                    }
                );
            }
            FirstTime = false;
        }

        private async void List_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is LogListForm logIdChose)) return;
            if(ViewModel.IsEditAllowed) await Navigation.PushAsync(new LogChanger(logIdChose));
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