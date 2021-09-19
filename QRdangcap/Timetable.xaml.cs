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
        private static readonly Color[] Pallette = { Color.Red, Color.BlueViolet, Color.OrangeRed, Color.DarkGreen, Color.Violet, Color.LightBlue };
        public int Stages = 0;
        public Timetable()
        {
            InitializeComponent();
            ViewModel = new TimetableViewModel();
            BindingContext = ViewModel;
            RefreshingView.IsRefreshing = true;
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
                var Absenting = new TimetableForm()
                {
                    From = response[i].DateCSD.Date.AddHours(10),
                    To = response[i].DateCED.Date.AddHours(10),
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
    }
}