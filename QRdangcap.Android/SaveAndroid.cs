using Java.IO;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(SaveAndroid))]

internal class SaveAndroid : ISave
{
    [Obsolete]
    public async void SaveAndView(string fileName, String contentType, MemoryStream stream)
    {
        string root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        Java.IO.File myDir = new Java.IO.File(root);
        myDir.Mkdir();
        Java.IO.File file = new Java.IO.File(myDir, fileName);
        if (file.Exists()) file.Delete();
        FileOutputStream outs = new FileOutputStream(file);
        outs.Write(stream.ToArray());
        outs.Flush();
        outs.Close();
        if (file.Exists())
        {
            Android.Net.Uri path = Android.Net.Uri.FromFile(file);
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(path.Path),
                Title = "Mở báo cáo bằng ứng dụng.."
            });
        }
    }
}