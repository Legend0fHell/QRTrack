using Xamarin.Forms;
using System.Globalization;
using Plugin.FirebasePushNotification;

namespace QRdangcap
{
    public partial class App : Application
    {
        public App()
        {
            // Syncfusion License (Community - 19.2.0.48)
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDc5MDg5QDMxMzkyZTMyMmUzMGVvRk5GSmNqaDU4cUhYZGxqK1hoUm03eDR4QWtaZEZlNzV4K292b3k1WU09");
            InitializeComponent();
            CultureInfo forceVNCulture = new CultureInfo("vi-VN");
            CultureInfo.DefaultThreadCurrentCulture = forceVNCulture;

            CrossFirebasePushNotification.Current.OnTokenRefresh += Current_OnTokenRefresh;
            MainPage = new AppShell();
            System.Diagnostics.Debug.WriteLine("== DEBUGGING MODE ACTIVATED ==");
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
