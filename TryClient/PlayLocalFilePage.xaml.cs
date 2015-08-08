using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TryClient.Helps;
using TryClient.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TryClient
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayLocalFilePage : Page
    {
        DownloadInfo downloadInfo=null;
        CoreDispatcher dispatcher = null;
        StorageFolder Downloadfolder = KnownFolders.VideosLibrary;

        public PlayLocalFilePage()
        {
            this.InitializeComponent();
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            downloadInfo = (DownloadInfo)e.Parameter;
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            if (DisplayInformation.GetForCurrentView().CurrentOrientation != DisplayOrientations.Landscape)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            mediaPlayControl.IsDispRequest = true;
            OpenFileToMediaControl();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            mediaPlayControl.IsDispRequest = false;
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;

            if (DisplayInformation.GetForCurrentView().CurrentOrientation != DisplayOrientations.Portrait)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        { 
            Frame.Navigate(typeof(DownloadPage),1);
            e.Handled = true;
        }

        private void OpenFileToMediaControl()
        {
            Task task = new Task(async () =>
            {
                StorageFile file = await ToolClass.GetStorageFile(Downloadfolder, downloadInfo.FileName);
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                { 
                    mediaPlayControl.ResolutionStr=HashMap.ResolutionMap[downloadInfo.Resolution];
                    mediaPlayControl.MediaTitle = downloadInfo.Title;
                    mediaPlayControl.MediaStorageFile = file;
                });
            });
            task.Start();
        }

        private void mediaPlayControl_MediaEnded(object sender, EventArgs e)
        { 
            Frame.Navigate(typeof(DownloadPage),1); 
        } 
    }
}
