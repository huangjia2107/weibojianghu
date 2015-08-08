using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace TryClient.Helps
{
    public enum DownloadStatus
    {
        None,
        Compare,
        Wait,
        Downloading,
        Pause,
        Complete,
        Error,
        UrlError,
        NameNull,
        CreatFileError
    }

    public enum Success
    {
        True,
        False,
        Unsure
    }

    public enum SaveDataType
    {
        MediaData,
        DownloadedData
    }

    public enum MsgCode
    {
        Exit_App, //退出
        Flush_Error,//刷新数据错误

        No_Internet, //无网络
        Cannot_PlayMedia,//无法播放

        FirstMedia_Completed,//最新一期播放完毕
        NewestMedia_Completed,//最新一期播放完毕
        CurMedia_First,//当前视频是第一期
        CurMedia_Newest,//当前视频是最新一期

    }

    public class ResolutionEventArgs : EventArgs
    {
        public int ResolutionIndex { get; set; } 
    }

    public class MediaStateEventArgs:EventArgs
    {
        public MediaElementState state { get; set; }
    }
}
