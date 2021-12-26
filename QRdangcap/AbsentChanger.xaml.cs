using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AbsentChanger : ContentPage
    {
        public AbsentLogForm globalLogList = new AbsentLogForm();
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();

        public AbsentChanger(AbsentLogForm logList)
        {
            InitializeComponent();
            globalLogList = logList;
            ChoseLogId.Text = logList.LogId.ToString();
            ChoseString.Text = instance.RetrieveNameUser(logList.StId);
            ChoseString2.Text = instance.RetrieveNameUser(logList.ReporterId);
            FromDate.Date = logList.DateCSD;
            ToDate.Date = logList.DateCED;
        }

        public async void Button_Clicked_1(object sender, System.EventArgs e)
        {
            string QueryName = ChoseString.Text;
            DependencyService.Get<IToast>().ShowShort("Đang sửa: " + QueryName);
            ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
            {
                Mode = "15",
                Contents = globalLogList.LogId.ToString(),
                Contents2 = UserData.StudentIdDatabase.ToString(),
                ContentStartTime = FromDate.Date.DayOfYear,
                ContentEndTime = ToDate.Date.DayOfYear,
            });
            AbsentLogForm LogChanged = new AbsentLogForm()
            {
                ChangeStat = 1,
                LogId = globalLogList.LogId,
                StId = globalLogList.StId,
                ContentStartDate = FromDate.Date.DayOfYear,
                ContentEndDate = ToDate.Date.DayOfYear,
                ReporterId = UserData.StudentIdDatabase,
            };
            if (response.Status == "SUCCESS")
            {
                MessagingCenter.Send<Page, AbsentLogForm>(this, "AbsentChangerEdit", LogChanged);
                DependencyService.Get<IToast>().ShowShort("Sửa thành công: " + QueryName);
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Thất bại (Không tồn tại): " + QueryName);
            }
            await Navigation.PopAsync();
        }

        public async void Button_Clicked_2(object sender, System.EventArgs e)
        {
            if (await DisplayActionSheet("Bạn có chắc chắn muốn xóa báo cáo không?", "Có", "Không") == "Có")
            {
                string QueryName = ChoseString.Text;
                DependencyService.Get<IToast>().ShowShort("Đang xóa: " + QueryName);
                ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
                {
                    Mode = "16",
                    Contents = globalLogList.LogId.ToString(),
                });
                AbsentLogForm LogChanged = new AbsentLogForm()
                {
                    ChangeStat = 2,
                };
                if (response.Status == "SUCCESS")
                {
                    MessagingCenter.Send<Page, AbsentLogForm>(this, "AbsentChangerEdit", LogChanged);
                    DependencyService.Get<IToast>().ShowShort("Xóa thành công: " + QueryName);
                }
                else
                {
                    DependencyService.Get<IToast>().ShowShort("Thất bại (Không tồn tại): " + QueryName);
                }
                await Navigation.PopAsync();
            }
        }
    }
}