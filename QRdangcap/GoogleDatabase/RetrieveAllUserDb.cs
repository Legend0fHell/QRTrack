using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;
using System.Globalization;
using QRdangcap.LocalDatabase;
using SQLite;
using System.Diagnostics;
using Xamarin.Essentials;

namespace QRdangcap.GoogleDatabase
{
    public class RetrieveAllUserDb
    {
        readonly Stopwatch excTime = new Stopwatch();
        public async void RetrieveAllUserDatabase()
        {
            DependencyService.Get<IToast>().ShowShort("Đang tải dữ liệu...");
            excTime.Reset();
            excTime.Start();
            var client = new HttpClient();
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
            DependencyService.Get<IToast>().ShowShort("Tải thành công! (" + excTime.ElapsedMilliseconds + "ms)");
        }
        public async void RetrieveAllLogDatabase()
        {
            //DependencyService.Get<IToast>().ShowShort("Đang tải dữ liệu...");
            excTime.Reset();
            excTime.Start();
            var client = new HttpClient();
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
        }
        
        public async void CheckUpdates()
        {
            var client = new HttpClient();
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
            if(response.Status != GlobalVariables.ClientVersion)
            {
                await App.Current.MainPage.DisplayAlert("Đã có phiên bản mới, " + response.Status + "!",
                    "Cập nhật vào " + response.DateTimeMessage.ToOffset(new TimeSpan(7,0,0)).ToString("dd.MM.yyyy HH:mm") + 
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
    }
}
 