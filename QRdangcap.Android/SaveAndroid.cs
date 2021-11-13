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
        string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
        Java.IO.File myDir = new Java.IO.File(root + "/CYBLaNha");
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