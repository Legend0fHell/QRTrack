using System.Collections.ObjectModel;
using System.ComponentModel;

namespace QRdangcap.ViewModel
{
    public class TimetablePageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LearnRow> _TimetableColl = new ObservableCollection<LearnRow>();
        public string _ClassName = "Đang tải dữ liệu";
        public string _AddInfo;

        public ObservableCollection<LearnRow> TimetableColl
        {
            get => _TimetableColl;
            set
            {
                _TimetableColl = value;
                OnPropertyChanged(nameof(TimetableColl));
            }
        }

        public string ClassName
        {
            get => _ClassName;
            set
            {
                _ClassName = value;
                OnPropertyChanged(nameof(ClassName));
            }
        }

        public string AddInfo
        {
            get => _AddInfo;
            set
            {
                _AddInfo = value;
                OnPropertyChanged(nameof(AddInfo));
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}