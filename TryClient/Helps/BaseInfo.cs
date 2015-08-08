using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryClient.Helps
{
    public class BaseInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            { 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
