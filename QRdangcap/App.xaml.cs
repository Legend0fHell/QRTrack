using Xamarin.Forms;
using System.Globalization;

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

            MainPage = new AppShell();
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
