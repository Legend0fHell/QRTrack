using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using QRdangcap.GoogleDatabase;
using System;
using System.Globalization;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;
using QRdangcap.LocalDatabase;
using SQLite;
using Firebase.Database;
using Firebase.Database.Query;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class SubmitForm : ContentPage
    {
        int UserIDRead = 0;
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localLogHistDatabasePath);
        public SubmitForm()
        {
            InitializeComponent();
            ChoseString.Text = "Chọn học sinh...";
            otherMistake.Text = "";
            DeviceDate.Text = DateTime.Now.ToString("dddd, dd.MM.yyyy", CultureInfo.CreateSpecificCulture("vi-VN"));
            db.CreateTable<LogListForm>();
            MessagingCenter.Subscribe<Page, UserListForm>(this, "ChoseSTId", (p, resUser) =>
                {
                    ChoseString.Text = resUser.StId.ToString() + " " + resUser.StClass + " - " + resUser.StName;
                    UserIDRead = resUser.StId;
                });
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DeviceClock.Text = DateTime.Now.ToString("HH:mm:ss");
                });
                return true;
            });
            BindingContext = this;
        }
        
        public async void Scanner()
        {
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
            {
                PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE },
                AutoRotate = false
            };
            var ScanView = new ZXingScannerPage(options);

            ScanView.OnScanResult += (result) =>
            {
                ScanView.IsScanning = false;
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopAsync();
                    bool invalidDetect = false;
                    UserIDRead = 0;
                    string decodedQRCode = "";
                    RetrieveAllUserDb instance = new RetrieveAllUserDb();
                    decodedQRCode = instance.Base64Decode(result.Text);
                    for (int i = 0; i < decodedQRCode.Length; ++i)
                    {
                        if (decodedQRCode[i] < 48 || decodedQRCode[i] > 57)
                        {
                            invalidDetect = true;
                            break;
                        }
                        else
                        {
                            UserIDRead *= 10;
                            UserIDRead += decodedQRCode[i] - '0';
                        }
                    }
                    if (invalidDetect)
                    {
                        DisplayAlert("Đọc", "Đọc thất bại! Code QR không hợp lệ!", "OK");
                    }
                    else
                    {
                        ChoseString.Text = instance.RetrieveNameUser(UserIDRead);
                    }
                });
            };

            await Navigation.PushAsync(ScanView);
        }
        public async void SendToDatabase()
        {
            if (!UserData.IsAtSchool && GlobalVariables.IsGPSRequired)
            {
                RetrieveAllUserDb instance = new RetrieveAllUserDb();
                instance.UpdateCurLocation();
                DependencyService.Get<IToast>().ShowShort("Bạn đang ở ngoài trường! (nếu hệ thống sai, hãy thử lại)");
                return;
            }
            string MistakeStringCombined;
            if (UserIDRead < 4)
            {
                DependencyService.Get<IToast>().ShowShort("Vui lòng điền dữ liệu hợp lệ!");
                return;
            }

            if(otherMistake.Text.Contains("NONE") || otherMistake.Text.Contains(";"))
            {
                DependencyService.Get<IToast>().ShowShort("Lỗi không hợp lệ. Vui lòng nhập lại.");
                return;
            }
            else
            {
                List<string> MistakeString = new List<string>();
                if (Reason0.IsChecked) MistakeString.Add("Quên thẻ");
                if (Reason1.IsChecked) MistakeString.Add("Sai đồng phục");
                if (Reason2.IsChecked) MistakeString.Add("Lỗi ATGT");
                if (!(otherMistake.Text.Equals(null) || otherMistake.Text.Equals(""))) MistakeString.Add(otherMistake.Text);
                MistakeStringCombined = "";
                if (MistakeString.Count == 0) MistakeStringCombined = "NONE";
                else
                {
                    foreach (string mistake in MistakeString)
                    {
                        MistakeStringCombined += mistake + ";";
                    }
                    MistakeStringCombined = MistakeStringCombined.Substring(0, MistakeStringCombined.Length - 1);
                }

            }
            int tmpStId = UserIDRead;
            string QueryName = ChoseString.Text;
            DependencyService.Get<IToast>().ShowShort("Đang gửi: " + QueryName);
            var client = new HttpClient();
            var model = new FeedbackModel()
            {
                Mode = "6",
                Contents = UserIDRead.ToString(),
                Contents2 = UserData.StudentIdDatabase.ToString(),
                Contents3 = MistakeStringCombined
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var resultQR = await client.PostAsync(uri, requestContent);
            var resultContent = await resultQR.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);

            string Reason;
            if (response.Message1 == 1)
            {
                Reason = "(Đúng giờ)";
            }
            else if (response.Message1 == 2)
            {
                Reason = "(Muộn)";
            }
            else if (response.Message1 == -1) Reason = "(Ngoài giờ)";
            else Reason = "(Trùng)";
            if (response.Status == "SUCCESS")
            {
                FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
                var lmao = new OutboundLog
                {
                    StId = tmpStId,
                    ReporterId = UserData.StudentIdDatabase,
                    Mistake = MistakeStringCombined,
                    LoginStatus = 1,
                };
                var keyy = await fc.Child("Logging").PostAsync(lmao);
                InboundLog curLog = await fc.Child("Logging").Child(keyy.Key).OnceSingleAsync<InboundLog>();
                DateTime CurDateTime = new DateTime(curLog.Timestamp, DateTimeKind.Local);
                TimeSpan CurTime = new TimeSpan(CurDateTime.Hour, CurDateTime.Minute, CurDateTime.Second);
                if (CurTime >= UserData.StartTime && CurTime <= UserData.EndTime)
                {
                    if (CurTime > UserData.LateTime)
                    {
                        curLog.LoginStatus = 2;
                        await fc.Child("Logging").Child(keyy.Key).PutAsync(curLog);
                    }
                }
                DependencyService.Get<IToast>().ShowShort("OK " + Reason + ": " + QueryName);
                /*
                LogListForm SentLog = new LogListForm()
                {
                    LogId = response.Message2,
                    LoginStatus = response.Message1,
                    ReporterId = UserData.StudentIdDatabase,
                    Mistake = MistakeStringCombined,
                    StId = tmpStId,
                    LoginDate = response.DateTimeMessage
                };
                db.Insert(SentLog);
                */
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Lỗi " + Reason + ": " + QueryName);
            }
        }
        private void ClearContents()
        {
            ChoseString.Text = "Chọn học sinh...";
            UserIDRead = 0;
            Reason0.IsChecked = false;
            Reason1.IsChecked = false;
            Reason2.IsChecked = false;
            otherMistake.Text = "";
        }
        public void Button_Clicked(object sender, System.EventArgs e)
        {
            Scanner();
            
        }   

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            SendToDatabase();
            ClearContents();
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            ClearContents();
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var ChoosePage = new QueryInfo();
            await Navigation.PushAsync(ChoosePage);
        }
        private async void History_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LocalLogHistory());
        }
    }
}