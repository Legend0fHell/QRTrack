using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using QRdangcap.LocalDatabase;
using SQLite;
using Syncfusion.XForms.PopupLayout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace QRdangcap.GoogleDatabase
{
    public class RetrieveAllUserDb
    {
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        private readonly Stopwatch excTime = new Stopwatch();
        public SQLiteConnection db2 = new SQLiteConnection(GlobalVariables.localLogHistDatabasePath);
        public static HttpClient client = new HttpClient();

        public async void RetrieveAllUserDatabase()
        {
            DependencyService.Get<IToast>().ShowShort("Đang tải dữ liệu...");
            excTime.Reset();
            excTime.Start();
            var model = new FeedbackModel()
            {
                Mode = "7",
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var result = await client.PostAsync(uri, requestContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<UserListForm[]>(resultContent);
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            db.CreateTable<UserListForm>();
            db.DeleteAll<UserListForm>();
            db.InsertAll(response);
            excTime.Stop();
            db.Dispose();
            DependencyService.Get<IToast>().ShowShort("Tải thành công! (" + excTime.ElapsedMilliseconds + "ms)");
        }

        public async void RetrieveAllLogDatabase()
        {
            //DependencyService.Get<IToast>().ShowShort("Đang tải dữ liệu...");
            excTime.Reset();
            excTime.Start();
            var model = new FeedbackModel()
            {
                Mode = "3",
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var result = await client.PostAsync(uri, requestContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<LogListForm[]>(resultContent);
            var db = new SQLiteConnection(GlobalVariables.localDatabasePath);
            db.CreateTable<LogListForm>();
            db.DeleteAll<LogListForm>();
            db.InsertAll(response);
            excTime.Stop();
            db.Dispose();
            //DependencyService.Get<IToast>().ShowShort("Tải thành công! (" + excTime.ElapsedMilliseconds + "ms)");
        }

        public string RetrieveNameUser(int Id)
        {
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            db.CreateTable<UserListForm>();
            return Id.ToString() + " " + db.Table<UserListForm>().ElementAt(Id - 4).StClass + " - " + db.Table<UserListForm>().ElementAt(Id - 4).StName;
        }

        public void CheckUserTableExist()
        {
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            db.CreateTable<UserListForm>();
            if (db.Table<UserListForm>().ToList().Count == 0)
            {
                RetrieveAllUserDatabase();
            }
            db.Dispose();
        }

        public async void CheckUpdates()
        {
            var model = new FeedbackModel()
            {
                Mode = "11",
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            var result = await client.PostAsync(uri, requestContent);
            var resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
            if (response.Status != GlobalVariables.ClientVersion)
            {
                await App.Current.MainPage.DisplayAlert("Đã có phiên bản mới, " + response.Status + "!",
                    "Cập nhật vào " + response.DateTimeMessage.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd.MM.yyyy HH:mm") +
                    ".\nThay đổi: " + response.Message, "OK");
                return;
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Không có cập nhật mới.");
            }
        }

        public string Base64Encode(string plainText)
        {
            if (plainText == null) return null;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        public string Base64Decode(string base64EncodedData)
        {
            if (base64EncodedData == null) return null;
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
        }

        public async void UpdateCurLocation()
        {
            Location School = new Location()
            {
                Latitude = UserData.SchoolLat,
                Longitude = UserData.SchoolLon
            };
            var resultt = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(30)));
            double dist = resultt.CalculateDistance(School, DistanceUnits.Kilometers);
            if (dist * 1000 >= UserData.SchoolDist) UserData.IsAtSchool = false;
            else UserData.IsAtSchool = true;
        }

        public async void Firebase_SendLog(int STId, string Mistake = "NONE", bool IsOverwriteOk = false, bool IsSentToGG = true)
        {
            string ThisIdenId = DateTime.Now.DayOfYear.ToString() + "_" + DateTime.Now.ToString("yyyy") + "_" + STId.ToString();
            string QueryName = RetrieveNameUser(STId);

            if (UserData.IsTodayOff)
            {
                DependencyService.Get<IToast>().ShowShort("Lỗi (Ngoài giờ): " + QueryName);
                return;
            }
            Debug.WriteLine($"Khởi tạo: {QueryName}");
            var tmp = await fc.Child("Logging").OrderBy("IdentityId").EqualTo(ThisIdenId).OnceAsync<InboundLog>();
            if (tmp.Count > 0)
            {
                List<FirebaseObject<InboundLog>> tmp2 = tmp.ToList();
                DateTime oldLogTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(tmp2[0].Object.Timestamp).ToLocalTime();
                if (IsOverwriteOk)
                {
                    Debug.WriteLine($"Phát hiện trùng: {QueryName}");
                    SfPopupLayout OverwritePopup = new SfPopupLayout();
                    OverwritePopup.PopupView.HeaderTitle = "Học sinh này đã được ghi trước đó!";
                    OverwritePopup.PopupView.AppearanceMode = AppearanceMode.TwoButton;
                    OverwritePopup.PopupView.AcceptButtonText = "Ghi đè";
                    OverwritePopup.PopupView.DeclineButtonText = "Hủy";
                    OverwritePopup.PopupView.AnimationMode = AnimationMode.Fade;
                    OverwritePopup.PopupView.PopupStyle.OverlayColor = Color.Black;
                    OverwritePopup.PopupView.PopupStyle.OverlayOpacity = 0.35;
                    OverwritePopup.BackgroundColor = new Color(230, 230, 230);
                    OverwritePopup.PopupView.HeightRequest = 350;
                    DataTemplate contentTemplateView = new DataTemplate(() =>
                    {
                        StackLayout popupContent = new StackLayout()
                        {
                            Spacing = 1,
                            Children =
                            {
                                new Label {Text = "Bạn có thể ghi đè biên bản cũ (không đổi thời gian), hoặc hủy biên bản này.", FontSize=17, HorizontalOptions=LayoutOptions.CenterAndExpand},
                                new Frame
                                {
                                    CornerRadius = 10,
                                    HasShadow = true,
                                    HeightRequest = 40,
                                    Content = new StackLayout
                                    {
                                        Orientation = StackOrientation.Horizontal,
                                        Children = {
                                            new Label {Text = "Cũ", FontSize=20, FontAttributes=FontAttributes.Bold},
                                            new StackLayout
                                            {
                                                HorizontalOptions=LayoutOptions.EndAndExpand,
                                                Children =
                                                {
                                                    new Label {Text = QueryName, FontSize=15 },
                                                    new StackLayout
                                                    {
                                                        Orientation = StackOrientation.Horizontal,

                                                        Children =
                                                        {
                                                            new Label {Text = tmp2[0].Object.Mistake, TextColor = Color.Red},
                                                            new Label {
                                                                Text = oldLogTime.ToString("HH:mm:ss"), FontSize = 20, FontAttributes=FontAttributes.Bold,
                                                                TextColor=tmp2[0].Object.LoginStatus == 1 ? Color.Green : Color.Orange, HorizontalOptions=LayoutOptions.EndAndExpand},
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new Frame
                                {
                                    CornerRadius = 10,
                                    HasShadow = true,
                                    HeightRequest = 40,
                                    Content = new StackLayout
                                    {
                                        Orientation = StackOrientation.Horizontal,
                                        Children = {
                                            new Label {Text = "Mới", FontSize=20, FontAttributes=FontAttributes.Bold},
                                            new StackLayout
                                            {
                                                HorizontalOptions=LayoutOptions.EndAndExpand,
                                                Children =
                                                {
                                                    new Label {Text = QueryName, FontSize=15 },
                                                    new StackLayout
                                                    {
                                                        Orientation = StackOrientation.Horizontal,

                                                        Children =
                                                        {
                                                            new Label {Text = Mistake, TextColor = Color.Red},
                                                            new Label {
                                                                Text = oldLogTime.ToString("HH:mm:ss"), FontSize = 20, FontAttributes=FontAttributes.Bold,
                                                                TextColor=tmp2[0].Object.LoginStatus == 1 ? Color.Green : Color.Orange, HorizontalOptions=LayoutOptions.EndAndExpand},
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                            }
                        };
                        return popupContent;
                    });
                    OverwritePopup.PopupView.ContentTemplate = contentTemplateView;
                    OverwritePopup.ClosePopupOnBackButtonPressed = true;
                    OverwritePopup.PopupView.AcceptCommand = new Command(PopupAcp);
                    OverwritePopup.PopupView.DeclineCommand = new Command(PopupDecl);
                    OverwritePopup.Show();
                    void PopupAcp()
                    {
                        Debug.WriteLine($"Ghi đè: {QueryName}");
                        Firebase_OverwriteLog(tmp2[0], oldLogTime, Mistake);
                        return;
                    }
                    void PopupDecl()
                    {
                        DependencyService.Get<IToast>().ShowShort("Lỗi (Trùng): " + QueryName);
                        return;
                    }
                }
                else
                {
                    DependencyService.Get<IToast>().ShowShort("Lỗi (Trùng): " + QueryName);
                    return;
                }
            }
            else
            {
                Debug.WriteLine($"Không phát hiện trùng, đang ghi: {QueryName}");
                Firebase_SendLog2(STId, Mistake, IsSentToGG);
            }
        }

        public async void Firebase_OverwriteLog(FirebaseObject<InboundLog> curLog, DateTime logTime, string Mistake = "NONE")
        {
            curLog.Object.Mistake = Mistake;
            curLog.Object.ReporterId = UserData.StudentIdDatabase;
            await fc.Child("Logging").Child(curLog.Key).PutAsync(curLog.Object);
            var model = new FeedbackModel()
            {
                Mode = "6",
                Contents = curLog.Object.StId.ToString(),
                Contents2 = logTime.DayOfYear.ToString(),
                Contents3 = curLog.Object.LoginStatus.ToString(),
                Contents4 = Mistake.Equals("NONE") ? "0" : "1",
            };
            var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
            var jsonString = JsonConvert.SerializeObject(model);
            var requestContent = new StringContent(jsonString);
            _ = await client.PostAsync(uri, requestContent);
            DependencyService.Get<IToast>().ShowShort("Ghi đè thành công");
            Debug.WriteLine($"Ghi đè OK");
        }

        public async void Firebase_SendLog2(int STId, string Mistake = "NONE", bool IsSentToGG = true)
        {
            string ThisIdenId = DateTime.Now.DayOfYear.ToString() + "_" + DateTime.Now.ToString("yyyy") + "_" + STId.ToString();
            string QueryName = RetrieveNameUser(STId);
            OutboundLog SubLog = new OutboundLog()
            {
                StId = STId,
                ReporterId = UserData.StudentIdDatabase,
                Mistake = Mistake,
                IdentityId = ThisIdenId,
                LoginStatus = 1,
            };
            Debug.WriteLine($"Lấy thời gian: {QueryName}");
            var LogKeys = await fc.Child("Logging").PostAsync(SubLog);
            InboundLog curLog = await fc.Child("Logging").Child(LogKeys.Key).OnceSingleAsync<InboundLog>();
            curLog.Keys = LogKeys.Key;
            DateTime CurDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(curLog.Timestamp).ToLocalTime();
            TimeSpan CurTime = new TimeSpan(CurDateTime.Hour, CurDateTime.Minute, CurDateTime.Second);
            if (CurTime >= UserData.StartTime && CurTime <= UserData.EndTime)
            {
                if (CurTime > UserData.LateTime) curLog.LoginStatus = 2;
                await fc.Child("Logging").Child(LogKeys.Key).PutAsync(curLog);
                if (!IsSentToGG) return;
                Debug.WriteLine($"Gửi lên GG Sheet: {QueryName}");
                var model = new FeedbackModel()
                {
                    Mode = "6",
                    Contents = STId.ToString(),
                    Contents2 = CurDateTime.DayOfYear.ToString(),
                    Contents3 = curLog.LoginStatus.ToString(),
                    Contents4 = Mistake.Equals("NONE") ? "0" : "1",
                };
                var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
                var jsonString = JsonConvert.SerializeObject(model);
                var requestContent = new StringContent(jsonString);
                var result = await client.PostAsync(uri, requestContent);
                var resultContent = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ResponseModel>(resultContent);
                if (response.Message1 == 3)
                {
                    await fc.Child("Logging").Child(curLog.Keys).DeleteAsync();
                    DependencyService.Get<IToast>().ShowShort("Lỗi (HS Báo nghỉ): " + QueryName);
                }
                else
                {
                    DependencyService.Get<IToast>().ShowShort("OK: " + QueryName);
                    db2.CreateTable<LogListForm>();
                    Debug.WriteLine($"Thêm vào db local: {QueryName}");
                    LogListForm SentLog = new LogListForm()
                    {
                        Keys = curLog.Keys,
                        LoginStatus = curLog.LoginStatus,
                        ReporterId = UserData.StudentIdDatabase,
                        Mistake = curLog.Mistake,
                        StId = STId,
                        Timestamp = curLog.Timestamp
                    };
                    db2.Insert(SentLog);
                }
            }
            else
            {
                await fc.Child("Logging").Child(LogKeys.Key).DeleteAsync();
                DependencyService.Get<IToast>().ShowShort("Lỗi (Ngoài giờ): " + QueryName);
            }
        }
    }
}