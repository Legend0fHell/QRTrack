using System;
using System.IO;
using Android.Content;
using Java.IO;
using Xamarin.Forms;
using System.Threading.Tasks;


[assembly: Dependency(typeof(SaveAndroid))]

class SaveAndroid: ISave
    {
    [Obsolete]
    public async Task SaveAndView(string fileName, String contentType, MemoryStream stream)
        {
        //string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
                string extension = Android.Webkit.MimeTypeMap.GetFileExtensionFromUrl(Android.Net.Uri.FromFile(file).ToString());
                string mimeType = Android.Webkit.MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(path, mimeType);
                await Xamarin.Essentials.Launcher.OpenAsync(file.Path);
//                 Android.App.Application.Context.StartActivity(Intent.CreateChooser(intent, "Choose App"));
            }
        }
    }
