using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using TryClient;
using TryClient.Models;
using TryClient.Helps;
using System.Collections.ObjectModel; 
using System.Threading.Tasks;

/// <summary>
///OpenApi 的摘要说明
///56api接口
/// </summary>
public class donet_sdk
{
    public donet_sdk()
    {
    }
    private static String appkeyOne = "3000005110";
    private static String secretOne = "54ed2eccd8b29ef2";
    private static String charset = "UTF-8";
    private static String meth = "Get";
    public static int total = 0;
 
    /// <summary>
    /// 时间戳转为C#格式时间
    /// </summary>
    /// <param name=”timeStamp”></param>
    /// <returns></returns>
    public static DateTime GetTime(string timeStamp)
    {
        DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970,1,1),TimeZoneInfo.Local); 
        long lTime = long.Parse(timeStamp + "0000000");
        TimeSpan toNow = new TimeSpan(lTime); 
        return dtStart.Add(toNow);
    }

    public async static Task<VideoPlayInfo> getMobileVideoInfo(string vid)
    {
        VideoPlayInfo playInfo = new VideoPlayInfo();

        //请求并获取返回的json串，截取符合json解析公共方法的部分。
        string str_json = await GetOpen56APi("http://oapi.56.com/video/mobile.json", "vid=" + vid);

        if (string.IsNullOrEmpty(str_json))
            return null;

        int begin = str_json.IndexOf(":{");
        int end = str_json.LastIndexOf("},");
        str_json = str_json.Substring(begin + 1, (end - begin));

        //调用Json解析公共方法，将Json串转化成List<WebsiteCategory>
        playInfo = JsonUtil.Deserialize<VideoPlayInfo>(str_json);
        return playInfo;
    }


    public async static Task<ObservableCollection<VideoShowInfo>> getVideo_listByJson(string param)
    {
        List<VideoShowInfo> showInfoList = new List<VideoShowInfo>();

        //请求并获取返回的json串，截取符合json解析公共方法的部分。
        string str_json = await GetOpen56APi("http://oapi.56.com/video/opera.json", param);

        if (string.IsNullOrEmpty(str_json))
            return null;

        int begin = str_json.IndexOf("[");
        int end = str_json.LastIndexOf("]");

        try 
        { 
            str_json = str_json.Substring(begin, (end - begin) + 1);
        //调用Json解析公共方法，将Json串转化成List<WebsiteCategory>
            showInfoList = JsonUtil.JSONStringToList<VideoShowInfo>(str_json);
        }
        catch
        {
            return null;
        }
        if (showInfoList != null)
            return new ObservableCollection<VideoShowInfo>(showInfoList);
        else
            return null;
    } 

    /*
    private static List<object> ListToListObj<T>(List<T> list)
    {
        List<object> result = new List<object>();
        list.ForEach(obj => result.Add(obj));
        return result;
    }
     * */

    #region 获取请求
    /// <summary>
    /// 获取请求
    /// </summary>
    /// <param name="apiurl">请求的接口</param>
    /// <param name="meth">请求方式</param>
    /// <param name="param">非系统参数</param>
    /// <param name="charset">编码</param>
    /// <param name="appkey">appkey</param>
    /// <param name="secret">secret</param>
    /// <returns></returns>
    public async static Task<string> GetOpen56APi(String apiurl, String param)
    {
        String str = "", PageCode = "", sign = "";

        Dictionary<object, object> ht = new Dictionary<object, object>();

        String[] tag = { "&" };

        String[] ss = param.Split(tag, StringSplitOptions.RemoveEmptyEntries);

        if (ss != null && ss.Length > 0)
        {
            for (int i = 0; i < ss.Length; i++)
            {
                String subPageCode = ss[i].ToString();

                String[] tagg = { "=" };

                String[] ss2 = subPageCode.Split(tagg, StringSplitOptions.RemoveEmptyEntries);

                if (ss2 != null && ss2.Length > 0)
                {
                    String key = ss2[0].ToString();

                    String value = ss2[1].ToString();

                    if (key != "" && value != "")
                    {
                        ht.Add(key, value);
                    }
                }
            }

            sign = signRequest(ht);

            str = apiurl + "?" + sign;

            try
            {
                PageCode = unicode_js_1(await GetPageCode(str, charset, meth)).Replace("\\", "");

            }
            catch (Exception ex)
            {
                PageCode = null;
                //PageCode = ex.ToString();
            }
        }

        return PageCode;
    }

    #endregion

    #region 获取请求,默认测试帐号
    
    /// <summary>
    /// unicode转中文（符合js规则的）
    /// </summary>
    /// <returns></returns>
    private static string unicode_js_1(string str)
    {
        string outStr = "";
        Regex reg = new Regex(@"(?i)\\u([0-9a-f]{4})");
        outStr = reg.Replace(str, delegate(Match m1)
        {
            return ((char)Convert.ToInt32(m1.Groups[1].Value, 16)).ToString();
        });
        return outStr;
    }
    #endregion

    #region Md5值加密
    /// <summary>
    /// md5Run16 用md5 16位 加密输入字符串
    /// </summary>
    /// <param name="strInput">输入字符串 </param>
    /// param name="num">md5指示位（16 or 32） </param>
    /// <returns>默认返回用md5 16位 加密后的字符串</returns>
    private static String md5Run(String strInput, int num)
    {
        if (strInput != null && strInput != "")
        {
            string hashedPassword =  MD5.GetMd5String(strInput);//md5 32位

            if (num == 32)
            {
                return hashedPassword.ToLower();
            }
            else
            {
                string MD5pwd32 = hashedPassword.Substring(8, 16).ToLower();//MD5 32位
            }
        }
        return "";
    }

    private static String signRequest(Dictionary<object,object> hm)
    {

        String appkey = appkeyOne;

        String secret = secretOne;

        String req = md5Run(mapToString(hm), 32);	//第一轮次计算 MD5加密

        String sdate2 = System.DateTime.Now.ToString("yyyy-M-d HH:mm:ss");

        String sdate1 = "1970-01-01 00:00:00";

        System.TimeSpan timespan = Convert.ToDateTime(sdate2) - Convert.ToDateTime(sdate1);

        Int64 ts = Convert.ToInt64(timespan.TotalSeconds);

        hm.Add("sign", md5Run(req + "#" + appkey + "#" + secret + "#" + ts, 32));   //第二轮次计算 MD5加密

        hm.Add("appkey", appkey);

        hm.Add("ts", ts);

        return mapToString(hm);
    }
    #endregion

    #region 将 map 中的参数及对应值转换为字符串
    private static String mapToString(Dictionary<object,object> hm)
    {
        int count = hm.Keys.Count;

        Object[] array = new Object[count];

        hm.Keys.CopyTo(array, 0);

        System.Array.Sort(array);

        String str = "";

        for (int i = 0; i < array.Count();i++ )
        {
            String key = array[i].ToString();

            if (i !=(array.Count()-1))
            {
                str += key + "=" + hm[key] + "&";
            }
            else
            {
                str += key + "=" + hm[key];
            }
        }
        return str;
    }
    #endregion

    #region 获取源码
    //获取指定页面的源代码
    private async static Task<string> GetPageCode(String PageURL, String Charset)
    {
        //存放目标网页的html
        String strHtml = "";
        try
        {

            // Uri myUri = new Uri(PageURL);
            //连接到目标网页
            WebRequest wreq = WebRequest.Create(PageURL);
            wreq.Method = "GET";
            HttpWebResponse wresp = await GetWebResponse(wreq);

            //采用流读取，并确定编码方式
            Stream s = wresp.GetResponseStream();
            StreamReader objReader = new StreamReader(s, System.Text.Encoding.GetEncoding(Charset));

            string strLine = "";
            //读取
            while (strLine != null)
            {
                strLine = objReader.ReadLine();
                if (strLine != null)
                {
                    strHtml += strLine.Trim();
                }
            }
            return strHtml;
        }
        catch (Exception ei)	//遇到错误，打印错误
        {
            return strHtml = ei.ToString();
        }
    }

    //获取指定页面的源代码
    private async static Task<string> GetPageCode(String PageURL, String Charset, String meth)
    {
        //存放目标网页的html
        String strHtml = "";
        try
        {

            // Uri myUri = new Uri(PageURL);
            //连接到目标网页
            WebRequest wreq = WebRequest.Create(PageURL);
            if (meth == "" || meth == null)
            {
                meth = "get";
            }
            wreq.Method = meth;
            HttpWebResponse wresp =await GetWebResponse(wreq);

            //采用流读取，并确定编码方式
            Stream s = wresp.GetResponseStream();
            StreamReader objReader = new StreamReader(s, System.Text.Encoding.GetEncoding(Charset));

            string strLine = "";
            //读取
            while (strLine != null)
            {
                strLine = objReader.ReadLine();
                if (strLine != null)
                {
                    strHtml += strLine.Trim();
                }
            }
            return strHtml;
        }
        catch (Exception ei)	//遇到错误，打印错误
        {
            return strHtml = ei.ToString();
        }
    } 

    private async static Task<HttpWebResponse> GetWebResponse(WebRequest wreq)
    {
        WebResponse webResponse;
        webResponse = await wreq.GetResponseAsync();
        return (HttpWebResponse)webResponse;
    }

    #endregion 

}
