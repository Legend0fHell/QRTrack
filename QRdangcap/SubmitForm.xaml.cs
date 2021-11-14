using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SubmitForm : ContentPage
    {
        private int UserIDRead = 0;
        public SQLiteConnection db = new SQLiteConnection(GlobalVariables.localLogHistDatabasePath);
        public static RetrieveAllUserDb instance = new RetrieveAllUserDb();

        public SubmitForm()
        {
            InitializeComponent();
            ChoseString.Text = "Chọn học sinh...";
            otherMistake.Text = "";
            DeviceDate.Text = DateTime.Now.ToString("dddd, dd.MM.yyyy", CultureInfo.CreateSpecificCulture("vi-VN"));
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
        protected override bool OnBackButtonPressed()
        {
            if (Navigation.NavigationStack.Count == 1)
            {
                _ = Shell.Current.GoToAsync($"//main/MainPage", true);
                return true;
            }
            else return base.OnBackButtonPressed();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LblStatusSubString2.IsVisible = false;
            if (!UserData.IsTodayOff)
            {
                Lbl_Availability.IsVisible = false;
                FormSubmitting.IsVisible = true;
                if (UserData.IsHidden) LoginToday.IsVisible = false;
                else LoginToday.IsVisible = true;
            }
            else
            {
                Lbl_Availability.IsVisible = true;
                FormSubmitting.IsVisible = false;
                LoginToday.IsVisible = false;
            }
            if (UserData.StudentPriv > 0)
            {
                if (UserData.StudentPriv == 1 && (UserData.IsUserLogin == 0 || UserData.IsUserLogin == 3))
                {
                    FormSubmitting.IsVisible = false;
                    LblStatusSubString2.IsVisible = true;
                }
                else FormSubmitting.IsVisible = true;
            }
            else
            {
                FormSubmitting.IsVisible = false;
            }
            if (UserData.IsUserLogin == 0)
            {
                LblStatusToday.Text = "Bạn chưa điểm danh ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Black;
                LblStatusSubString.Text = "Nhấn để điểm danh!";
            }
            else if (UserData.IsUserLogin == 1)
            {
                LblStatusToday.Text = "Bạn đã điểm danh ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Green;
                LblStatusSubString.Text = "Đã điểm danh đúng giờ";
            }
            else if (UserData.IsUserLogin == 2)
            {
                LblStatusToday.Text = "Bạn đã điểm danh ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Orange;
                LblStatusSubString.Text = "Đã điểm danh muộn giờ";
            }
            else if (UserData.IsUserLogin == 3)
            {
                LblStatusToday.Text = "Bạn đã báo nghỉ ngày hôm nay!";
                LblStatusToday.TextColor = Xamarin.Forms.Color.Magenta;
                LblStatusSubString.Text = "";
            }
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
            if ((!UserData.IsAtSchool && GlobalVariables.IsGPSRequired)
                || (UserData.IsAtSchool && DateTime.Now >= UserData.LastGPSUpdate.AddMinutes(5) && GlobalVariables.IsGPSRequired))
            {
                if (!DependencyService.Get<IGpsDependencyService>().IsGpsEnable())
                {
                    await DisplayAlert("Thông báo", "GPS chưa được bật. Nhấn OK để kích hoạt GPS trước khi sử dụng.", "OK");
                    DependencyService.Get<IGpsDependencyService>().OpenSettings();
                    return;
                }
                else
                {
                    await instance.UpdateCurLocation();
                }
                if (UserData.IsLastTimeMock)
                {
                    await DisplayAlert("Điểm danh thất bại!", "Phát hiện GPS đang bị làm giả!", "OK");
                    return;
                }
                else
                {
                    if (!UserData.IsAtSchool)
                    {
                        await DisplayAlert("Điểm danh thất bại!", "Bạn đang ở ngoài trường! (nếu hệ thống sai, hãy thử lại)", "OK");
                        return;
                    }
                }
            }
            string MistakeStringCombined;
            if (UserIDRead < 4)
            {
                DependencyService.Get<IToast>().ShowShort("Vui lòng điền dữ liệu hợp lệ!");
                return;
            }

            if (otherMistake.Text.Contains("NONE"))
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
                if (otherMistake.Text.Equals("NONE")) otherMistake.Text = "";
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
            instance.Firebase_SendLog(tmpStId, MistakeStringCombined, true);
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

        private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            if (UserData.IsUserLogin > 0)
            {
                DisplayAlert("Điểm danh", "Điểm danh thất bại! Bạn đã điểm danh trước đó!", "OK");
            }
            else Scanner2();
        }

        public async void Scanner2()
        {
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
            {
                PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE },
                AutoRotate = false
            };
            var ScanView = new ZXingScannerPage(options);
            string QRRandCode = "";
            ScanView.OnScanResult += (result) =>
            {
                ScanView.IsScanning = false;
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopAsync();
                    bool sepDetect = false, invalidDetect = false;
                    string decodedQRCode = "";

                    decodedQRCode = instance.Base64Decode(result.Text);
                    if (decodedQRCode.Length != 11) invalidDetect = true;
                    else
                    {
                        for (int i = 0; i < decodedQRCode.Length; ++i)
                        {
                            if (decodedQRCode[i] == '_')
                            {
                                if (sepDetect)
                                {
                                    invalidDetect = true;
                                    break;
                                }
                                else sepDetect = true;
                                continue;
                            }
                            else if (decodedQRCode[i] < 48 || (decodedQRCode[i] > 57 && decodedQRCode[i] < 65) || (decodedQRCode[i] > 90))
                            {
                                invalidDetect = true;
                                break;
                            }
                            if (sepDetect) QRRandCode += decodedQRCode[i];
                        }
                    }
                    if (!sepDetect || invalidDetect)
                    {
                        DisplayAlert("Điểm danh thất bại!", "Code QR không hợp lệ!", "OK");
                    }
                    else if ((!UserData.IsAtSchool && GlobalVariables.IsGPSRequired)
                        || (UserData.IsAtSchool && DateTime.Now >= UserData.LastGPSUpdate.AddMinutes(5) && GlobalVariables.IsGPSRequired))
                    {
                        LocationTmpUpdating();
                        async void LocationTmpUpdating()
                        {
                            if (!DependencyService.Get<IGpsDependencyService>().IsGpsEnable())
                            {
                                await DisplayAlert("Thông báo", "GPS chưa được bật. Nhấn OK để kích hoạt GPS trước khi sử dụng.", "OK");
                                DependencyService.Get<IGpsDependencyService>().OpenSettings();
                                return;
                            }
                            else
                            {
                                await instance.UpdateCurLocation();
                            }
                            if (UserData.IsLastTimeMock)
                            {
                                await DisplayAlert("Điểm danh thất bại!", "Phát hiện GPS đang bị làm giả!", "OK");
                            }
                            else
                            {
                                if (!UserData.IsAtSchool) await DisplayAlert("Điểm danh thất bại!", "Bạn đang ở ngoài trường! (nếu hệ thống sai, hãy thử lại)", "OK");
                                else SendData();
                            }
                        }
                    }
                    else
                    {
                        SendData();
                    }
                });
            };
            async void SendData()
            {
                ResponseModel response = (ResponseModel)await instance.HttpPolly(new FeedbackModel()
                {
                    Mode = "2",
                    Contents = QRRandCode,
                    Contents2 = UserData.StudentIdDatabase.ToString(),
                });
                string Reason = "";
                if (response.Message1 == 1)
                {
                    Reason = "Bạn đã điểm danh đúng giờ!";
                    UserData.IsUserLogin = 1;
                }
                else if (response.Message1 == 2)
                {
                    Reason = "Bạn đã điểm danh muộn!";
                    UserData.IsUserLogin = 2;
                }
                else if (response.Message1 == 0) Reason = "Code QR đã cũ hoặc không hợp lệ!";
                else if (response.Message1 == -1) Reason = "Bạn điểm danh ngoài khoảng thời gian quy định!";
                else Reason = "Bạn đã điểm danh trước đó!";
                if (response.Status == "SUCCESS")
                {
                    instance.Firebase_SendLog(UserData.StudentIdDatabase, "NONE", false, false);
                    await DisplayAlert("Điểm danh thành công!", Reason, "OK");
                    OnAppearing();
                }
                else
                {
                    await DisplayAlert("Điểm danh thất bại!", Reason, "OK");
                }
            }
            OnAppearing();
            await Navigation.PushAsync(ScanView);
        }
    }
}