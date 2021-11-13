using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using Syncfusion.XForms.PopupLayout;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RestView : ContentPage
    {
        private readonly RestViewViewModel ViewModel;
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public List<int> ListOffDay = new List<int>();

        public RestView()
        {
            InitializeComponent();
            Schedule.CellTapped += Schedule_CellTapped;

            ViewModel = new RestViewViewModel();
            BindingContext = ViewModel;
            RefreshingView.IsRefreshing = true;
        }

        private void Schedule_CellTapped(object sender, Syncfusion.SfSchedule.XForms.CellTappedEventArgs e)
        {
            SfPopupLayout OverwritePopup = new SfPopupLayout();
            OverwritePopup.PopupView.HeaderTitle = $"Đặt trạng thái (ngày) {e.Datetime:dd.MM}";
            OverwritePopup.PopupView.AppearanceMode = AppearanceMode.TwoButton;
            OverwritePopup.PopupView.AcceptButtonText = "Xác nhận";
            OverwritePopup.PopupView.DeclineButtonText = "Hủy";
            OverwritePopup.PopupView.AnimationMode = AnimationMode.Fade;
            OverwritePopup.PopupView.PopupStyle.OverlayColor = Color.Black;
            OverwritePopup.PopupView.PopupStyle.OverlayOpacity = 0.35;
            OverwritePopup.BackgroundColor = new Color(230, 230, 230);
            OverwritePopup.PopupView.HeightRequest = 150;
            Switch RestDayState = new Switch { IsToggled = ListOffDay.Contains(e.Datetime.DayOfYear), HorizontalOptions = LayoutOptions.EndAndExpand, VerticalOptions = LayoutOptions.StartAndExpand };
            DataTemplate contentTemplateView = new DataTemplate(() =>
            {
                StackLayout popupContent = new StackLayout()
                {
                    Margin = new Thickness(10, 10),
                    Orientation = StackOrientation.Horizontal,
                    Children =
                    {
                        new Label {Text = "Ngày nghỉ?", Margin = new Thickness(10,0,0,-10), HorizontalTextAlignment=TextAlignment.Start},
                        RestDayState,
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
                DependencyService.Get<IToast>().ShowShort("Đang đặt...");
                ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
                {
                    Mode = "14",
                    Contents = "3",
                    Contents2 = RestDayState.IsToggled ? "0" : "1",
                    ContentStartTime = e.Datetime.Date.DayOfYear,
                    ContentEndTime = e.Datetime.Date.DayOfYear,
                });
                if (response.Status == "SUCCESS")
                {
                    if (RestDayState.IsToggled && !ListOffDay.Contains(e.Datetime.DayOfYear))
                    {
                        ListOffDay.Add(e.Datetime.DayOfYear);
                        RestDayForm TmpForm = new RestDayForm()
                        {
                            From = new DateTime(DateTime.Now.Year, 1, 1).Date.AddDays(e.Datetime.DayOfYear - 1).AddHours(7),
                            To = new DateTime(DateTime.Now.Year, 1, 1).Date.AddDays(e.Datetime.DayOfYear - 1).AddHours(8),
                            Color = Color.Red,
                        };
                        ViewModel.Schedules.Add(TmpForm);
                    }
                    else if (!RestDayState.IsToggled && ListOffDay.Contains(e.Datetime.DayOfYear))
                    {
                        ListOffDay.Remove(e.Datetime.DayOfYear);
                        RestDayForm TmpForm = new RestDayForm()
                        {
                            From = new DateTime(DateTime.Now.Year, 1, 1).Date.AddDays(e.Datetime.DayOfYear - 1).AddHours(7),
                            To = new DateTime(DateTime.Now.Year, 1, 1).Date.AddDays(e.Datetime.DayOfYear - 1).AddHours(8),
                            Color = Color.Red,
                        };
                        ViewModel.Schedules.Remove((RestDayForm)e.Appointments.FirstOrDefault());
                    }
                    DependencyService.Get<IToast>().ShowShort("Thành công!");
                }
                else
                {
                    DependencyService.Get<IToast>().ShowShort("Có lỗi xảy ra.");
                }
                return;
            }
            void PopupDecl()
            {
                return;
            }
        }

        public async void GetRestDay()
        {
            ObservableCollection<RestDayForm> ScheduleTmp = new ObservableCollection<RestDayForm>();
            ListOffDay = new List<int>((int[])await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "10",
            }, true, "int[]"));
            for (int i = 0; i < ListOffDay.Count; i++)
            {
                RestDayForm TmpForm = new RestDayForm()
                {
                    From = new DateTime(DateTime.Now.Year, 1, 1).Date.AddDays(ListOffDay[i] - 1).AddHours(7),
                    To = new DateTime(DateTime.Now.Year, 1, 1).Date.AddDays(ListOffDay[i] - 1).AddHours(8),
                    Color = Color.Red,
                };
                ScheduleTmp.Add(TmpForm);
            }
            ViewModel.Schedules = ScheduleTmp;
            RefreshingView.IsRefreshing = false;
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            GetRestDay();
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RestDay());
        }
    }
}