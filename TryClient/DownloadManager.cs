using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TryClient.Helps;
using TryClient.Models;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.Web;

namespace TryClient
{
    public class DownloadManager
    {
        //正在下载的
        public ObservableCollection<DownloadInfo> downloadingInfoList = new ObservableCollection<DownloadInfo>();
        //已下载的
        public ObservableCollection<DownloadInfo> downloadedInfoList = new ObservableCollection<DownloadInfo>();

        CoreDispatcher dispatcher = null;
        public bool IsDownloding { get; set; }

        private List<DownloadOperation> activeDownloads = new List<DownloadOperation>();
        private StorageFolder Downloadfolder = KnownFolders.VideosLibrary;

        private class DownloadParam
        {
            public DownloadInfo downloadInfo { get; set; }
            public DownloadOperation downloadOperation { get; set; }
        }

        private static readonly DownloadManager instance = new DownloadManager();
        private DownloadManager()
        {
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        public static DownloadManager GetInstance()
        {
            return instance;
        }

        public async void StartDownloadAllList()
        {
            while (downloadingInfoList.Count > 0)
            {
                IsDownloding = true;
                DownloadInfo downloadinfo = null;
                foreach (DownloadInfo down in downloadingInfoList)
                {
                    downloadinfo = down;
                    break;
                }
                await StartDownload(downloadinfo);
            }
            IsDownloding = false;
        }

        /*
        // Enumerate the downloads that were going on in the background while the app was closed.
        public async Task DiscoverActiveDownloadsAsync()
        {
            IReadOnlyList<DownloadOperation> downloads = null;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Discovery error", ex))
                {
                    throw;
                }
                return;
            }

            //Log("Loading background downloads: " + downloads.Count);

            if (downloads.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (DownloadOperation download in downloads)
                {
                    //Log(String.Format(CultureInfo.CurrentCulture, "Discovered background download: {0}, Status: {1}", download.Guid, download.Progress.Status));

                    // Attach progress and completion handlers.
                    DownloadInfo d = downloadingInfoList.Where(down => down.DownOperation == download).FirstOrDefault();
                    if (d != null)
                        tasks.Add(HandleDownloadAsync(false, d));
                }

                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
                // download only when the first one completed; attach to the third download when the second one
                // completes etc. We want to attach to all downloads immediately.
                // If there are actions that need to be taken once downloads complete, await tasks here, outside
                // the loop.
                await Task.WhenAll(tasks);
            }
        }*/

        private async Task StartDownload(DownloadInfo downloadInfo)
        {
            downloadInfo.Status = DownloadStatus.Compare;
            Uri source;
            if (!Uri.TryCreate(downloadInfo.VideoUrl.Trim(), UriKind.Absolute, out source))
            {
                downloadInfo.Status = DownloadStatus.UrlError;
                return;
            }

            string destination = downloadInfo.FileName;
            if (string.IsNullOrWhiteSpace(destination))
            {
                downloadInfo.Status = DownloadStatus.NameNull;
                return;
            }

            StorageFile destinationFile;
            try
            {
                destinationFile = await Downloadfolder.CreateFileAsync(
                    destination, CreationCollisionOption.ReplaceExisting);
            }
            catch
            {
                //rootPage.NotifyUser("Error while creating file: " + ex.Message, NotifyType.ErrorMessage);
                downloadInfo.Status = DownloadStatus.CreatFileError;
                ToolClass.DeleteExistedFile(Downloadfolder, destination);
                return;
            }

            BackgroundDownloader downloader = new BackgroundDownloader();
            // downloader.CostPolicy = BackgroundTransferCostPolicy.Always;
            DownloadOperation download = downloader.CreateDownload(source, destinationFile);
            download.Priority = BackgroundTransferPriority.Default;

            downloadInfo.Cts = new CancellationTokenSource();

            DownloadParam downloadParameter = new DownloadParam
            {
                downloadInfo = downloadInfo,
                downloadOperation = download
            };

            // Attach progress and completion handlers.
            await HandleDownloadAsync(true, downloadParameter);
        }

        private async Task HandleDownloadAsync(bool start, DownloadParam downloadParameter)
        {
            try
            {
                // LogStatus("Running: " + download.Guid, NotifyType.StatusMessage);

                // Store the download so we can pause/resume.
                activeDownloads.Add(downloadParameter.downloadOperation);

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(download =>
                {
                    double percent = 0;
                    if (download.Progress.TotalBytesToReceive > 0)
                    {
                        percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
                        Debug.WriteLine("当前进度：{0}", percent);
                    }
                    if (percent != 100)
                    {
                        downloadParameter.downloadInfo.Status = DownloadStatus.Downloading;
                    }
                    else
                        downloadParameter.downloadInfo.Status = DownloadStatus.Complete;
                    downloadParameter.downloadInfo.DownProgress = percent;
                });

                if (start)
                { 
                    await downloadParameter.downloadOperation.StartAsync().AsTask(downloadParameter.downloadInfo.Cts.Token, progressCallback);
                }
                else
                { 
                    await downloadParameter.downloadOperation.AttachAsync().AsTask(downloadParameter.downloadInfo.Cts.Token, progressCallback);
                }

                ResponseInformation response = downloadParameter.downloadOperation.GetResponseInformation(); 
            } 
            catch (Exception ex)
            {
                downloadParameter.downloadInfo.Status = DownloadStatus.Error;
                if (downloadParameter.downloadInfo.Cts != null && downloadParameter.downloadInfo.Cts.IsCancellationRequested == false)
                {
                    downloadParameter.downloadInfo.Cts.Cancel();
                    downloadParameter.downloadInfo.Cts.Dispose();
                }
                DeleteLocalFile(downloadParameter.downloadInfo);

                return;
                /*
                if (!IsExceptionHandled("Execution error", ex, downloadParameter.downloadOperation))
                {
                    throw;
                }
                 * */
            }
            finally
            {
                activeDownloads.Remove(downloadParameter.downloadOperation);

                if (downloadParameter.downloadInfo.Status == DownloadStatus.Complete)
                {
                    Debug.WriteLine("Download <<" + downloadParameter.downloadInfo.Title + ">> Complete.");

                    if (downloadingInfoList.Contains(downloadParameter.downloadInfo))
                        downloadingInfoList.Remove(downloadParameter.downloadInfo);

                    if (!DownloadedListContains(downloadParameter.downloadInfo.Vid))
                        downloadedInfoList.Add(downloadParameter.downloadInfo);
                }

            }
        }

        private bool IsExceptionHandled(string title, Exception ex, DownloadOperation download = null)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
            if (error == WebErrorStatus.Unknown)
            {
                return false;
            }

            if (download == null)
            {
                //LogStatus(String.Format(CultureInfo.CurrentCulture, "Error: {0}: {1}", title, error),  NotifyType.ErrorMessage);
            }
            else
            {
                // LogStatus(String.Format(CultureInfo.CurrentCulture, "Error: {0} - {1}: {2}", download.Guid, title,error), NotifyType.ErrorMessage);
            }

            return true;
        }

