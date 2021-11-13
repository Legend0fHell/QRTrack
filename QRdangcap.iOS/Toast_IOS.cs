using Foundation;
using QRdangcap.iOS;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(Toast_IOS))]

namespace QRdangcap.iOS
{
    public class Toast_IOS : IToast
    {
        private const double LONG_DELAY = 3.5;
        private const double SHORT_DELAY = 1.5;

        public void Show(string message)
        {
            ShowAlert(message, LONG_DELAY);
        }

        public void ShowShort(string message)
        {
            ShowAlert(message, SHORT_DELAY);
        }

        private void ShowAlert(string message, double seconds)
        {
            var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);

            var alertDelay = NSTimer.CreateScheduledTimer(seconds, obj =>
            {
                DismissMessage(alert, obj);
            });

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
        }

        private void DismissMessage(UIAlertController alert, NSTimer alertDelay)
        {
            if (alert != null)
            {
                alert.DismissViewController(true, null);
            }

            if (alertDelay != null)
            {
                alertDelay.Dispose();
            }
        }
    }
}