using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TryClient.Helps;
using Windows.Networking.BackgroundTransfer;

namespace TryClient.Models
{
    [DataContract]
    public class DownloadInfo:BaseInfo
    {
        [DataMember]
        public string Vid { get; set; }
        [DataMember]
        public int Resolution { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Date { get; set; }
        [DataMember]
        public string Duration { get; set; }
        [DataMember]
        public string ImageUrl { get; set; }
        [DataMember]
        public string VideoUrl { get; set; }
        [DataMember]
        public string FileName 
        {
            get { return Title+".mp4";}
        }
         
        [IgnoreDataMember]
        public CancellationTokenSource Cts { get; set; }

        //选中标识
        private bool _IsSelected;
        [DataMember]
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
        [DataMember]
        public bool IsSeletedBorder
        {
            get { return _IsSelectedBorder; }
            set
            {
                _IsSelectedBorder = value;
                NotifyPropertyChange("IsSeletedBorder");
            }
        }

        private DownloadStatus _Status;
        [DataMember]
        public DownloadStatus Status
        {
            get { return _Status; }
            set { _Status = value; NotifyPropertyChange("Status"); }
        }

        private double _DownProgress;
        [DataMember]
        public double DownProgress
        {
            get { return _DownProgress; }
            set { _DownProgress = value; NotifyPropertyChange("DownProgress"); }
        } 
    }
}
