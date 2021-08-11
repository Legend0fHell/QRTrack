using QRdangcap.LocalDatabase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace QRdangcap.ViewModel
{
    public class StatsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<LogListForm> _LogListFirebase = new ObservableCollection<LogListForm>();
        public string _RetrieveLog = "Đang lấy dữ liệu...";
        public ObservableCollection<LogListForm> LogListFirebase
        {
            get => _LogListFirebase;
            set
            {
                _LogListFirebase = value;
                OnPropertyChanged(nameof(LogListFirebase));
            }
        }
        public string RetrieveLog
        {
            get => _RetrieveLog;
            set
            {
                _RetrieveLog = value;
                OnPropertyChanged(nameof(RetrieveLog));
            }
        }
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
