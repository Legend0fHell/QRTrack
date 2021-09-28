using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using System;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Timetable : ContentPage
    {
        private readonly TimetableViewModel ViewModel;
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        private static readonly Color[] Pallette = { Color.Red, Color.BlueViolet, Color.OrangeRed, Color.DarkGreen, Color.Violet, Color.Blue };
        public int Stages = 0;
        public TimetableForm LogBeingChanged { get; set; }

        public Timetable()
        {
            InitializeComponent();

            Schedule.MonthInlineAppointmentTapped += Schedule_MonthInlineAppointmentTapped;
            ViewModel = new TimetableViewModel();
            BindingContext = ViewModel;
            RefreshingView.IsRefreshing = true;
            MessagingCenter.Subscribe<Page, AbsentLogForm>(this, "AbsentChangerEdit", (p, LogChanged) =>
            {
                if (LogChanged.ChangeStat <= 2)
                {
                    int LogIndex = ViewModel.Timetables.IndexOf(LogBeingChanged);
                    ViewModel.Timetables.RemoveAt(LogIndex);
                    LogBeingChanged.ContentStartDate = LogChanged.ContentStartDate;
                    LogBeingChanged.ContentEndDate = LogChanged.ContentEndDate;
                    LogBeingChanged.ReporterId = LogChanged.ReporterId;
                    LogBeingChanged.From = LogChanged.DateCSD.Date;
                    LogBeingChanged.To = LogChanged.DateCED.Date.Add(new TimeSpan(0, 23, 59, 59));
                    ViewModel.Timetables.Add(LogBeingChanged);
                }
                else
                {
                    var randomGen = new Random();
                    TimetableForm Absenting = new TimetableForm()
                    {
                        LogId = LogChanged.LogId,
                        StId = LogChanged.StId,
                        ContentStartDate = LogChanged.ContentStartDate,
                        ContentEndDate = LogChanged.ContentEndDate,
                        ReporterId = LogChanged.ReporterId,
                        From = LogChanged.DateCSD.Date,
                        To = LogChanged.DateCED.Date.Add(new TimeSpan(0, 23, 59, 59)),
                        Name = $"{LogChanged.StClass} - {LogChanged.StName}",
                        Color = Pallette[randomGen.Next(6)],
                    };
                    ViewModel.Timetables.Add(Absenting);
                }
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.IsAbsentAllowed = UserData.StudentPriv >= 2;
        }

        private async void Schedule_MonthInlineAppointmentTapped(object sender, Syncfusion.SfSchedule.XForms.MonthInlineAppointmentTappedEventArgs e)
        {
            if (ViewModel.IsAbsentAllowed)
            {
                LogBeingChanged = e.Appointment as TimetableForm;
                AbsentLogForm appointment = e.Appointment as AbsentLogForm;
                await Navigation.PushAsync(new AbsentChanger(appointment));
            }
            else return;
        }

        public async void GetAbsent()
        {
            ObservableCollection<DateTime> BlackoutDatesTmp = new ObservableCollection<DateTime>();
            int[] ListOffDay = (int[])await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "10",
            }, true, "int[]");
            for (int i = 0; i < ListOffDay.Length; i++)
            {
                BlackoutDatesTmp.Add(new DateTime(DateTime.Now.Year, 1, 1).Date.AddDays(ListOffDay[i] - 1));
            }
            ViewModel.BlackoutDates = BlackoutDatesTmp;
            ViewModel.Timetables = new ObservableCollection<TimetableForm>();
            AbsentLogForm[] response = (AbsentLogForm[])await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "17",
            }, true, "AbsentLogForm[]", 5, "Không có dữ liệu!");
            var randomGen = new Random();
            for (int i = 0; i < response.Length; i++)
            {
                TimetableForm Absenting = new TimetableForm()
                {
                    LogId = response[i].LogId,
                    StId = response[i].StId,
                    ContentStartDate = response[i].ContentStartDate,
                    ContentEndDate = response[i].ContentEndDate,
                    ReporterId = response[i].ReporterId,
                    From = response[i].DateCSD.Date,
                    To = response[i].DateCED.Date.Add(new TimeSpan(0, 23, 59, 59)),
                    Name = $"{response[i].StClass} - {response[i].StName}",
                    Color = Pallette[randomGen.Next(6)],
                };
                ViewModel.Timetables.Add(Absenting);
            }
            RefreshingView.IsRefreshing = false;
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            GetAbsent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SendAbsent());
        }
    }
}