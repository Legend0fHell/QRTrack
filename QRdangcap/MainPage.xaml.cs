﻿using Newtonsoft.Json;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace QRdangcap
{
    public partial class MainPage : ContentPage
    {
        public static HttpClient client = new HttpClient();
        public RetrieveAllUserDb instance = new RetrieveAllUserDb();

        public MainPage()
        {
            InitializeComponent();
            Wishes.Text = "Chúc bạn có một ngày vui vẻ!";
            DeviceDate.Text = DateTime.Now.ToString("ddd, dd.MM.yyyy", CultureInfo.CreateSpecificCulture("vi-VN"));
            VerText.Text = GlobalVariables.ClientVersion + " (" + GlobalVariables.ClientVersionDate.ToString("dd.MM") + ")";
            RefreshingView.IsRefreshing = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DeviceClock.Text = DateTime.Now.ToString("HH:mm:ss");
                    InitStaticText();
                    // Replace with overwriting OnAppearing.
                    if (UserData.StudentPreIdDatabase != UserData.StudentIdDatabase)
                    {
                        UserData.StudentPreIdDatabase = UserData.StudentIdDatabase;
                    }
                });
                return true;
            });
        }
        public void InitStaticText()
        {
            Greeting.Text = "Xin chào, " + UserData.StudentIdDatabase.ToString() + ". " + UserData.StudentFullName + "!";
            Priv.Text = UserData.StudentPriv.ToString();
            LblClass.Text = UserData.StudentClass;
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
            RefreshingView.IsRefreshing = false;
        }

        private async void Logout_Clicked(object sender, System.EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            UserData.StudentPreIdDatabase = UserData.StudentIdDatabase;
        }

        public void Button_Clicked(object sender, System.EventArgs e)
        {
            if (UserData.IsUserLogin > 0)
            {
                DisplayAlert("Điểm danh", "Điểm danh thất bại! Bạn đã điểm danh trước đó!", "OK");
            }
            else Scanner();
        }

        public async void Scanner()
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
                    if (!sepDetect || invalidDetect)
                    {
                        DisplayAlert("Điểm danh thất bại!", "Code QR không hợp lệ!", "OK");
                    }
                    else if (!UserData.IsAtSchool && GlobalVariables.IsGPSRequired)
                    {
                        LocationTmpUpdating();
                        async void LocationTmpUpdating()
                        {
                            await instance.UpdateCurLocation();
                            if(UserData.IsLastTimeMock)
                            {
                                await DisplayAlert("Điểm danh thất bại!", "Phát hiện GPS đang bị làm giả!", "OK");
                            }
                            await DisplayAlert("Điểm danh thất bại!", "Bạn đang ở ngoài trường! (nếu hệ thống sai, hãy thử lại)", "OK");
                        }
                    }
                    else
                    {
                        var model = new FeedbackModel()
                        {
                            Mode = "2",
                            Contents = QRRandCode,
                            Contents2 = UserData.StudentIdDatabase.ToString(),
                        };
                        var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
                        var jsonString = JsonConvert.SerializeObject(model);
                        var requestContent = new StringContent(jsonString);
                        SendData();
                        async void SendData()
                        {
                            var resultQR = await client.PostAsync(uri, requestContent);
                            var resultContent = await resultQR.Content.ReadAsStringAsync();
                            var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
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
                            }
                            else
                            {
                                await DisplayAlert("Điểm danh thất bại!", Reason, "OK");
                            }
                        }
                    }
                });
            };

            await Navigation.PushAsync(ScanView);
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            InitStaticText();
        }

        private void UpdateUser_Tapped(object sender, EventArgs e)
        {
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            instance.RetrieveAllUserDatabase();
        }
    }
}