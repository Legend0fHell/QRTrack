using Firebase.Database.Query;
using Plugin.CloudFirestore;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using QRdangcap.ViewModel;
using SQLite;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportGen : ContentPage
    {
        private readonly ReportGenViewModel ViewModel;

        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();
        public List<UserListForm> ItemsList = new List<UserListForm>();
        public List<string> ClassList = new List<string>();
        public string ClassReport { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public ReportGen()
        {
            InitializeComponent();
            ViewModel = new ReportGenViewModel();
            BindingContext = ViewModel;
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            ClassList = db.Table<UserListForm>().Select(x => x.StClass).AsParallel().Distinct().ToList();
            db.Dispose();
            ClassList.Insert(0, "Toàn bộ");
            ClassList.Remove("Debug");
            ClassChose.ItemsSource = ClassList;
            ClassChose.SelectedIndex = 0;
            FromDate.Date = DateTime.Now;
            ToDate.Date = DateTime.Now;
        }

        private void Class_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClassReport = ClassList[ClassChose.SelectedIndex];
        }

        public LogListForm InboundLogConv(InboundLog inbound)
        {
            return new LogListForm()
            {
                Keys = inbound.Keys,
                StId = inbound.StId,
                Mistake = inbound.Mistake,
                Timestamp = (inbound.Timestamp.ToDateTime().Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerMillisecond,
                ReporterId = inbound.ReporterId,
                LoginStatus = inbound.LoginStatus,
            };
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            Stopwatch excTime = new Stopwatch();
            excTime.Reset();
            excTime.Start();
            bool Lock = false;
            BeginTime = FromDate.Date.Date;
            EndTime = ToDate.Date.Date.AddSeconds(86399);
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    TimeExec.Text = $"Thời gian: {(int)excTime.Elapsed.TotalSeconds}s";
                });
                return !Lock;
            });
            Status.Text = $"Trạng thái: Lấy dữ liệu";
            IQuerySnapshot tmp;
            if (ClassChose.SelectedIndex == 0)
            {
                tmp = await CrossCloudFirestore.Current.Instance.Collection("logging")
                .OrderBy("Timestamp").WhereGreaterThanOrEqualsTo("Timestamp", new Timestamp(BeginTime)).WhereLessThanOrEqualsTo("Timestamp", new Timestamp(EndTime))
                .GetAsync();
            }
            else
            {
                tmp = await CrossCloudFirestore.Current.Instance.Collection("logging").WhereEqualsTo("StClass", ClassReport)
                .OrderBy("Timestamp").WhereGreaterThanOrEqualsTo("Timestamp", new Timestamp(BeginTime)).WhereLessThanOrEqualsTo("Timestamp", new Timestamp(EndTime))
                .GetAsync();
            }
            var tmp2 = tmp.ToObjects<InboundLog>();
            foreach (InboundLog inbound in tmp2)
            {
                if (inbound.NoMistake > 0) ViewModel.Folders.Add(InboundLogConv(inbound));
            }

            Status.Text = $"Trạng thái: Xuất file Excel";
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel97to2003;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Create(2);
                IWorksheet worksheet = workbook.Worksheets[0];
                worksheet.Name = "Tổng quát";
                worksheet.Range["A4"].EntireRow.FreezePanes();

                int curRow = 4;
                string[] header2 = { "Lớp", "Ngày", "Họ và tên", "Lỗi", "Số lỗi" };
                int[] header2Size = { 6, 26, 25, 40, 8 };
                for (int i = 0; i < header2.Length; i++)
                {
                    worksheet.Range[3, i + 1].Text = header2[i];
                    worksheet.Range[3, i + 1].ColumnWidth = header2Size[i];
                }
                worksheet.Range["A3:E3"].CellStyle.Font.Bold = true;
                worksheet.Range[4, 8].Text = "Lớp";
                worksheet.Range[4, 9].Text = "Tổng lỗi";
                worksheet.Range[4, 10].Text = "Xếp hạng";
                worksheet.Range["H4:J4"].CellStyle.Font.Bold = true;
                int ClassRow = 5;
                for (int i = 1; i < ClassList.Count; i++)
                {
                    if (ClassChose.SelectedIndex != 0) i = ClassChose.SelectedIndex;
                    worksheet.Range[curRow, 1, curRow, 5].Merge();
                    worksheet.Range[curRow, 1].Text = $"Lớp {ClassList[i]}";
                    worksheet.Range[curRow, 1].CellStyle.Font.Bold = true;
                    curRow++;
                    int lstSame = curRow;
                    int prevCurRow = curRow;
                    var tmpFolder = ViewModel.Folders.Where(x => x.StClass == ClassList[i]);
                    foreach (LogListForm listForm in tmpFolder)
                    {
                        string DateInVN = listForm.LoginDate.ToString("dddd, dd.MM.yyyy", CultureInfo.CreateSpecificCulture("vi-VN"));

                        if (worksheet.Range[lstSame, 2].Text != DateInVN)
                        {
                            worksheet.Range[curRow, 2].Text = DateInVN;
                            lstSame = curRow;
                        }
                        worksheet.Range[curRow, 1].Text = ClassList[i];
                        worksheet.Range[curRow, 3].Text = listForm.StName;
                        worksheet.Range[curRow, 4].Text = (listForm.LoginStatus == 2 ? "Đi học muộn;" : "") + (listForm.Mistake.Equals("NONE") ? "" : listForm.Mistake);
                        worksheet.Range[curRow, 5].Number = listForm.LoginStatus - 1 + (listForm.Mistake.Equals("NONE") ? 0 : listForm.Mistake.Count(x => x.Equals(';')) + 1);
                        curRow++;
                    }
                    worksheet.Range[ClassRow, 8].Text = $"{ClassList[i]}";
                    worksheet.Range[curRow, 4].Text = "Tổng số lỗi: ";
                    worksheet.Range[curRow, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet.Range[curRow, 4, curRow, 5].CellStyle.Font.Bold = true;
                    worksheet.Range[curRow, 5].Formula = tmpFolder.Count() == 0 ? "=0" : $"=SUM(E{prevCurRow}:E{curRow - 1})";
                    worksheet.Range[ClassRow, 9].Formula = $"=E{curRow}";
                    worksheet.Range[ClassRow, 10].Formula = $"=RANK(I{ClassRow}, I5:I{3 + ClassList.Count}, 1)";
                    curRow++;
                    worksheet.Range[curRow, 4].Text = "Xếp hạng: ";
                    worksheet.Range[curRow, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet.Range[curRow, 4, curRow, 5].CellStyle.Font.Bold = true;
                    worksheet.Range[curRow, 5].Formula = $"=J{ClassRow}";
                    worksheet.Range[prevCurRow - 1, 1, curRow, 5].BorderAround(ExcelLineStyle.Double);
                    curRow++;
                    ClassRow++;
                    if (ClassChose.SelectedIndex != 0) break;
                }
                worksheet.Range[4, 8, 3 + ClassList.Count, 10].BorderAround(ExcelLineStyle.Double);
                Assembly executingAssembly = typeof(ReportGen).GetTypeInfo().Assembly;
                Stream inputStream = executingAssembly.GetManifestResourceStream("QRdangcap.ResourcesImg.logo2.png");
                IPictureShape shape = worksheet.Pictures.AddPicture(1, 1, inputStream);
                shape.Height = 40;
                shape.Width = 40;
                worksheet.Range["A1:A2"].Merge();
                worksheet.Range["B1"].Text = "Hệ thống Quản lý Học sinh CYB";
                worksheet.Range["B2"].Text = $"Tổng quát Học sinh Phạm lỗi";
                worksheet.Range["C1:E1"].Merge();
                worksheet.Range["C2:E2"].Merge();
                worksheet.Range["C1"].Text = $"Báo cáo của {ClassReport}";
                worksheet.Range["C2"].Text = $"Thời gian báo cáo: Từ {BeginTime:d} đến {EndTime:d}";
                worksheet.Range[1, 1, curRow, 10].CellStyle.Font.FontName = "Calibri";
                worksheet = workbook.Worksheets[1];
                worksheet.Name = "Chi tiết";
                string[] header = { "", "ID", "Họ và tên", "Lớp", "Lỗi", "", "Thời gian báo cáo", "", "Người báo cáo", "Trạng thái" };
                int[] headerSize = { 20, 5, 26, 7, 40, 12, 25, 4, 30, 9 };
                for (int i = 0; i < header.Length; i++)
                {
                    worksheet.Range[3, i + 1].Text = header[i];
                    worksheet.Range[3, i + 1].ColumnWidth = headerSize[i];
                }
                worksheet.ImportData(ViewModel.Folders.OrderBy(x => x.StId), 4, 1, false);
                worksheet.Range["A3:J3"].CellStyle.Font.Bold = true;
                worksheet.DeleteColumn(11);
                worksheet.DeleteColumn(8);
                worksheet.DeleteColumn(6);
                worksheet.DeleteColumn(1);
                worksheet.Range[$"E3:E{ViewModel.Folders.Count + 3}"].NumberFormat = "hh:mm:ss dd/MM/yyyy";
                worksheet.Range["A4"].EntireRow.FreezePanes();
                worksheet.Range["A1:A2"].Merge();
                IPictureShape shape2 = worksheet.Pictures.AddPicture(1, 1, inputStream);
                shape2.Height = 40;
                shape2.Width = 40;
                worksheet.Range["B1"].Text = "Hệ thống Quản lý Học sinh CYB";
                worksheet.Range["B2"].Text = $"Chi tiết Học sinh Phạm lỗi";
                worksheet.Range["C1:E1"].Merge();
                worksheet.Range["C2:E2"].Merge();
                worksheet.Range["C1"].Text = $"Báo cáo của {ClassReport}";
                worksheet.Range["C2"].Text = $"Thời gian báo cáo: Từ {BeginTime:d} đến {EndTime:d}";
                worksheet.Range[1, 1, ViewModel.Folders.Count + 3, 10].CellStyle.Font.FontName = "Calibri";
                MemoryStream stream = new MemoryStream();
                workbook.SaveAs(stream);
                workbook.Close();
                DependencyService.Get<ISave>().SaveAndView($"BaoCao.xls", "application/msexcel", stream);
            }
            excTime.Stop();
            Status.Text = $"Trạng thái: Thành công";
            Lock = true;
        }
    }
}