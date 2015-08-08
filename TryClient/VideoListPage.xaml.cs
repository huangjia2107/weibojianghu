using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TryClient.Helps;
using TryClient.Models;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Networking.Connectivity;
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
    public sealed partial class VideoListPage : Page
    {
        ObservableCollection<VideoMonthInfo> videomonthlist;
        MonthToListParam monthToListParam = null;

        CoreDispatcher dispatcher = null;
        AppSettings appSetting = null;
        public VideoListPage()
        {
            this.InitializeComponent();
            dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            appSetting = new AppSettings();
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            monthToListParam = (MonthToListParam)e.Parameter;
            videomonthlist = GetVideoMonthList(monthToListParam.AllVidellist, monthToListParam.YearIndex, monthToListParam.MonthIndex);
            ShowMonthVideoList.ItemsSource = videomonthlist;
            dateShow.Text = monthToListParam.YearIndex + "." + monthToListParam.MonthIndex;

            if (DisplayInformation.GetForCurrentView().CurrentOrientation != DisplayOrientations.Portrait)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;

            if (LoadMediaInfoPanel.Visibility == Visibility.Visible)
                return;
            if (AddDownloadPanel.Visibility == Visibility.Visible)
                return;

            if (SelectBtn.Visibility == Visibility.Collapsed)
            {
                ResetButtonStatus();
            }
            else
                Frame.Navigate(typeof(MainPage));
        }

        private void ResetButtonStatus()
        {
            foreach (VideoMonthInfo info in videomonthlist)
            {
                info.IsSelected = false;
                info.IsSeletedBorder = false;
            }

            SelectBtn.Visibility = Visibility.Visible;
            SelectDownloadBtn.IsEnabled = false;
        }

        private ObservableCollection<VideoMonthInfo> GetVideoMonthList(ObservableCollection<VideoShowInfo> videoList, int year, int month)
        {
            return new ObservableCollection<VideoMonthInfo>(
                (from video in videoList
                 where int.Parse(video.Sub_Index.Substring(0, 4)) == year
                 where int.Parse(video.Sub_Index.Substring(4, 2)) == month
                 select new VideoMonthInfo
                 {
                     Vid = video.Vid,
                     Title = video.Title,
                     Date = ToolClass.GetDateFormatByStr(video.Sub_Index),
                     Img = video.Img,
                     DayIndex = int.Parse(video.Sub_Index.Substring(6, 2)),
                     Times = video.Times,
                     TotalTime = video.TotalTime
                 }).OrderByDescending(data => data.DayIndex));
        }

        private void UpdateDownloadStatusBySelectedCount()
        {
            foreach (VideoMonthInfo info in videomonthlist)
            {
                if (info.IsSelected == true)
                {
                    SelectDownloadBtn.IsEnabled = true;
                    return;
                }
            }

            SelectDownloadBtn.IsEnabled = false;
        }

        private async void ShowMonthVideoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShowMonthVideoList.SelectedItem == null)
                return;

            VideoMonthInfo info = ShowMonthVideoList.SelectedItem as VideoMonthInfo;

            //下载
            if (SelectBtn.Visibility == Visibility.Collapsed)
            {
                if (info.IsSelected)
                {
                    info.IsSelected = false;
                    UpdateDownloadStatusBySelectedCount();
                }
                else
                {
                    if (DownloadManager.GetInstance().DownloadingListContains(info.Vid))
                    {
                        tipText.Text = "正在下载列表中已经包含该项!";
                        MsgVisibleStoryboard.Begin();
                    }
                    else if (DownloadManager.GetInstance().DownloadedListContains(info.Vid))
                    {
                        MessageDialog messageDialog = new MessageDialog("该项下载已完成,是否重新下载？", "重复下载");
                        messageDialog.Commands.Add(new UICommand("重新下载", new UICommandInvokedHandler(command =>
                        {
                            info.IsSelected = true;
                            SelectDownloadBtn.IsEnabled = true;
                        })));
                        messageDialog.Commands.Add(new UICommand("不要下载", new UICommandInvokedHandler(command =>
                        {

                        })));

                        messageDialog.DefaultCommandIndex = 0;
                        messageDialog.CancelCommandIndex = 1;
                        await messageDialog.ShowAsync();
                    }
                    else
                    {
                        info.IsSelected = true;
                        SelectDownloadBtn.IsEnabled = true;
                    }
                }

                ShowMonthVideoList.SelectedIndex = -1;
                return;
            }

            //播放

            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (InternetConnectionProfile == null)
            {
                tipText.Text = "当前无网络连接,无法播放!";
                MsgVisibleStoryboard.Begin();
                ShowMonthVideoList.SelectedIndex = -1;
                return;
            }
            else if (InternetConnectionProfile.IsWlanConnectionProfile == false && InternetConnectionProfile.IsWwanConnectionProfile == false)
            {
                tipText.Text = "当前无网络连接,无法播放!";
                MsgVisibleStoryboard.Begin();
                ShowMonthVideoList.SelectedIndex = -1;
                return;
            }

            commandBar.Visibility = Visibility.Collapsed;
            LoadMediaInfoPanel.Visibility = Visibility.Visible;

            ThreadPool.RunAsync(async (workItem) =>
            {
                VideoPlayInfo playInfo = await donet_sdk.getMobileVideoInfo(info.Vid);
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (playInfo == null)
                    {
                        tipText.Text = HashMap.MsgCodeMap[MsgCode.Cannot_PlayMedia];
                        MsgVisibleStoryboard.Begin();
                        LoadMediaInfoPanel.Visibility = Visibility.Collapsed;
                        commandBar.Visibility = Visibility.Visible;
                        ShowMonthVideoList.SelectedIndex = -1;
                        return;
                    }

                    ListToPlayParam listToPlayParam = new ListToPlayParam
                    {
                        param = monthToListParam,
                        AllVidellist = monthToListParam.AllVidellist.ToList(),
                        PlayInfo = playInfo,
                        isfrommain = false
                    };

                    LoadMediaInfoPanel.Visibility = Visibility.Collapsed;
                    commandBar.Visibility = Visibility.Visible;

                    await Task.Delay(100);
                    Frame.Navigate(typeof(PlayVideoPage), listToPlayParam);
                });
            });
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectBtn.Visibility = Visibility.Collapsed;
            foreach (VideoMonthInfo info in videomonthlist)
            {
                info.IsSeletedBorder = true;
            }
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            bool IsContainsRepeateItem = false;
            foreach (VideoMonthInfo info in videomonthlist)
            {
                if (DownloadManager.GetInstance().DownloadingListContains(info.Vid))
                {
                    info.IsSelected = false;
                    IsContainsRepeateItem = true;
                }
                else if (DownloadManager.GetInstance().DownloadedListContains(info.Vid))
                {
                    info.IsSelected = false;
                    IsContainsRepeateItem = true;
                }
                else
                    info.IsSelected = true;
            }
            UpdateDownloadStatusBySelectedCount();

            if (IsContainsRepeateItem == true)
            {
                tipText.Text = "正在下载或已下载的项不能再添加!";
                MsgVisibleStoryboard.Begin();
            }
        }


        private void SelectDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (InternetConnectionProfile == null)
            {
                tipText.Text = "当前无网络连接,无法下载!";
                MsgVisibleStoryboard.Begin();
                return;
            }
            if (InternetConnectionProfile.IsWlanConnectionProfile == false && InternetConnectionProfile.IsWwanConnectionProfile == false)
            {
                tipText.Text = "当前无网络连接,无法下载!";
                MsgVisibleStoryboard.Begin();
                return;
            }


            if (InternetConnectionProfile.IsWlanConnectionProfile == false && InternetConnectionProfile.IsWwanConnectionProfile == true)
            {
                if ((bool)(appSetting.DownloadInternetType) == false)
                {
                    tipText.Text = "设置中2G/3G/4G网络下不允许下载!";
                    MsgVisibleStoryboard.Begin();
                    return;
                }
            }

            VideoPlayInfo playInfo = null;
            bool ContainsInvalidMedia = false;
            DownloadManager downloadManager = DownloadManager.GetInstance();
            commandBar.Visibility = Visibility.Collapsed;
            AddDownloadPanel.Visibility = Visibility.Visible;

            ThreadPool.RunAsync(async (workItem) =>
            {
                foreach (VideoMonthInfo info in videomonthlist)
                {
                    if (info.IsSelected == true)
                    {
                        playInfo = await donet_sdk.getMobileVideoInfo(info.Vid);
                        if (playInfo == null)
                        {
                            ContainsInvalidMedia = true;
                            continue;
                        }

                        downloadManager.downloadingInfoList.Add(
                            new DownloadInfo
                            {
                                Vid = playInfo.vid,
                                Resolution = (bool)(appSetting.DownloadResolution) == true ? 3 : 2,
                                Title = "[" + HashMap.ResolutionMap[(bool)(appSetting.DownloadResolution) == true ? 3 : 2] + "]" + playInfo.Subject,
                                Date = info.Date,
                                Duration = info.TotalTime,
                                ImageUrl = playInfo.img,
                                VideoUrl = (bool)(appSetting.DownloadResolution) == true ? playInfo.rfiles[3].url : playInfo.rfiles[2].url,
                                Status = DownloadStatus.Wait
                            }
                        );
                    }
                }

                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ResetButtonStatus();
                    AddDownloadPanel.Visibility = Visibility.Collapsed;
                    commandBar.Visibility = Visibility.Visible;
                    if (ContainsInvalidMedia == true)
                        tipText.Text = "个别视频因无效而未能添加进下载列表!";
                    else
                        tipText.Text = "下载任务添加完成!";
                    MsgVisibleStoryboard.Begin();

                    if (downloadManager.IsDownloding == false)
                        downloadManager.StartDownloadAllList();
                });
            });
        }
    }
}
