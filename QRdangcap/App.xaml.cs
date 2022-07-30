using Plugin.FirebasePushNotification;
using System.Globalization;
using Xamarin.Forms;

namespace QRdangcap
{
    public partial class App : Application
    {
        public App()
        {
            // Syncfusion License (Community - 19.4.0.38 : https://www.syncfusion.com/account/downloads)
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Your_License_Key");
            CultureInfo forceVNCulture = new CultureInfo("vi-VN");
            CultureInfo.DefaultThreadCurrentCulture = forceVNCulture;

            CrossFirebasePushNotification.Current.OnTokenRefresh += Current_OnTokenRefresh;
            MainPage = new AppShell();
        }

        private void Current_OnTokenRefresh(object source, FirebasePushNotificationTokenEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Token: {e.Token}");
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
