using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryClient.Models
{
    public class ListToPlayParam
    {
        public MonthToListParam param { get; set; }
        public List<VideoShowInfo> AllVidellist { get; set; }
        public VideoPlayInfo PlayInfo { get; set; }
        public bool isfrommain { get; set; }
    }
}
