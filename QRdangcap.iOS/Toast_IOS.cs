using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using QRdangcap.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(Toast_IOS))]

namespace QRdangcap.iOS
{
    public class Toast_IOS : IToast
    {
        const double LONG_DELAY = 3.5;
        const double SHORT_DELAY = 1.5;
        public void Show(string message)
        {
            ShowAlert(message, LONG_DELAY);
        }
        public void ShowShort(string message)
        {
            ShowAlert(message, SHORT_DELAY);
        }
        void ShowAlert(string message, double seconds)
        {
            var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);

            var alertDelay = NSTimer.CreateScheduledTimer(seconds, obj =>
            {
                DismissMessage(alert, obj);
            });

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
        }

        void DismissMessage(UIAlertController alert, NSTimer alertDelay)
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