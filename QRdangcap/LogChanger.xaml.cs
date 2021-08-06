using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.GoogleDatabase;
using QRdangcap.LocalDatabase;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogChanger : ContentPage
    {
        public LogListForm globalLogList = new LogListForm();
        public FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);

        public LogChanger(LogListForm logList)
        {
            InitializeComponent();
            globalLogList = logList;
            ChoseLogId.Text = logList.Keys;
            ChoseString.Text = logList.StId.ToString() + " " + logList.StClass + " - " + logList.StName;
            StMistake.Text = logList.Mistake;
            OnTime.IsChecked = false;
            LateTime.IsChecked = false;
            if (logList.LoginStatus == 1) OnTime.IsChecked = true;
            else if (logList.LoginStatus == 2) LateTime.IsChecked = true;
            StTime.Text = logList.LoginDate.ToLocalTime().ToString("HH:mm:ss dd.MM.yyyy");
        }

        public async void Button_Clicked_1(object sender, System.EventArgs e)
        {
            InboundLog NewLog = new InboundLog()
            {
                StId = globalLogList.StId,
                Keys = globalLogList.Keys,
                ReporterId = UserData.StudentIdDatabase,
                Mistake = StMistake.Text,
                Timestamp = globalLogList.Timestamp,
                LoginStatus = OnTime.IsChecked ? 1 : 2,
            };
            string QueryName = ChoseString.Text;
            await fc.Child("Logging").Child(globalLogList.Keys).PutAsync(NewLog);
            DependencyService.Get<IToast>().ShowShort("Sửa thành công: " + QueryName);
            await Navigation.PopAsync();
        }

        public async void Button_Clicked_2(object sender, System.EventArgs e)
        {
            string QueryName = ChoseString.Text;
            await fc.Child("Logging").Child(globalLogList.Keys).DeleteAsync();
            DependencyService.Get<IToast>().ShowShort("Xóa thành công: " + QueryName);
            await Navigation.PopAsync();
        }
    }
}