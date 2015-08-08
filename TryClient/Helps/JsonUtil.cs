using System; 
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace TryClient
{
    /// <summary>
    /// JsonUtil：工具类，实现Json串的解析
    /// </summary>
    public static class JsonUtil
    {
        public static List<T> JSONStringToList<T>(this string JsonStr)
        {  
            try
            {
                List<T> objs = JsonConvert.DeserializeObject<List<T>>(JsonStr);
                return objs;
            }
            catch
            {
                return null;
            }
        }

        public static T Deserialize<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json.Trim())))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                    return (T)serializer.ReadObject(ms);
                }
            }
            catch
            {
                return default(T);
            }
        }

    }
}
