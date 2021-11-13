using Android.Widget;
using QRdangcap.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(Toast_Android))]

namespace QRdangcap.Droid
{
    public class Toast_Android : IToast
    {
        public void Show(string message)
        {
            Android.Widget.Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }

        public void ShowShort(string message)
        {
            Android.Widget.Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
        }
    }
}