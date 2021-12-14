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
    public class LearnRow
    {
        public string Thu2 { get; set; }
        public string Thu3 { get; set; }
        public string Thu4 { get; set; }
        public string Thu5 { get; set; }
        public string Thu6 { get; set; }
        public string Thu7 { get; set; }
    }

    public partial class TimetablePage : ContentPage
    {
        private readonly TimetablePageViewModel ViewModel;
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public List<string> ClassList = new List<string>();

        public TimetablePage()
        {
            InitializeComponent();
            ViewModel = new TimetablePageViewModel();
            BindingContext = ViewModel;
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            ClassList = db.Table<UserListForm>().Select(x => x.StClass).AsParallel().Distinct().ToList();
            db.Dispose();
            ClassList.Remove("Debug");
            ClassChose.ItemsSource = ClassList;
            ClassChose.SelectedIndex = Math.Max(0, ClassList.IndexOf(UserData.StudentClass));
        }

        private void Class_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshingView.IsRefreshing = true;
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            UpdateTimetable(ClassList[ClassChose.SelectedIndex]);
        }

        private async void UpdateTimetable(string ClassChosen)
        {
            if (ClassChosen == "Debug") return;
            ViewModel.TimetableColl.Clear();
            ViewModel.ClassName = "Đang tải dữ liệu";
            ViewModel.AddInfo = "";
            string[] response = (string[])await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "20",
                Contents = ClassChosen,
            }, true, "string[]", 5, "Không thấy dữ liệu!");
            ViewModel.ClassName = $"Lớp {ClassChosen}";
            ViewModel.AddInfo = $"Buổi sáng - Có hiệu lực từ ngày {response[0]}";
            for (int i = 1; i < 6; i++)
            {
                ViewModel.TimetableColl.Add(new LearnRow
                {
                    Thu2 = response[i].IndexOf("-") == -1 ? response[i] : response[i].Substring(0, response[i].IndexOf("-")).Trim(),
                    Thu3 = response[i + 5].IndexOf("-") == -1 ? response[i + 5] : response[i + 5].Substring(0, response[i + 5].IndexOf("-")).Trim(),
                    Thu4 = response[i + 10].IndexOf("-") == -1 ? response[i + 10] : response[i + 10].Substring(0, response[i + 10].IndexOf("-")).Trim(),
                    Thu5 = response[i + 15].IndexOf("-") == -1 ? response[i + 15] : response[i + 15].Substring(0, response[i + 15].IndexOf("-")).Trim(),
                    Thu6 = response[i + 20].IndexOf("-") == -1 ? response[i + 20] : response[i + 20].Substring(0, response[i + 20].IndexOf("-")).Trim(),
                    Thu7 = response[i + 25].IndexOf("-") == -1 ? response[i + 25] : response[i + 25].Substring(0, response[i + 25].IndexOf("-")).Trim(),
                });
            }
            RefreshingView.IsRefreshing = false;
        }
    }
}