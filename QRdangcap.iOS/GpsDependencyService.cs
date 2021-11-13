using CoreLocation;
using Foundation;
using QRdangcap.iOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(GpsDependencyService))]
namespace QRdangcap.iOS
{
    public class GpsDependencyService : IGpsDependencyService
    {

        public bool IsGpsEnable()
        {
            if (CLLocationManager.Status == CLAuthorizationStatus.Denied)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void OpenSettings()
        {
            var WiFiURL = new NSUrl("prefs:root=WIFI");

            if (UIApplication.SharedApplication.CanOpenUrl(WiFiURL))
            {
                UIApplication.SharedApplication.OpenUrl(WiFiURL);
            }
            else
            {
                UIApplication.SharedApplication.OpenUrl(new NSUrl("App-Prefs:root=WIFI"));
            }
        }
    }
}