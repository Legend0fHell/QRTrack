using QRdangcap.DatabaseModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;

namespace QRdangcap.ViewModel
{
    public class TimetableForm : AbsentLogForm
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Color Color { get; set; }
        public bool AllDay => false;
        public string Name { get; set; }
    }

    public class TimetableViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<TimetableForm> timetables;
        private ObservableCollection<DateTime> blackoutDates;
        private bool isAbsentAllowed;

        private void RaiseOnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<TimetableForm> Timetables
        {
            get
            {
                return timetables;
            }
            set
            {
                timetables = value;
                RaiseOnPropertyChanged("Timetables");
            }
        }

        public ObservableCollection<DateTime> BlackoutDates
        {
            get
            {
                return blackoutDates;
            }
            set
            {
                blackoutDates = value;
                RaiseOnPropertyChanged("BlackoutDates");
            }
        }

        public bool IsAbsentAllowed
        {
            get
            {
                return isAbsentAllowed;
            }
            set
            {
                isAbsentAllowed = value;
                RaiseOnPropertyChanged("IsAbsentAllowed");
            }
        }
    }
}