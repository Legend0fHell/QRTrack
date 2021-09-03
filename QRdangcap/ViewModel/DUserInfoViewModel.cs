using QRdangcap.DatabaseModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace QRdangcap.ViewModel
{
    public class DUserInfoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LogListForm> _LogListFirebase = new ObservableCollection<LogListForm>();
        public string _RetrieveLog = "Đang lấy dữ liệu...";
        public bool _IsVisi = false;
        public bool _IsEditAllowed = false;
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
        public bool IsEditAllowed
        {
            get => _IsEditAllowed;
            set
            {
                _IsEditAllowed = value;
                OnPropertyChanged(nameof(IsEditAllowed));
            }
        }
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}