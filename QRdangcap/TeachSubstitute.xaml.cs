using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class TeachSubstitution
    {
        public string BegDate { get; set; }
        public string EndDate { get; set; }
        public string SubDate { get; set; }
        public string RetireName { get; set; }
        public string Lesson { get; set; }
        public string ClassName { get; set; }
        public string LessonName { get; set; }
        public string ReplaceName { get; set; }
        public string RepLessonName { get; set; }
    }

    public partial class TeachSubstitute : ContentPage
    {
        private readonly TeachSubstituteViewModel ViewModel;
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();

        public TeachSubstitute()
        {
            InitializeComponent();
            ViewModel = new TeachSubstituteViewModel();
            BindingContext = ViewModel;
            RefreshingView.IsRefreshing = true;
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            UpdateTable();
        }

        private async void UpdateTable()
        {
            ViewModel.SubsColl.Clear();
            ViewModel.ClassName = "Đang tải dữ liệu";
            ViewModel.AddInfo = "";
            TeachSubstitution[] response = (TeachSubstitution[])await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "21",
            }, true, "TeachSubstitution[]", 5, "Không thấy dữ liệu!");
            ViewModel.ClassName = $"Lịch dạy thay";
            ViewModel.AddInfo = $"Từ ngày {response[0].BegDate} đến {response[0].EndDate}";
            foreach(TeachSubstitution i in response)
            {
                ViewModel.SubsColl.Add(i);
            }
            RefreshingView.IsRefreshing = false;
        }
    }
}