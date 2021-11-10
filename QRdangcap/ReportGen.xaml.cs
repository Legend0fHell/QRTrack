using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using SQLite;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportGen : ContentPage
    {
        private readonly ReportGenViewModel ViewModel;
        
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public IDisposable Subscriber;
        public List<UserListForm> ItemsList = new List<UserListForm>();
        public List<string> ClassList = new List<string>();
        public List<string> ModeList = new List<string>();
        public string ThisId3IdHB { get; set; }
        public string ThisId3IdHE { get; set; }
        public string ClassReport { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ModeUsing { get; set; }
        public ReportGen()
        {
            InitializeComponent();
            ViewModel = new ReportGenViewModel();
            BindingContext = ViewModel;
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            ClassList = db.Table<UserListForm>().Select(x => x.StClass).AsParallel().Distinct().ToList();
            ClassList.Insert(0, "Toàn bộ");
            ClassChose.ItemsSource = ClassList;
            ClassChose.SelectedIndex = 0;
            ModeList = new List<string> { "Phạm lỗi", "Không phạm lỗi", "Toàn bộ" };
            ModeChose.ItemsSource = ModeList;
            ModeChose.SelectedIndex = 0;
            FromDate.Date = DateTime.Now;
            ToDate.Date = DateTime.Now;
        }
        private void Class_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClassReport = ClassList[ClassChose.SelectedIndex];
            if(ClassChose.SelectedIndex != 0)
            {
                ThisId3IdHB = instance.To6DigitString(ClassReport) + "_";
                ThisId3IdHE = ThisId3IdHB;
            }
            else
            {
                ThisId3IdHB = "000000_";
                ThisId3IdHE = "ZZZZZZ_";
            }
        }
        private void ModeChose_SelectedIndexChanged(object sender, EventArgs e)
        {
            ModeUsing = ModeChose.SelectedIndex;
        }
        private void Button_Clicked(object sender, EventArgs e)
        {
            Stopwatch excTime = new Stopwatch();
            excTime.Reset();
            excTime.Start();
            BeginTime = FromDate.Date.Date;
            EndTime = ToDate.Date.Date.AddSeconds(86399);
            string ThisId3IdBegin = ThisId3IdHB + (BeginTime.Subtract(new DateTime(1970, 1, 1)).Ticks / TimeSpan.TicksPerMillisecond).ToString();
            string ThisId3IdEnd = ThisId3IdHE + (EndTime.Subtract(new DateTime(1970, 1, 1)).Ticks / TimeSpan.TicksPerMillisecond).ToString();
            int Counter = 0;
            Subscriber = fc.Child("Logging").OrderBy("Id3Id").StartAt(ThisId3IdBegin).EndAt(ThisId3IdEnd).AsObservable<LogListForm>().Subscribe(
            x =>
            {
                System.Diagnostics.Debug.WriteLine($" Firebase update: {x.EventType} - {x.Object.StId}");
                if (x.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                {
                    if (ModeUsing == 2) ViewModel.Folders.Add(x.Object);
                    else if (ModeUsing == 1 && x.Object.Mistake.Equals("NONE")) ViewModel.Folders.Add(x.Object);
                    else if (ModeUsing == 0 && !x.Object.Mistake.Equals("NONE")) ViewModel.Folders.Add(x.Object);
                    Counter++;
                }
            });
            int LazyCounter = 0;
            int Retries = 0;
            bool Lock = false;
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    TimeExec.Text = $"Thời gian: {excTime.Elapsed.TotalSeconds}";
                    if (LazyCounter == Counter && Retries > 5 && !Lock)
                    {
                        Status.Text = $"Trạng thái: Xuất file Excel ({Counter} mục)";
                        Subscriber.Dispose();
                        Lock = true;
                        
                        using (ExcelEngine excelEngine = new ExcelEngine())
                        {
                            IApplication application = excelEngine.Excel;
                            application.DefaultVersion = ExcelVersion.Excel97to2003;
                            IWorkbook workbook = excelEngine.Excel.Workbooks.Create(1);
                            IWorksheet worksheet = workbook.Worksheets[0];
                            string[] header = { "", "ID", "Họ và tên", "Lớp", "Lỗi", "", "Thời gian báo cáo", "", "Người báo cáo", "Trạng thái" };
                            int[] headerSize = { 20, 5, 30, 7, 40, 12, 25, 4, 30, 9 };
                            for (int i = 0; i < header.Length; i++)
                            {
                                worksheet.Range[3, i + 1].Text = header[i];
                                worksheet.Range[3, i + 1].ColumnWidth = headerSize[i];
                            }
                            worksheet.ImportData(ViewModel.Folders.OrderBy(x => x.StId), 4, 1, false);
                            worksheet.Range["A3:J3"].CellStyle.Font.Bold = true;
                            worksheet.Range["A4"].EntireRow.FreezePanes();
                            worksheet.DeleteColumn(11);
                            worksheet.DeleteColumn(8);
                            worksheet.DeleteColumn(6);
                            worksheet.DeleteColumn(1);
                            worksheet.Range[$"E3:E{ViewModel.Folders.Count + 3}"].NumberFormat = "hh:mm:ss dd/MM/yyyy";
                            worksheet.Range["A1:A2"].Merge();
                            Assembly executingAssembly = typeof(ReportGen).GetTypeInfo().Assembly;
                            Stream inputStream = executingAssembly.GetManifestResourceStream("QRdangcap.ResourcesImg.logo.jpg");
                            IPictureShape shape = worksheet.Pictures.AddPicture(1, 1, inputStream);
                            shape.Height = 40;
                            shape.Width = 40;
                            worksheet.Range["B1"].Text = "Hệ thống Quản lý Học sinh CYB";
                            worksheet.Range["B2"].Text = $"Báo cáo Học sinh {ModeList[ModeUsing]}";
                            worksheet.Range["C1:E1"].Merge();
                            worksheet.Range["C2:E2"].Merge();
                            worksheet.Range["C1"].Text = $"Báo cáo của lớp  {ClassReport}";
                            worksheet.Range["C2"].Text = $"Thời gian báo cáo: Từ {BeginTime:d} đến {EndTime:d}";
                            MemoryStream stream = new MemoryStream();
                            workbook.SaveAs(stream);
                            workbook.Close();
                            DependencyService.Get<ISave>().SaveAndView($"Export v2 {DateTime.Now:dd-MM-yyyy HH-mm}.xls", "application/msexcel", stream);
                        }
                        
                        excTime.Stop();
                        Status.Text = $"Trạng thái: Thành công ({Counter} mục)";
                    }
                    else if (LazyCounter != Counter)
                    {
                        LazyCounter = Counter;
                        Status.Text = $"Trạng thái: Đang lấy dữ liệu ({LazyCounter} mục)";
                        Retries = 0;
                    }
                    else if (LazyCounter == Counter && !Lock && excTime.ElapsedMilliseconds >= 5000)
                    {
                        LazyCounter = Counter;
                        Status.Text = $"Trạng thái: Kiểm tra dữ liệu ({Counter} mục)";
                        Retries++;
                    }
                });
                return !Lock;
            });
        }
    }
}