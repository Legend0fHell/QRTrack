using Firebase.Database;
using Firebase.Database.Query;
using QRdangcap.DatabaseModel;
using QRdangcap.GoogleDatabase;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace QRdangcap.ViewModel
{
    public class ReportGenViewModel : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Folder> folders = new ObservableCollection<Folder>();
        public ObservableCollection<Folder> Folders
        {
            get { return folders; }
            set
            {
                folders = value;
                OnPropertyChanged(nameof(Folders));
            }
        }
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        }
        public ObservableCollection<File> Files { get; set; }
        public ObservableCollection<SubFile> SubFiles { get; set; }
    }
}