using Plugin.CloudFirestore;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogChanger : ContentPage
    {
        public LogListForm globalLogList = new LogListForm();
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localLogHistDatabasePath);
        public DateTime EditingTime { get; set; }
        public LogChanger(LogListForm logList)
        {
            InitializeComponent();
            globalLogList = logList;
            ChoseLogId.Text = logList.Keys;
            ChoseString.Text = instance.RetrieveNameUser(logList.StId);
            ChoseString2.Text = instance.RetrieveNameUser(logList.ReporterId);
            StMistake.Text = logList.Mistake.Equals("NONE") ? "" : logList.Mistake;
            OnTime.IsChecked = false;
            LateTime.IsChecked = false;
            // TODO: Changeable interval via google sheets.
            if (UserData.StudentPriv >= 2)
            {
                OnTime.IsEnabled = true;
                LateTime.IsEnabled = true;
                StMistake.IsReadOnly = false;
                EditButton.IsVisible = true;
            }
            else if(UserData.StudentPriv == 1)
            {
                OnTime.IsEnabled = false;
                LateTime.IsEnabled = false;
                if (logList.LoginDate.AddMinutes(10) >= DateTime.Now + UserData.OffsetWithNIST)
                {
                    StMistake.IsReadOnly = false;
                    EditButton.IsVisible = true;
                }
                else
                {
                    StMistake.IsReadOnly = true;
                    EditButton.IsVisible = false;
                }
            }
            if (logList.LoginStatus == 1) OnTime.IsChecked = true;
            else if (logList.LoginStatus == 2) LateTime.IsChecked = true;
            EditingTime = logList.LoginDate;
            StTime.Text = logList.LoginDate.ToString("HH:mm:ss dd.MM.yyyy");
        }

        public async void Button_Clicked_1(object sender, System.EventArgs e)
        {
            if (UserData.StudentPriv == 1 && EditingTime.AddMinutes(10) < DateTime.Now + UserData.OffsetWithNIST)
            {
                DependencyService.Get<IToast>().ShowShort("Hết thời gian sửa");
                await Navigation.PopAsync();
                return;
            }
            if (string.IsNullOrEmpty(StMistake.Text) || string.IsNullOrWhiteSpace(StMistake.Text))
            {
                StMistake.Text = "NONE";
            }
            var NewLog = new
            {
                globalLogList.StId,
                ReporterId = UserData.StudentIdDatabase,
                Mistake = StMistake.Text,
                LoginStatus = OnTime.IsChecked ? 1 : 2,
                globalLogList.StClass,
                NoMistake = (OnTime.IsChecked ? 1 : 2) - 1 + (StMistake.Text.Equals("NONE") ? 0 : StMistake.Text.Count(x => x.Equals(';')) + 1)
            };
            string QueryName = ChoseString.Text;
            await CrossCloudFirestore.Current.Instance.Collection("logging").Document(globalLogList.Keys).UpdateAsync(NewLog);
            _ = await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "13",
                Contents = globalLogList.StId.ToString(),
                Contents2 = globalLogList.LoginDate.DayOfYear.ToString(),
                Contents3 = OnTime.IsChecked ? "1" : "2",
                Contents4 = StMistake.Text.Equals("NONE") ? "0" : (StMistake.Text.Count(x => x.Equals(';')) + 1).ToString(),
            }, false);
            DependencyService.Get<IToast>().ShowShort("Sửa thành công: " + QueryName);
            await Navigation.PopAsync();
        }

        public async void Button_Clicked_2(object sender, System.EventArgs e)
        {
            if (await DisplayActionSheet("Bạn có chắc chắn muốn xóa báo cáo không?", "Có", "Không") == "Có")
            {
                if (UserData.StudentPriv == 1 && EditingTime.AddMinutes(10) < DateTime.Now + UserData.OffsetWithNIST)
                {
                    DependencyService.Get<IToast>().ShowShort("Hết thời gian sửa");
                    await Navigation.PopAsync();
                    return;
                }
                string QueryName = ChoseString.Text;
                await CrossCloudFirestore.Current.Instance.Collection("logging").Document(globalLogList.Keys).DeleteAsync();
                _ = await instance.HttpPolly(new FeedbackModel()
                {
                    Mode = "13",
                    Contents = globalLogList.StId.ToString(),
                    Contents2 = globalLogList.LoginDate.DayOfYear.ToString(),
                    Contents3 = "",
                    Contents4 = "",
                }, false);
                DependencyService.Get<IToast>().ShowShort("Xóa thành công: " + QueryName);
                await Navigation.PopAsync();
            }
        }
    }
}