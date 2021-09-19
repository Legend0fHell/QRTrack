using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Syncfusion.SfSchedule.XForms;
using System.Collections.ObjectModel;

namespace QRdangcap.ViewModel
{
    public class TimetableForm
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Color Color { get; set; }
        public bool AllDay => true;
        public string Name { get; set; }
    }
    public class TimetableViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<TimetableForm> timetables;
        private ObservableCollection<DateTime> blackoutDates;
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
    }
}
