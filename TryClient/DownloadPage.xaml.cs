using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TryClient.Helps;
using TryClient.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class DownloadPage : Page
    {
        CoreDispatcher dispatcher = null;
        public DownloadPage()
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
            int index = (int)(e.Parameter);
            pivot.SelectedIndex = index;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            DownloadingList.ItemsSource = DownloadManager.GetInstance().downloadingInfoList;
            DownloadedList.ItemsSource = DownloadManager.GetInstance().downloadedInfoList;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;

            if (CancelDownloadPanel.Visibility == Visibility.Visible)
                return;

            if (SelectBtn.Visibility == Visibility.Collapsed)
            {
                ResetBtnStatus();
                e.Handled = true;
                return;
            }

            Frame.Navigate(typeof(MainPage));
        }

        private void ResetBtnStatus()
        {
            DownloadManager.GetInstance().SetSelectedStatus(DownloadManager.GetInstance().downloadingInfoList, false, false);
            DownloadManager.GetInstance().SetSelectedStatus(DownloadManager.GetInstance().downloadedInfoList, false, false);

            SelectBtn.Visibility = Visibility.Visible;
            SelectClearBtn.Visibility = Visibility.Collapsed;
            SelectAbortBtn.Visibility = Visibility.Collapsed;
            SelectClearBtn.IsEnabled = false;
            SelectAbortBtn.IsEnabled = false;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectBtn.Visibility == Visibility.Collapsed)
            {
                ResetBtnStatus();
            }
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectBtn.Visibility = Visibility.Collapsed;
            if (pivot.SelectedIndex == 0)
            {
                DownloadManager.GetInstance().SetSelectedStatus(DownloadManager.GetInstance().downloadingInfoList, false, true);
                SelectAbortBtn.Visibility = Visibility.Visible;
            }
            else
            {
                DownloadManager.GetInstance().SetSelectedStatus(DownloadManager.GetInstance().downloadedInfoList, false, true);
                SelectClearBtn.Visibility = Visibility.Visible;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // await DownloadManager.GetInstance().DiscoverActiveDownloadsAsync();
        }

        private void UpdateButtonStatusBySelectedCount(Button btn, ObservableCollection<DownloadInfo> downloadInfoList)
        {
            foreach (DownloadInfo info in downloadInfoList)
            {
                if (info.IsSelected == true)
                {
                    btn.IsEnabled = true;
                    return;
                }
            }

            btn.IsEnabled = false;
        }

        private void DownloadingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DownloadingList.SelectedItem == null)
                return;

            DownloadInfo info = DownloadingList.SelectedItem as DownloadInfo;

            if (SelectBtn.Visibility == Visibility.Collapsed)
            {
                if (info.IsSelected)
                {
                    info.IsSelected = false;
                    UpdateButtonStatusBySelectedCount(SelectAbortBtn, DownloadManager.GetInstance().downloadingInfoList);
                }
                else
                {
                    info.IsSelected = true;
                    SelectAbortBtn.IsEnabled = true;
                }

                DownloadingList.SelectedIndex = -1;
            }
        }

        private async void DownloadedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DownloadedList.SelectedItem == null)
                return;

            DownloadInfo info = DownloadedList.SelectedItem as DownloadInfo;

            //多选
            if (SelectBtn.Visibility == Visibility.Collapsed)
            {
                if (info.IsSelected)
                {
                    info.IsSelected = false;
                    UpdateButtonStatusBySelectedCount(SelectClearBtn, DownloadManager.GetInstance().downloadedInfoList);
                }
                else
                {
                    info.IsSelected = true;
                    SelectClearBtn.IsEnabled = true;
                }

                DownloadedList.SelectedIndex = -1;

                return;
            }

            //播放
            StorageFile file = await DownloadManager.GetInstance().GetStorageFileAsync(info);
            if (file == null)
            {
                //文件不存在
                MessageDialog messageDialog = new MessageDialog("文件不存在,是否删除该条记录？", "无法播放");
                messageDialog.Commands.Add(new UICommand("删除", new UICommandInvokedHandler(command =>
                {
                    DownloadManager.GetInstance().downloadedInfoList.Remove(info);
                })));
                messageDialog.Commands.Add(new UICommand("保留", new UICommandInvokedHandler(command =>
                {

                })));

                messageDialog.DefaultCommandIndex = 0;
                messageDialog.CancelCommandIndex = 2;
                await messageDialog.ShowAsync();
                DownloadedList.SelectedIndex = -1;
            }
            else
            {
                await Task.Delay(100);
                Frame.Navigate(typeof(PlayLocalFilePage), info);
            }
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedIndex == 0)
            {
                if(DownloadManager.GetInstance().downloadingInfoList.Count>0)
                {
                    DownloadManager.GetInstance().SetSelectedStatus(DownloadManager.GetInstance().downloadingInfoList, true, true);
                    SelectAbortBtn.IsEnabled = true;
                }
            }

            if (pivot.SelectedIndex == 1)
            {
                if (DownloadManager.GetInstance().downloadedInfoList.Count>0)
                {
                    DownloadManager.GetInstance().SetSelectedStatus(DownloadManager.GetInstance().downloadedInfoList, true, true);
                    SelectClearBtn.IsEnabled = true;
                }
            }
        }

        private async void SelectAbortBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog("确认终止所选下载？", "取消下载");
            messageDialog.Commands.Add(new UICommand("是", new UICommandInvokedHandler(command =>
            {
                DownloadManager downloadManager = DownloadManager.GetInstance();
                commandBar.Visibility = Visibility.Collapsed;
                CancelDownloadPanel.Visibility = Visibility.Visible;

                ThreadPool.RunAsync(async (workItem) =>
                { 
                    await downloadManager.CancelDownloads(); 

                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (downloadManager.downloadingInfoList.Count == 0)
                            downloadManager.IsDownloding = false;

                        ResetBtnStatus();
                        CancelDownloadPanel.Visibility = Visibility.Collapsed;
                        commandBar.Visibility = Visibility.Visible;
                    });
                });

            })));
            messageDialog.Commands.Add(new UICommand("否", new UICommandInvokedHandler(command =>
            {
            })));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private async void SelectClearBtn_Click(object sender, RoutedEventArgs e)
        {
            List<DownloadInfo> infoList = new List<DownloadInfo>();
            foreach (DownloadInfo info in DownloadManager.GetInstance().downloadedInfoList)
            {
                if (info.IsSelected == true && info.IsSeletedBorder == true)
                    infoList.Add(info);
            }

            MessageDialog messageDialog = new MessageDialog("是否同时删除视频源文件？", "删除记录");
            messageDialog.Commands.Add(new UICommand("同时删除", new UICommandInvokedHandler(async command =>
            {
                foreach (DownloadInfo info in infoList)
                {
                    DownloadManager.GetInstance().downloadedInfoList.Remove(info);
                    await DownloadManager.GetInstance().DeleteLocalFile(info);
                }
            })));
            messageDialog.Commands.Add(new UICommand("只删记录", new UICommandInvokedHandler(command =>
            {
                foreach (DownloadInfo info in infoList)
                {
                    DownloadManager.GetInstance().downloadedInfoList.Remove(info);
                }
            })));

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 2;
            await messageDialog.ShowAsync();

            ResetBtnStatus();
        }
    }
}
