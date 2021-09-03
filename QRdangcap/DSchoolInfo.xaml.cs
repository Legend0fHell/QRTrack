using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DSchoolInfo : ContentPage
    {
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public ObservableRangeCollection<ClassroomListForm> ItemsList { get; set; }
        public int globalSortStrat = -1;
        public int startDate = DateTime.Now.DayOfYear;
        public int endDate = DateTime.Now.DayOfYear;
        public List<ClassroomListForm> classroomListForms = new List<ClassroomListForm>();

        public DSchoolInfo(int startDate2, int endDate2)
        {
            InitializeComponent();
            string[] SortingMode = { "Tên", "Số HS" };
            SortMode.ItemsSource = SortingMode.ToList();
            SortMode.SelectedIndex = 0;
            startDate = startDate2;
            endDate = endDate2;
            RefreshAll.IsRefreshing = true;
        }

        private async void ClrList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.CurrentSelection.FirstOrDefault() is ClassroomListForm ClrUserChose)) return;
            var ChoosePage = new DClassInfo(ClrUserChose.ClrName);
            await Navigation.PushAsync(ChoosePage);
            ClrList.SelectedItem = null;
        }

        public async void UpdateSumLog(int SortStrat)
        {
            if (SortStrat == globalSortStrat)
            {
                RefreshAll.IsRefreshing = false;
                return;
            }
            List<ClassroomListForm[]> response = (List<ClassroomListForm[]>)await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "8",
                ContentStartTime = startDate,
                ContentEndTime = endDate
            }, true, "List<ClassroomListForm[]>");
            classroomListForms = new List<ClassroomListForm>();
            for (int classes = response[0][0].ClrName.Equals("NoInfo") ? 1 : 0; classes < response[0].Count(); ++classes)
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
                }
                classroomListForms.Add(tmpForm);
            }
            ItemsList = new ObservableRangeCollection<ClassroomListForm>();
            if (SortStrat == 0)
            {
                ItemsList.AddRange(classroomListForms.Skip(1).OrderBy(x => x.ClrName));
            }
            else
            {
                ItemsList.AddRange(classroomListForms.Skip(1).OrderByDescending(x => x.ClrNoSt));
            }
            globalSortStrat = SortStrat;
            ClrList.ItemsSource = ItemsList;
            RefreshAll.IsRefreshing = false;
        }

        private void RefreshAll_Refreshing(object sender, EventArgs e)
        {
            UpdateSumLog(SortMode.SelectedIndex);
        }

        private void SortMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshAll.IsRefreshing = true;
            ItemsList = new ObservableRangeCollection<ClassroomListForm>();
            if (SortMode.SelectedIndex == 0)
            {
                ItemsList.AddRange(classroomListForms.Skip(1).OrderBy(x => x.ClrName));
            }
            else
            {
                ItemsList.AddRange(classroomListForms.Skip(1).OrderByDescending(x => x.ClrNoSt));
            }
            globalSortStrat = SortMode.SelectedIndex;
            ClrList.ItemsSource = ItemsList;
        }
    }
}