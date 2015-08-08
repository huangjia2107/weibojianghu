using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryClient.Helps
{
    public class HashMap
    {
        public static Dictionary<int, string> MonthMap = new Dictionary<int, string>()
        {
            {12,    "Dec."},
            {11,    "Nov."},
            {10,    "Oct."},
            {9,     "Sep."},
            {8,     "Aug."},
            {7,     "Jul."},
            {6,     "Jun."},
            {5,     "May."},
            {4,     "Apr."},
            {3,     "Mar."},
            {2,     "Feb."},
            {1,     "Jan."}
        };

        public static Dictionary<DownloadStatus, string> DownStatusMap = new Dictionary<DownloadStatus, string>()
        {
            {DownloadStatus.None,            ""},
            {DownloadStatus.Compare,         "准备..."},
            {DownloadStatus.Wait,            "等待..."},
            {DownloadStatus.Downloading,     "下载中..."},
            {DownloadStatus.Pause,           "暂停中"},
            {DownloadStatus.Complete,        "下载完成."},
            {DownloadStatus.Error,           "下载异常."},
            {DownloadStatus.UrlError,        "下载路径异常."},
            {DownloadStatus.NameNull,        "名称异常."},
            {DownloadStatus.CreatFileError,  "创建文件异常."}
        };

        public static Dictionary<int, string> ResolutionMap = new Dictionary<int, string>() 
        {
            {2,   "标清"},
            {3,   "高清"}
        };

        public static Dictionary<MsgCode, string> MsgCodeMap = new Dictionary<MsgCode, string>() 
        {
            {MsgCode.Exit_App,                       "再按一次退出程序"},
            {MsgCode.Flush_Error,                    "数据返回有误,请稍后重试"},
            {MsgCode.No_Internet,                    "标清"},
            {MsgCode.Cannot_PlayMedia,               "当前视频无法播放,请稍后重试."},
            {MsgCode.FirstMedia_Completed,           "第一期已经播放完毕"},
            {MsgCode.NewestMedia_Completed,          "最新一期已经播放完毕"},
            {MsgCode.CurMedia_First,          "当前视频已经是第一期"},
            {MsgCode.CurMedia_Newest,          "当前视频已经是最新一期"},
        }; 
    }
}
