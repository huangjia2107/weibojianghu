using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryClient.Helps;

namespace TryClient.Models
{
    public class VideoMonthInfo : BaseInfo
    {
        //视频ID
        public string Vid { get; set; }
        //标题
        public string Title { get; set; }
        //日期
        public string Date { get; set; }
        //播放次数
        public string Times { get; set; }
        //时长
        public string TotalTime { get; set; }
        //截图
        public string Img { get; set; }
        //日期
        public int DayIndex { get; set; }

        //选中标识
        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                NotifyPropertyChange("IsSelected");
            }
        }

        //选中标识框
        private bool _IsSelectedBorder;
        public bool IsSeletedBorder
        {
            get { return _IsSelectedBorder; }
            set
            {
                _IsSelectedBorder = value;
                NotifyPropertyChange("IsSeletedBorder");
            }
        }
    }
}
