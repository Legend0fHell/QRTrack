using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace QRdangcap.ViewModel
{
    public class RestDayForm
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Color Color { get; set; }
        public bool AllDay => true;
    }
    public class RestViewViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<RestDayForm> schedules;
        
        private void RaiseOnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<RestDayForm> Schedules
        {
            get
            {
                return schedules;
            }
            set
            {
                schedules = value;
                RaiseOnPropertyChanged("Schedules");
            }
        }
    }
}
