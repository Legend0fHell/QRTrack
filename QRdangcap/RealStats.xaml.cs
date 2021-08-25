using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RealStats : ContentPage
    {
        private readonly Stopwatch excTime = new Stopwatch();
        public object MonthSelected { get; set; }
        public ObservableCollection<ChartForm> DoughnutSeriesData { get; set; }
        public RetrieveAllUserDb instance = new RetrieveAllUserDb();

        public RealStats()
        {
            InitializeComponent();
            DatePicked.MaximumDate = DateTime.Now;
            EndDatePicked.MaximumDate = DateTime.Now;
            StartDatePicked.MaximumDate = DateTime.Now;
            WatchMode.ItemsSource = new List<string>() { "Ngày", "Tháng", "Cụ thể" };
            WatchMode.SelectedIndex = 0;
            ObservableCollection<string> Headers = new ObservableCollection<string>()
            {
                "Tháng", "Năm"
            };
            ObservableCollection<object> Cnt = new ObservableCollection<object>();
            ObservableCollection<int> Monthh = new ObservableCollection<int>();
            ObservableCollection<int> Yearr = new ObservableCollection<int>()
            {
                DateTime.Now.Year-1, DateTime.Now.Year,
            };
            for (int i = 1; i <= 12; ++i)
            {
                Monthh.Add(i);
            }
            Cnt.Add(Monthh);
            Cnt.Add(Yearr);
            MonthPicker.ItemsSource = Cnt;
            MonthPicker.ColumnHeaderText = Headers;
            MonthPicker.SelectedItem = new ObservableCollection<object>()
            {
                DateTime.Now.Month, DateTime.Now.Year
            };
        }

        private int StOnTime = 0, StNum = 0, StLateTime = 0, StNotYet = 0, StAbsent = 0;

        // TODO: Config these button to work based on current mode.
        private void PrevDate_Clicked(object sender, EventArgs e)
        {
            DatePicked.Date = DatePicked.Date.Subtract(TimeSpan.FromDays(1));
        }

        private void NextDate_Clicked(object sender, EventArgs e)
        {
            if (DatePicked.Date < DateTime.Now)
            {
                DatePicked.Date = DatePicked.Date.Add(TimeSpan.FromDays(1));
            }
            // TODO: Check if out of bounds, if thats true, prevent them.
        }

        private async void Details_Clicked(object sender, EventArgs e)
        {
            if (WatchMode.SelectedIndex == 0)
            {
                var ChoosePage = new DSchoolInfo(DatePicked.Date.DayOfYear, DatePicked.Date.DayOfYear);
                await Navigation.PushAsync(ChoosePage);
            }
            else if (WatchMode.SelectedIndex == 1)
            {
                ObservableCollection<object> source = MonthPicker.SelectedItem as ObservableCollection<object>;
                DateTime curBeginSelection = new DateTime((int)source[1], (int)source[0], 1);
                DateTime curEndSelection = new DateTime((int)source[1], (int)source[0],
                    (DateTime.Now.Month > (int)source[0]) ? DateTime.DaysInMonth((int)source[1], (int)source[0]) : DateTime.Now.Day);
                MonthView.Text = curBeginSelection.ToString("dd.MM.yyyy") + " - " + curEndSelection.ToString("dd.MM.yyyy");
                var ChoosePage = new DSchoolInfo(curBeginSelection.DayOfYear, curEndSelection.DayOfYear);
                await Navigation.PushAsync(ChoosePage);
            }
            else
            {
                var ChoosePage = new DSchoolInfo(StartDatePicked.Date.DayOfYear, EndDatePicked.Date.DayOfYear);
                await Navigation.PushAsync(ChoosePage);
            }
        }

        private async void YourClassInfo_Tapped(object sender, EventArgs e)
        {
            var ChoosePage = new DClassInfo(UserData.StudentClass);
            await Navigation.PushAsync(ChoosePage);
        }

        private async void ClrList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is ClassroomListForm ClrUserChose)) return;
            var ChoosePage = new DClassInfo(ClrUserChose.ClrName);
            await Navigation.PushAsync(ChoosePage);
            ClrList.SelectedItem = null;
        }

        private void WatchMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (WatchMode.SelectedIndex == 0)
            {
                MonthView.IsVisible = false;
                DailyView.IsVisible = true;
                CustomView.IsVisible = false;
                refreshAll.IsRefreshing = true;
            }
            else if (WatchMode.SelectedIndex == 1)
            {
                MonthView.IsVisible = true;
                DailyView.IsVisible = false;
                CustomView.IsVisible = false;
                refreshAll.IsRefreshing = true;
            }
            else
            {
                MonthView.IsVisible = false;
                DailyView.IsVisible = false;
                CustomView.IsVisible = true;
                refreshAll.IsRefreshing = true;
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            MonthPicker.IsOpen = !MonthPicker.IsOpen;
        }

        private void MonthPicker_OkButtonClicked(object sender, Syncfusion.SfPicker.XForms.SelectionChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                refreshAll.IsRefreshing = true;
                MonthPicker.IsOpen = !MonthPicker.IsOpen;
            }
        }

        private void DatePicked_DateSelected(object sender, DateChangedEventArgs e)
        {
            refreshAll.IsRefreshing = true;
        }

        public async void UpdateChart(int StartTime, int EndTime)
        {
            excTime.Reset();
            excTime.Start();
            List<ClassroomListForm[]> response = (List<ClassroomListForm[]>)await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "8",
                ContentStartTime = StartTime,
                ContentEndTime = EndTime
            }, true, "List<ClassroomListForm[]>");
            if (response[0][0].ClrName.Equals("NoInfo"))
            {
                Frame_Chart1.IsVisible = false;
                Frame_Chart2.IsVisible = false;
                Frame_Chart3.IsVisible = false;
                Frame_Chart4.IsVisible = false;
                Frame_Info.IsVisible = true;
                refreshAll.IsRefreshing = false;
                return;
            }
            else
            {
                Frame_Chart1.IsVisible = true;
                Frame_Chart2.IsVisible = true;
                Frame_Chart3.IsVisible = true;
                Frame_Chart4.IsVisible = true;
                Frame_Info.IsVisible = false;
            }
            List<ClassroomListForm> classroomListForms = new List<ClassroomListForm>();
            for (int classes = 0; classes < response[0].Count(); ++classes)
            {
                ClassroomListForm tmpForm = new ClassroomListForm()
                {
                    ClrName = response[0][classes].ClrName,
                    ClrNoSt = response[0][classes].ClrNoSt * response.Count(),
                };
                for (int i = 0; i < response.Count(); ++i)
                {
                    tmpForm.ClrOnTime += response[i][classes].ClrOnTime;
                    tmpForm.ClrLateTime += response[i][classes].ClrLateTime;
                    tmpForm.ClrAbsent += response[i][classes].ClrAbsent;
                    tmpForm.ClrError += response[i][classes].ClrError;
                }
                classroomListForms.Add(tmpForm);
            }
            ClrList.ItemsSource = classroomListForms.Skip(1);
            StNum = classroomListForms[0].ClrNoSt;
            StOnTime = classroomListForms[0].ClrOnTime;
            StLateTime = classroomListForms[0].ClrLateTime;
            StNotYet = classroomListForms[0].ClrNotYet;
            StAbsent = classroomListForms[0].ClrAbsent;
            excTime.Stop();

            DoughnutSeriesData = new ObservableCollection<ChartForm>()
            {
                new ChartForm("Trường (" + StNum.ToString() + ")", 0, StNum),
                new ChartForm("Đúng giờ", StOnTime, StNum),
                new ChartForm("Muộn giờ", StLateTime, StNum),
                new ChartForm("Báo nghỉ", StAbsent, StNum),
                new ChartForm("Chưa ĐD", StNotYet, StNum),
            };
            DoughnutAllSchool.ItemsSource = DoughnutSeriesData;
            usrClassDesc.Text = "Lớp của bạn (" + UserData.StudentClass + ")";
            ClassroomListForm usrClass = classroomListForms.Find(x => x.ClrName.Equals(UserData.StudentClass));
            series1.Label = "Lớp (" + usrClass.ClrNoSt + ")";
            series2.Label = "Đúng giờ (" + usrClass.ClrOnTime + ")";
            series3.Label = "Muộn giờ (" + usrClass.ClrLateTime + ")";
            series4.Label = "Báo nghỉ (" + usrClass.ClrAbsent + ")";
            series5.Label = "Chưa ĐD (" + usrClass.ClrNotYet + ")";
            series1.ItemsSource = new ObservableCollection<ChartForm>()
            {
                new ChartForm("Lớp", 0, usrClass.ClrNoSt),
            };
            series2.ItemsSource = new ObservableCollection<ChartForm>()
            {
                new ChartForm("Lớp", usrClass.ClrOnTime, usrClass.ClrNoSt),
            };
            series3.ItemsSource = new ObservableCollection<ChartForm>()
            {
                new ChartForm("Lớp", usrClass.ClrLateTime, usrClass.ClrNoSt),
            };
            series4.ItemsSource = new ObservableCollection<ChartForm>()
            {
                new ChartForm("Lớp", usrClass.ClrAbsent, usrClass.ClrNoSt),
            };
            series5.ItemsSource = new ObservableCollection<ChartForm>()
            {
                new ChartForm("Lớp", usrClass.ClrNotYet, usrClass.ClrNoSt),
            };
            ObservableCollection<ChartForm> ClassErrorList = new ObservableCollection<ChartForm>();
            for (int classes = 1; classes < response[0].Count(); ++classes)
            {
                ClassErrorList.Add(new ChartForm(classroomListForms[classes].ClrName, classroomListForms[classes].ClrError, classroomListForms[classes].ClrNoSt));
            }
            ErrorRateSeries.ItemsSource = ClassErrorList.OrderByDescending(x => x.Value);
            refreshAll.IsRefreshing = false;
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            if (WatchMode.SelectedIndex == 0)
            {
                UpdateChart(DatePicked.Date.DayOfYear, DatePicked.Date.DayOfYear);
            }
            else if (WatchMode.SelectedIndex == 1)
            {
                ObservableCollection<object> source = MonthPicker.SelectedItem as ObservableCollection<object>;
                DateTime curBeginSelection = new DateTime((int)source[1], (int)source[0], 1);
                DateTime curEndSelection = new DateTime((int)source[1], (int)source[0],
                    (DateTime.Now.Month > (int)source[0]) ? DateTime.DaysInMonth((int)source[1], (int)source[0]) : DateTime.Now.Day);
                MonthView.Text = curBeginSelection.ToString("dd.MM.yyyy") + " - " + curEndSelection.ToString("dd.MM.yyyy");
                UpdateChart(curBeginSelection.DayOfYear, curEndSelection.DayOfYear);
            }
            else
            {
                UpdateChart(StartDatePicked.Date.DayOfYear, EndDatePicked.Date.DayOfYear);
            }
        }
    }
}