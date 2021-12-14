using QRdangcap.DatabaseModel;
using System.ComponentModel;
using Xamarin.CommunityToolkit.ObjectModel;

namespace QRdangcap.ViewModel
{
    public class MedicalInfoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableRangeCollection<UserListForm> _DisplayList = new ObservableRangeCollection<UserListForm>();

        public ObservableRangeCollection<UserListForm> DisplayList
        {
            get => _DisplayList;
            set
            {
                _DisplayList = value;
                OnPropertyChanged(nameof(DisplayList));
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}