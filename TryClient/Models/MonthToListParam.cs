using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryClient.Models
{
    public class MonthToListParam
    {
        public int YearIndex { get; set; }
        public int MonthIndex { get; set; }
        public ObservableCollection<VideoShowInfo> AllVidellist { get; set; }
    }
}
