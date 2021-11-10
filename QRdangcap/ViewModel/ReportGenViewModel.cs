using QRdangcap.DatabaseModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace QRdangcap.ViewModel
{
    public class ReportGenViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LogListForm> folders = new ObservableCollection<LogListForm>();

        public ObservableCollection<LogListForm> Folders
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
    }
}