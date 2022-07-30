using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using Plugin.CloudFirestore;
using Polly;
using QRdangcap.DatabaseModel;
using SQLite;
using Syncfusion.XForms.PopupLayout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace QRdangcap.GoogleDatabase
{
    public class RetrieveAllUserDb
    {
        public static FirebaseClient fc = new FirebaseClient(GlobalVariables.FirebaseURL);
        public static HttpClient client = new HttpClient();

        public async Task<object> HttpPolly(FeedbackModel model, bool OutputNeeded = true, string OutputType = "ResponseModel", int MaxRetry = 5, string CustomNullResponse = "")
        {
            model.ProtectKey = "Your_Protect_Key";
            int retry = 1;
            object response = new object();
            while (retry < MaxRetry)
            {
                try
                {
                    string responseString = await Policy
                    .Handle<HttpRequestException>(ex => !ex.Message.Contains("404"))
                    .WaitAndRetryAsync
                    (
                        retryCount: MaxRetry,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                        onRetry: (ex, time) =>
                        {
                            DependencyService.Get<IToast>().ShowShort($"Đã có lỗi xảy ra: {ex.Message}. Đang thử lại...");
                            Debug.WriteLine($"Đã có lỗi xảy ra: {ex.Message}, đang thử lại {time}.");
                        }
                    )
                    .ExecuteAsync(async () =>
                    {
                        var uri = "https://script.google.com/macros/s/AKfycbz-788uVtNyd9408r92pHXnI6H4QfMVWrey6biV2zhdz60hoQauo1a4Y3YwuJuQ1UhKAg/exec";
                        var jsonString = JsonConvert.SerializeObject(model);
                        var requestContent = new StringContent(jsonString);
                        var result = await client.PostAsync(uri, requestContent);
                        var resultContent = await result.Content.ReadAsStringAsync();
                        return resultContent;
                    });
                    if (OutputNeeded)
                    {
                        switch (OutputType)
                        {
                            case "ResponseModel":
                                return response = JsonConvert.DeserializeObject<ResponseModel>(responseString);

                            case "AbsentLogForm[]":
                                return response = JsonConvert.DeserializeObject<AbsentLogForm[]>(responseString);

                            case "UserListForm[]":
                                return response = JsonConvert.DeserializeObject<UserListForm[]>(responseString);

                            case "UserListForm2[]":
                                return response = JsonConvert.DeserializeObject<UserListForm2[]>(responseString);

                            case "int[]":
                                return response = JsonConvert.DeserializeObject<int[]>(responseString);

                            case "string[]":
                                return response = JsonConvert.DeserializeObject<string[]>(responseString);

                            case "List<ClassroomListForm[]>":
                                return response = JsonConvert.DeserializeObject<List<ClassroomListForm[]>>(responseString);

                            case "TeachSubstitution[]":
                                return response = JsonConvert.DeserializeObject<TeachSubstitution[]>(responseString);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ++retry;
                    if (retry > MaxRetry / 2) DependencyService.Get<IToast>().ShowShort($"Dữ liệu lỗi, đang thử lại lần {retry}/5...");
                    Debug.WriteLine($"Dữ liệu vào lỗi ({ex.Message}), đang thử lại {retry}/5");
                    if (retry == MaxRetry) break;
                    await Task.Delay(retry * 1000);
                    continue;
                }
                break;
            }
            if (!CustomNullResponse.Equals("") && retry == MaxRetry)
            {
                DependencyService.Get<IToast>().ShowShort(CustomNullResponse);
                switch (OutputType)
                {
                    case "ResponseModel":
                        return response = new ResponseModel();

                    case "AbsentLogForm[]":
                        return response = new AbsentLogForm[0];

                    case "UserListForm[]":
                        return response = new UserListForm[0];

                    case "UserListForm2[]":
                        return response = new UserListForm2[0];

                    case "int[]":
                        return response = new int[0];

                    case "string[]":
                        return response = new string[0];

                    case "List<ClassroomListForm[]>":
                        return response = new List<ClassroomListForm[]>();

                    case "TeachSubstitution[]":
                        return response = new TeachSubstitution[0];
                        
                }
            }
            return response;
        }

        public async Task<bool> RetrieveAllUserDatabase()
        {
            int retry = 1;
            while (retry < 5)
            {
                try
                {
                    IReadOnlyCollection<FirebaseObject<UserListForm>> response2 = await fc.Child("Users").OnceAsync<UserListForm>();
                    List<UserListForm> response = new List<UserListForm>();
                    foreach(FirebaseObject<UserListForm> i in response2)
                    {
                        response.Add(i.Object);
                    }
                    var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
                    db.CreateTable<UserListForm>();
                    db.DeleteAll<UserListForm>();
                    db.InsertAll(response.OrderBy(x => x.StId));
                    db.Dispose();
                }
                catch (Exception ex)
                {
                    ++retry;
                    DependencyService.Get<IToast>().ShowShort($"Lỗi: Không thể kết nối với csdl. Đang thử lại {retry}/5");
                    Debug.WriteLine($"Khởi tạo lỗi ({ex.Message}):, thử lại {retry}/5");
                    await Task.Delay(retry * 1000);
                    continue;
                }
                break;
            }
            return true;
        }

        public async Task<bool> GetGlobalLogStat()
        {
            DateTime CurDateTime = DateTime.Now + UserData.OffsetWithNIST;
            if (!UserData.ForcedStatusUpdate && CurDateTime <= UserData.LastStatusUpdate.AddMinutes(5)) return true;
            int[] response = (int[])await HttpPolly(new FeedbackModel()
            {
                Mode = "19",
            }, true, "int[]");
            if(response[0] == -10)
            {
                UserData.IsTodayOff = true;
            }
            else UserData.IsTodayOff = false;
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            List<UserListForm> lmao = db.Table<UserListForm>().ToList();
            foreach (UserListForm usr in lmao)
            {
                usr.LogStatus = UserData.IsTodayOff ? 0 : response[usr.StId - 4];
            }
            UserData.IsUserLogin = UserData.IsTodayOff ? 0 : response[UserData.StudentIdDatabase - 4];
            db.UpdateAll(lmao);
            db.Dispose();
            UserData.ForcedStatusUpdate = false;
            UserData.LastStatusUpdate = CurDateTime;
            MessagingCenter.Send(Application.Current, "LoadCompletion", 1);
            return true;
        }

        public string ConvertToUnsign(string str)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        private class TmpRanking
        {
            public int RankingPoint { get; set; }
            public int User { get; set; }
            public int Ranking { get; set; }
        }

        public async Task<int> GetGlobalUserRanking()
        {
            int[] response = (int[])await HttpPolly(new FeedbackModel()
            {
                Mode = "3",
            }, true, "int[]");
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            List<TmpRanking> tmpRankings = new List<TmpRanking>();
            for (int i = 0; i < response.Length; ++i)
            {
                tmpRankings.Add(new TmpRanking()
                {
                    RankingPoint = response[i],
                    User = i + 4,
                });
            }
            tmpRankings = new List<TmpRanking>(tmpRankings.OrderByDescending(x => x.RankingPoint));
            int RankingBuffer = 0, RankingPointBuffer = 0;
            for (int i = 0; i < response.Length; ++i)
            {
                if (i == 0 || tmpRankings[i].RankingPoint != RankingPointBuffer) RankingBuffer++;
                tmpRankings[i].Ranking = RankingBuffer;
                RankingPointBuffer = tmpRankings[i].RankingPoint;
            }
            tmpRankings = new List<TmpRanking>(tmpRankings.OrderBy(x => x.User));
            UserData.UserRanking = tmpRankings[UserData.StudentIdDatabase - 4].Ranking;
            UserData.UserRankingPoint = tmpRankings[UserData.StudentIdDatabase - 4].RankingPoint;
            List<UserListForm> lmao = db.Table<UserListForm>().ToList();
            UserData.NoUserRanked = 0;
            foreach (UserListForm usr in lmao)
            {
                UserData.NoUserRanked += usr.IsHidden ^ 1;
                usr.RankingPoint = tmpRankings[usr.StId - 4].RankingPoint;
                usr.Ranking = tmpRankings[usr.StId - 4].Ranking;
            }
            db.UpdateAll(lmao);
            db.Dispose();
            MessagingCenter.Send(Application.Current, "LoadCompletion", 1);
            return 1;
        }

        public string RetrieveNameUser(int Id, bool ClassOnly = false)
        {
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            db.CreateTable<UserListForm>();
            string ResClass = db.Table<UserListForm>().ElementAt(Id - 4).StClass;
            var StringResult = ResClass + " - " + db.Table<UserListForm>().ElementAt(Id - 4).StName;
            db.Dispose();
            return (ClassOnly ? ResClass : StringResult);
        }

        public async Task<bool> CheckUserTableExist()
        {
            var db = new SQLiteConnection(GlobalVariables.localUserDatabasePath);
            db.CreateTable<UserListForm>();
            if (db.Table<UserListForm>().ToList().Count == 0)
            {
                await RetrieveAllUserDatabase();
            }
            db.Dispose();
            return true;
        }

        public async void CheckUpdates()
        {
            ResponseModel response = (ResponseModel)await HttpPolly(new FeedbackModel() { Mode = "11" });
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
            string resultt;
            try
            {
                resultt = Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
            }
            catch
            {
                DependencyService.Get<IToast>().ShowShort("Có lỗi xảy ra (Mã hóa QR lỗi).");
                resultt = "";
            }
            return resultt;
        }

        public string Base64Decode(string base64EncodedData)
        {
            if (base64EncodedData == null) return null;
            string resultt;
            try
            {
                resultt = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
            }
            catch
            {
                DependencyService.Get<IToast>().ShowShort("Có lỗi xảy ra (Giải mã QR lỗi).");
                resultt = "";
            }
            return resultt;
        }

        public async Task<bool> UpdateCurLocation()
        {
            Xamarin.Essentials.Location School = new Xamarin.Essentials.Location()
            {
                Latitude = UserData.SchoolLat,
                Longitude = UserData.SchoolLon
            };
            try
            {
                var resultt = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(20)));
                DateTime CurDateTime = DateTime.Now + UserData.OffsetWithNIST;
                UserData.LastGPSUpdate = CurDateTime;
                double dist = resultt.CalculateDistance(School, DistanceUnits.Kilometers);
                if (resultt.IsFromMockProvider)
                {
                    UserData.LastMockLoc = resultt;
                    UserData.IsLastTimeMock = true;
                    UserData.IsAtSchool = false;
                }
                else
                {
                    if (UserData.IsLastTimeMock && resultt.CalculateDistance(UserData.LastMockLoc, DistanceUnits.Kilometers) <= 1.0) UserData.IsAtSchool = false;
                    else
                    {
                        if (dist * 1000 >= UserData.SchoolDist) UserData.IsAtSchool = false;
                        else
                        {
                            UserData.IsAtSchool = true;
                        }
                        UserData.IsLastTimeMock = false;
                    }
                }
                return true;
            }
            catch
            {
                UserData.IsAtSchool = false;
                UserData.IsLastTimeMock = false;
                return false;
            }
        }

        public async void Firebase_SendLog(int STId, string Mistake = "NONE", bool IsOverwriteOk = false, bool IsSentToGG = true, bool SendMedical = false, bool Mani = false, DateTime DateManipulate = new DateTime())
        {
            string QueryName = RetrieveNameUser(STId);
            if (Mani)
            {
                DateTime CurDateTime = DateManipulate;
                Debug.WriteLine("Khống: Không phát hiện trùng, đang ghi");
                Firebase_SendLog2(STId, Mistake, IsSentToGG, SendMedical, true, DateManipulate);
                return;
            }
            if (UserData.IsTodayOff)
            {
                DependencyService.Get<IToast>().ShowShort("Lỗi (Ngoài giờ): " + QueryName);
                return;
            }
            Debug.WriteLine($"Khởi tạo: {QueryName}");
            int retry = 1;
            while (retry < 5)
            {
                try
                {
                    DateTime CurDateTime = DateTime.Now + UserData.OffsetWithNIST;
                    var tmp3 = await CrossCloudFirestore.Current.Instance.Collection("logging").WhereEqualsTo("StId", STId).OrderBy("Timestamp", true).WhereGreaterThanOrEqualsTo("Timestamp", CurDateTime.Date).GetAsync();
                    var tmp = tmp3.ToObjects<InboundLog>().ToList();
                    if (tmp.Count > 0)
                    {
                        DateTime oldLogTime = tmp[0].Timestamp.ToDateTime().ToLocalTime();
                        if (IsOverwriteOk && tmp[0].Mistake != Mistake)
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
                                    Spacing = 5,
                                    Children =
                                {
                                new Label {Text = "Bạn có thể ghi đè biên bản cũ (không đổi thời gian), hoặc hủy biên bản này.", FontSize=15, HorizontalTextAlignment=TextAlignment.Center, Margin=new Thickness(10,0)},
                                new Frame
                                {
                                    CornerRadius = 10,
                                    HasShadow = true,
                                    HeightRequest = 40,
                                    Content = new StackLayout
                                    {
                                        Orientation = StackOrientation.Horizontal,
                                        Children = {
                                            new Label {Text = "Cũ", FontSize=20, FontAttributes=FontAttributes.Bold, MinimumWidthRequest=40},
                                            new StackLayout
                                            {
                                                HorizontalOptions=LayoutOptions.EndAndExpand,
                                                Children =
                                                {
                                                    new Label {Text = QueryName, FontSize=15, LineBreakMode=LineBreakMode.HeadTruncation},
                                                    new StackLayout
                                                    {
                                                        Orientation = StackOrientation.Horizontal,

                                                        Children =
                                                        {
                                                            new Label {Text = tmp[0].Mistake.Equals("NONE") ? "" : tmp[0].Mistake, TextColor = Color.Red, LineBreakMode=LineBreakMode.HeadTruncation},
                                                            new Label {
                                                                Text = oldLogTime.ToString("HH:mm:ss"), FontSize = 20, FontAttributes=FontAttributes.Bold,
                                                                TextColor=tmp[0].LoginStatus == 1 ? Color.Green : Color.Orange, HorizontalTextAlignment=TextAlignment.End, MinimumWidthRequest=90},
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
                                            new Label {Text = "Mới", FontSize=20, FontAttributes=FontAttributes.Bold, MinimumWidthRequest=40},
                                            new StackLayout
                                            {
                                                HorizontalOptions=LayoutOptions.EndAndExpand,
                                                Children =
                                                {
                                                    new Label {Text = QueryName, FontSize=15, LineBreakMode=LineBreakMode.HeadTruncation},
                                                    new StackLayout
                                                    {
                                                        Orientation = StackOrientation.Horizontal,

                                                        Children =
                                                        {
                                                            new Label {Text = Mistake.Equals("NONE") ? "" : Mistake, TextColor = Color.Red, LineBreakMode=LineBreakMode.HeadTruncation},
                                                            new Label {
                                                                Text = oldLogTime.ToString("HH:mm:ss"), FontSize = 20, FontAttributes=FontAttributes.Bold,
                                                                TextColor=tmp[0].LoginStatus == 1 ? Color.Green : Color.Orange, HorizontalTextAlignment=TextAlignment.End, MinimumWidthRequest=90},
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
                                Firebase_OverwriteLog(tmp[0], oldLogTime, Mistake);
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
                        Firebase_SendLog2(STId, Mistake, IsSentToGG, SendMedical);
                    }
                }
                catch (Exception ex)
                {
                    ++retry;
                    DependencyService.Get<IToast>().ShowShort($"Lỗi: Không thể kết nối với csdl. Đang thử lại {retry}/5");
                    Debug.WriteLine($"Khởi tạo lỗi ({ex.Message}): {QueryName}, thử lại {retry}/5");
                    await Task.Delay(retry * 1000);
                    continue;
                }
                break;
            }
        }

        public async void Firebase_OverwriteLog(InboundLog curLog, DateTime logTime, string Mistake = "NONE")
        {
            DateTime CurDateTime = DateTime.Now + UserData.OffsetWithNIST;
            curLog.Mistake = Mistake;
            curLog.ReporterId = UserData.StudentIdDatabase;
            await CrossCloudFirestore.Current.Instance.Collection("logging").Document(curLog.Keys).UpdateAsync(curLog);
            _ = await HttpPolly(
                new FeedbackModel()
                {
                    Mode = "6",
                    Contents = curLog.StId.ToString(),
                    Contents2 = logTime.DayOfYear.ToString(),
                    Contents3 = curLog.LoginStatus.ToString(),
                    Contents4 = Mistake.Equals("NONE") ? "0" : (Mistake.Count(x => x.Equals(';')) + 1).ToString(),
                }, false);
            DependencyService.Get<IToast>().ShowShort("Ghi đè thành công");
            Debug.WriteLine($"Ghi đè OK");
        }

        public async void Firebase_SendLog2(int STId, string Mistake = "NONE", bool IsSentToGG = true, bool SendMedical = false, bool Mani = false, DateTime DateManipulate = new DateTime())
        {
            string QueryName = RetrieveNameUser(STId);
            Debug.WriteLine($"Gửi lên Firebase: {QueryName}");
            int retry;
            DateTime CurDateTime = DateTime.Now + UserData.OffsetWithNIST;
            if (Mani)
            {
                CurDateTime = DateManipulate;
            }
            TimeSpan CurTime = new TimeSpan(CurDateTime.Hour, CurDateTime.Minute, CurDateTime.Second);
            InboundLog curLog = new InboundLog();
            if (CurTime >= UserData.StartTime && CurTime <= UserData.EndTime || Mani)
            {
                curLog.LoginStatus = CurTime > UserData.LateTime ? 2 : 1;
                var SubLog = new
                {
                    StId = STId,
                    ReporterId = UserData.StudentIdDatabase,
                    Mistake,
                    curLog.LoginStatus,
                    StClass = RetrieveNameUser(STId, true),
                    Timestamp = FieldValue.ServerTimestamp,
                    NoMistake = curLog.LoginStatus - 1 + (Mistake.Equals("NONE") ? 0 : Mistake.Count(x => x.Equals(';')) + 1)
                };
                curLog.StId = STId;
                curLog.Mistake = SubLog.Mistake;
                curLog.ReporterId = UserData.StudentIdDatabase;
                retry = 1;
                while (retry < 5)
                {
                    try
                    {
                        if (Mani)
                        {
                            var LogKeys = await CrossCloudFirestore.Current.Instance.Collection("logging").AddAsync(new
                            {
                                StId = STId,
                                ReporterId = UserData.StudentIdDatabase,
                                Mistake,
                                curLog.LoginStatus,
                                StClass = RetrieveNameUser(STId, true),
                                Timestamp = new Timestamp(CurDateTime),
                                NoMistake = curLog.LoginStatus - 1 + (Mistake.Equals("NONE") ? 0 : Mistake.Count(x => x.Equals(';')) + 1)
                            });
                            curLog.Keys = LogKeys.Id;
                        }
                        else
                        {
                            var LogKeys = await CrossCloudFirestore.Current.Instance.Collection("logging").AddAsync(SubLog);
                            curLog.Keys = LogKeys.Id;
                        }
                    }
                    catch (Exception ex)
                    {
                        ++retry;
                        DependencyService.Get<IToast>().ShowShort($"Lỗi: Không thể kết nối với csdl. Đang thử lại {retry}/5");
                        Debug.WriteLine($"Gửi lên Firebase ({ex.Message}) lỗi: {QueryName}, thử lại {retry}/5");
                        await Task.Delay(retry * 1000);
                        continue;
                    }
                    break;
                }
                if (SendMedical)
                {
                    Debug.WriteLine($"Gửi yêu cầu Y tế: {QueryName}");
                    retry = 1;
                    while (retry < 5)
                    {
                        try
                        {
                            await fc.Child("Personal").Child($"{CurDateTime.Year}_{CurDateTime.DayOfYear}").Child(STId.ToString()).PutAsync(new { Medical = 1 });
                        }
                        catch (Exception ex)
                        {
                            ++retry;
                            DependencyService.Get<IToast>().ShowShort($"Lỗi: Không thể kết nối với csdl. Đang thử lại {retry}/5");
                            Debug.WriteLine($"Gửi lên Firebase Medical ({ex.Message}) lỗi: {QueryName}, thử lại {retry}/5");
                            await Task.Delay(retry * 1000);
                            continue;
                        }
                        break;
                    }
                }
                if (!IsSentToGG) return;
                Debug.WriteLine($"Gửi lên GG Sheet: {QueryName}");
                DependencyService.Get<IToast>().ShowShort("Đã gửi: " + QueryName);
                ResponseModel response = (ResponseModel)await HttpPolly(new FeedbackModel()
                {
                    Mode = "6",
                    Contents = STId.ToString(),
                    Contents2 = CurDateTime.DayOfYear.ToString(),
                    Contents3 = curLog.LoginStatus.ToString(),
                    Contents4 = Mistake.Equals("NONE") ? "0" : (Mistake.Count(x => x.Equals(';')) + 1).ToString(),
                });
                if (response.Message1 == 3)
                {
                    retry = 1;
                    while (retry < 5)
                    {
                        try
                        {
                            await CrossCloudFirestore.Current.Instance.Collection("logging").Document(curLog.Keys).DeleteAsync();
                        }
                        catch (Exception ex)
                        {
                            ++retry;
                            DependencyService.Get<IToast>().ShowShort($"Lỗi: Không thể kết nối với csdl. Đang thử lại {retry}/5");
                            Debug.WriteLine($"Xóa ({ex.Message}) lỗi: {QueryName}, thử lại {retry}/5");
                            await Task.Delay(retry * 1000);
                            continue;
                        }
                        break;
                    }
                    DependencyService.Get<IToast>().ShowShort("Lỗi (HS Báo nghỉ): " + QueryName);
                }
                else
                {
                    SQLiteConnection db2 = new SQLiteConnection(GlobalVariables.localLogHistDatabasePath);
                    db2.CreateTable<LogListForm>();
                    Debug.WriteLine($"Thêm vào db local: {QueryName}");
                    LogListForm SentLog = new LogListForm()
                    {
                        Keys = curLog.Keys,
                        LoginStatus = curLog.LoginStatus,
                        ReporterId = UserData.StudentIdDatabase,
                        Mistake = curLog.Mistake,
                        StId = STId,
                        Timestamp = (CurDateTime.AddHours(-7).Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerMillisecond,
                    };
                    db2.Insert(SentLog);
                    db2.Dispose();
                }
            }
            else
            {
                DependencyService.Get<IToast>().ShowShort("Lỗi (Ngoài giờ): " + QueryName);
            }
        }
    }
}
