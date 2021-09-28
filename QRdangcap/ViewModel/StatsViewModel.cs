using QRdangcap.DatabaseModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace QRdangcap.ViewModel
{
    public class StatsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LogListForm> _LogListFirebase = new ObservableCollection<LogListForm>();
        public string _RetrieveLog = "Đang lấy dữ liệu...";
        public bool _IsVisi = false;

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

        public bool IsVisi
        {
            get => _IsVisi;
            set
            {
                _IsVisi = value;
                OnPropertyChanged(nameof(IsVisi));
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}