        public async Task CancelDownloads()
        {
            List<DownloadInfo> tempList = new List<DownloadInfo>();
            foreach (DownloadInfo info in downloadingInfoList)
            {
                if (info.IsSelected == true)
                {
                    if (info.Cts != null && info.Cts.IsCancellationRequested==false)
                    {
                        info.Cts.Cancel();
                        info.Cts.Dispose();
                    }
                    tempList.Add(info);
                }
            }


            foreach (DownloadInfo info in tempList)
            {
                if (downloadingInfoList.Contains(info))
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
                    {
                         downloadingInfoList.Remove(info);
                    });
                }

                await DeleteLocalFile(info);
            }
            tempList = null;
        }

        public void SetSelectedStatus(ObservableCollection<DownloadInfo> downloadInfoList, bool isSelected, bool isSeletedBorder)
        {
            foreach (DownloadInfo info in downloadInfoList)
            {
                info.IsSelected = isSelected;
                info.IsSeletedBorder = isSeletedBorder;
            }
        }

        public async Task<StorageFile> GetStorageFileAsync(DownloadInfo info)
        {
            try
            {
                if (await ToolClass.FileExists(Downloadfolder, info.FileName))
                {
                    return await Downloadfolder.GetFileAsync(info.FileName);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task DeleteLocalFile(DownloadInfo info)
        {
            //删除本地文件
            await ToolClass.DeleteExistedFile(Downloadfolder,info.FileName);
        }

        public bool DownloadingListContains(string vid)
        {
            DownloadInfo downloadInfo = downloadingInfoList.Where(info => info.Vid == vid).FirstOrDefault();
            if (downloadInfo == null)
                return false;
            else
                return true;
        }

        public bool DownloadedListContains(string vid)
        {
            DownloadInfo downloadInfo = downloadedInfoList.Where(info => info.Vid == vid).FirstOrDefault();
            if (downloadInfo == null)
                return false;
            else
                return true;
        }
    }
}
