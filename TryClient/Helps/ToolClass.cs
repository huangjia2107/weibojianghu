using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TryClient.Models;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace TryClient.Helps
{
    public class ToolClass
    {
        private const string MediaDataFileName = "mediadata.xml";
        private const string DownloadedDataFileName = "downloadeddata.xml";

        public static string GetDateFormatByStr(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return null;

            if (dateStr.Length < 8)
                return null;

            return dateStr.Substring(0, 4) + "-" + dateStr.Substring(4, 2) + "-" + dateStr.Substring(6, 2);
        }

        public async static Task<bool> DeleteExistedFile(StorageFolder folder, string fileName)
        {
            try 
            {
                if (await FileExists(folder, fileName))
                {
                    StorageFile file = await folder.GetFileAsync(fileName);
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async static Task<bool> FileExists(StorageFolder folder, string fileName)
        {
            try
            {
                await folder.GetFileAsync(fileName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async static Task<StorageFile> GetStorageFile(StorageFolder folder, string fileName)
        {
            try
            {
                return await folder.GetFileAsync(fileName);
            }
            catch (Exception ex)
            {
                return null;
            }
        } 

        private async static Task WriteXML<T>(ObservableCollection<T> objectList,SaveDataType saveDataType)
        {
            await ThreadPool.RunAsync((sender) => WriteXMLAsync<T>(objectList,saveDataType).Wait(), Windows.System.Threading.WorkItemPriority.Normal);  
        }

        private async static Task<ObservableCollection<T>> ReadXML<T>(SaveDataType saveDataType)
        {
            ObservableCollection<T> templist=null;
            await ThreadPool.RunAsync(async (sender) => {
                templist=await ReadXMLAsync<T>(saveDataType);
            }, Windows.System.Threading.WorkItemPriority.Normal);

            return templist;
        }

        public async static Task WriteXMLAsync<T>(ObservableCollection<T> objectList,SaveDataType saveDataType)
        {
            string filename=saveDataType==SaveDataType.MediaData?MediaDataFileName:DownloadedDataFileName;

            StorageFile sessionFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            IRandomAccessStream sessionRandomAccess = await sessionFile.OpenAsync(FileAccessMode.ReadWrite);
            IOutputStream sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0);
            var serializer = new XmlSerializer(typeof(ObservableCollection<T>), new Type[] { typeof(T) });

            //AsStreamForWrite()需要System.IO命名空间
            serializer.Serialize(sessionOutputStream.AsStreamForWrite(), objectList);
            sessionRandomAccess.Dispose();
            await sessionOutputStream.FlushAsync();
            sessionOutputStream.Dispose();   
        } 

        public async static Task<ObservableCollection<T>> ReadXMLAsync<T>(SaveDataType saveDataType)
        {
            string filename = saveDataType == SaveDataType.MediaData ? MediaDataFileName : DownloadedDataFileName;

            if (await FileExists(ApplicationData.Current.LocalFolder, filename) == false)
                return null;
             
            try
            {
                StorageFile sessionFile = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
                IInputStream sessionInputStream = await sessionFile.OpenReadAsync();

                var serializer = new XmlSerializer(typeof(ObservableCollection<T>), new Type[] { typeof(T) });
                ObservableCollection<T> allvideolist = (ObservableCollection<T>)serializer.Deserialize(sessionInputStream.AsStreamForRead());
                sessionInputStream.Dispose();

                return allvideolist;
            }
            catch { return null; } 
        } 
    }
}
