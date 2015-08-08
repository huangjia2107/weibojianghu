using System.Collections.Generic;

namespace TryClient.Models
{
    public class VideoPlayInfo
    {
        public string bimg { get; set; }
        public string vid { get; set; }
        public string m_download { get; set; }
        public string textid { get; set; }
        public string img { get; set; }
        public string key { get; set; }
        public movie movie { get; set; }//
        public string tag { get; set; }
        public string rela_opera { get; set; }
        public string opera_id { get; set; }
        public string Subject { get; set; }
        public string hd { get; set; }
        public List<movieparam> rfiles { get; set; }//
        public string duration { get; set; }
        public string cpm { get; set; }
        public string tags { get; set; }
        public string cid { get; set; }
        public string user_id { get; set; }
    }

    public class movie
    {
        public string keyword { get; set; }
        public string m_copyright { get; set; }
        public string class1 { get; set; }
        public string version { get; set; }
        public string copyright { get; set; }
        public string total_items { get; set; }
        public string coop { get; set; }
        public string last_sub_index { get; set; }
        public string coop_vid { get; set; }
        public string chk_yn { get; set; }
        public string finished { get; set; }
    }

    public class movieparam
    {
        public string filesize { get; set; }
        public string totaltime { get; set; }
        public string url { get; set; }
        public string type { get; set; }
    }
}
