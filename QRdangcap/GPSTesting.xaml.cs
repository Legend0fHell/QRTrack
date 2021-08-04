using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using QRdangcap.GoogleDatabase;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GPSTesting : ContentPage
    {
        public Location School = new Location();
        public GPSTesting()
        {
            InitializeComponent();
            School = new Location()
            {
                Latitude = UserData.SchoolLat,
                Longitude = UserData.SchoolLon
            };
            SchoolLoc.Text = "Vị trí của trường (KĐ/VĐ): " + School.Latitude.ToString("F5") + " " + School.Longitude.ToString("F5");
            Approx.Text = "Sai số cho phép (mét): " + UserData.SchoolDist.ToString("F2");
            Accuracy.Text = "GPS đang bị làm giả?: ";
            UserLoc.Text = "Vị trí của bạn (KĐ/VĐ): ";
            Distance.Text = "Khoảng cách từ bạn tới trường (mét): ";
            IsInSchool.Text = "Bạn đang ở trường?: " + (UserData.IsAtSchool ? "Có" : "Không");
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var result = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(30)));
            double dist = result.CalculateDistance(School, DistanceUnits.Kilometers);
            UserLoc.Text = "Vị trí của bạn (KĐ/VĐ): " + result.Latitude.ToString("F5") + " " + result.Longitude.ToString("F5");
            Accuracy.Text = "GPS đang bị làm giả?: " + (result.IsFromMockProvider ? "Có" : "Không");
            Distance.Text = "Khoảng cách từ bạn tới trường (mét): " + (dist * 1000).ToString("F5");
            if (dist * 1000 >= UserData.SchoolDist) UserData.IsAtSchool = false;
            else UserData.IsAtSchool = true;
            IsInSchool.Text = "Bạn đang ở trường?: " + (UserData.IsAtSchool ? "Có" : "Không");
        }
    }
}