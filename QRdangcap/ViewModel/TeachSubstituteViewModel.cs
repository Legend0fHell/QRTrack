using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.CommunityToolkit.ObjectModel;

namespace QRdangcap.ViewModel
{
    public class TeachSubstituteViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TeachSubstitution> _SubsColl = new ObservableCollection<TeachSubstitution>();
        public string _ClassName = "Đang tải dữ liệu";
        public string _AddInfo;

        public ObservableCollection<TeachSubstitution> SubsColl
        {
            get => _SubsColl;
            set
            {
                _SubsColl = value;
                OnPropertyChanged(nameof(SubsColl));
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