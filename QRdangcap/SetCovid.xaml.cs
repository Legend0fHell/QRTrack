using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using Syncfusion.XForms.PopupLayout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetCovid : ContentPage
    {
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public ObservableRangeCollection<UserListForm> ItemsList { get; set; }
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
        public List<UserListForm> filteredItem = new List<UserListForm>();
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public int queryStat = -1;
        public bool UpdateStat { get; set; }
        private readonly Stopwatch SinceLastQuery = new Stopwatch();
        public bool Locked { get; set; }
        public SetCovid()
        {
            InitializeComponent();
            StateQuery.ItemsSource = new string[] { "T.Bộ", "-", "F0", "F1", "F2", "F3", "F4" };
            SinceLastQuery.Start();
            Locked = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if(SinceLastQuery.IsRunning && SinceLastQuery.ElapsedMilliseconds > 300)
                    {
                        SinceLastQuery.Stop();
                        if(!Locked) UpdateQuery();
                    }
                });
                return true;
            });
            
            Checking();
            async void Checking()
            {
                await instance.CheckUserTableExist();
            }
            UpdateStat = true;
            refreshAll.IsRefreshing = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            refreshAll.IsRefreshing = true;
        }

        public void Init()
        {
            Locked = true;
            ItemsList = new ObservableRangeCollection<UserListForm>();
            ItemsList.Clear();
            ItemsList.AddRange(db.Table<UserListForm>().ToList().Where(x => x.IsHidden == 0));
            myCollectionView.ItemsSource = ItemsList;
            Locked = false;
            BindingContext = this;
            refreshAll.IsRefreshing = false;
        }

        private void UpdateQuery()
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
            searchTerm = instance.ConvertToUnsign(searchTerm).ToLower();
            classSearchTerm = instance.ConvertToUnsign(classSearchTerm).ToLower();

            List<UserListForm> UserList = new List<UserListForm>(db.Table<UserListForm>().ToList().Where(x => x.IsHidden == 0));
            filteredItem = UserList;
            if (StateQuery.SelectedIndex > 0)
            {
                filteredItem = UserList.Where(x => x.CovidExposure.Equals(StateQuery.SelectedItem.ToString())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredItem = filteredItem.Where(x => x.UnsignStName.Contains(searchTerm)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(classSearchTerm))
            {
                filteredItem = filteredItem.Where(x => x.UnsignStClass.Contains(classSearchTerm)).ToList();
            }
            queryStat = 0;
            if (filteredItem.Count > 0) queryStat = 1;
            ItemsList.Clear();
            ItemsList.AddRange(filteredItem);
        }

        private void Test_TextChanged(object sender, TextChangedEventArgs e)
        {
            SinceLastQuery.Restart();
        }

        private async void RefreshView_Refreshing(object sender, EventArgs e)
        {
            if (UpdateStat)
            {
                await instance.RetrieveAllUserDatabase();
            }
            Init();
            if (UpdateStat)
            {
                StateQuery.SelectedIndex = 0;
            }
            else
            {
                UpdateQuery();
            }
            UpdateStat = false;
        }

        private void MyCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is UserListForm userIdChose)) return;
            else ChangeConfirm(userIdChose);
            myCollectionView.SelectedItem = null;
        }

        private async void ChangeConfirm(UserListForm usr)
        {
            UserListForm CurStatus = await fc.Child("Users").Child($"{usr.Username}").OnceSingleAsync<UserListForm>();
            SfPopupLayout OverwritePopup = new SfPopupLayout();
            OverwritePopup.PopupView.HeaderTitle = "Đổi trạng thái tiếp xúc";
            OverwritePopup.PopupView.AppearanceMode = AppearanceMode.TwoButton;
            OverwritePopup.PopupView.AcceptButtonText = "Xác nhận";
            OverwritePopup.PopupView.DeclineButtonText = "Hủy";
            OverwritePopup.PopupView.AnimationMode = AnimationMode.Fade;
            OverwritePopup.PopupView.PopupStyle.OverlayColor = Color.Black;
            OverwritePopup.PopupView.PopupStyle.OverlayOpacity = 0.35;
            OverwritePopup.BackgroundColor = new Color(230, 230, 230);
            OverwritePopup.PopupView.HeightRequest = 320;
            Picker StatePicker = new Picker { Title = "Diện tiếp xúc...", ItemsSource = new string[] { "-", "F0", "F1", "F2", "F3", "F4" }, SelectedItem = CurStatus.CovidExposure };
            Entry SourceExposure = new Entry { Placeholder = "Tiếp xúc với...", HorizontalOptions = LayoutOptions.FillAndExpand, Text = CurStatus.CovidExposureFrom };
            Button ResetDefault = new Button { Text = "Đặt lại dữ liệu", HorizontalOptions = LayoutOptions.FillAndExpand };
            ResetDefault.Clicked += ResetDefault_Clicked;
            DataTemplate contentTemplateView = new DataTemplate(() =>
            {
                StackLayout popupContent = new StackLayout()
                {
                    Margin = new Thickness(10, 10),
                    Children =
                    {
                        new Label {Text = $"Học sinh: {usr.StClass} - {usr.StName}", Margin = new Thickness(10,0,0,10), FontSize = 17},
                        new Label {Text = "Diện tiếp xúc...", Margin = new Thickness(10,0,0,-10)},
                        StatePicker,
                        new Label {Text = "Tiếp xúc với:", Margin = new Thickness(10,0,0,-10)},
                        SourceExposure,
                        ResetDefault,
                    }
                };
                return popupContent;
            });
            OverwritePopup.PopupView.ContentTemplate = contentTemplateView;
            OverwritePopup.ClosePopupOnBackButtonPressed = true;
            OverwritePopup.PopupView.AcceptCommand = new Command(PopupAcp);
            OverwritePopup.PopupView.DeclineCommand = new Command(PopupDecl);
            OverwritePopup.Show();
            async void PopupAcp()
            {
                if (StatePicker.SelectedItem != null)
                {
                    bool Operation = true;
                    try
                    {
                        await fc.Child("Users").Child($"{usr.Username}").PatchAsync(new {
                            CovidExposure = StatePicker.SelectedItem.ToString(),
                            CovidExposureFrom = string.IsNullOrWhiteSpace(SourceExposure.Text) ? "Không" : SourceExposure.Text
                        });
                    }
                    catch (Exception ex)
                    {
                        Operation = false;
                        DependencyService.Get<IToast>().ShowShort($"Thất bại: Có lỗi xảy ra. {ex}");
                    }
                    if (Operation)
                    {
                        DependencyService.Get<IToast>().ShowShort("Đổi diện thành công.");
                        usr.CovidExposure = StatePicker.SelectedItem.ToString();
                        usr.CovidExposureFrom = SourceExposure.Text;
                        db.Update(usr);
                        refreshAll.IsRefreshing = true;
                    }
                }
                else
                {
                    DependencyService.Get<IToast>().ShowShort("Thất bại: Nội dung không hợp lệ.");
                }
            }
            return;
            void PopupDecl()
            {
                return;
            }
            void ResetDefault_Clicked(object sender, EventArgs e)
            {
                StatePicker.SelectedIndex = 0;
                SourceExposure.Text = "Không";
            }
        }

        private void LoginStatUpdate_Clicked(object sender, EventArgs e)
        {
            UserData.ForcedStatusUpdate = true;
            UpdateStat = true;
            refreshAll.IsRefreshing = true;
        }

        private void StateQuery_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateQuery();
        }
    }
}