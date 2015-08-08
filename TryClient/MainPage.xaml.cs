
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
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=391641 上有介绍

namespace TryClient
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<VideoShowInfo> allvideolist;//所有视频列表
        ObservableCollection<MonthsData> monthlist;//月份信息
        int CurrentYear = 2014;

        List<VideoNewShow> imagelist = new List<VideoNewShow>();

        string connectionProfileInfo = string.Empty;
        ConnectionProfile InternetConnectionProfile = null;
        CoreDispatcher dispatcher = null;
        AppSettings appSettings = null;
        DispatcherTimer dt = new DispatcherTimer();
        bool IsExit = false;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            appSettings = new AppSettings();
            dt.Interval = TimeSpan.FromSeconds(3);
            dt.Tick += dt_Tick;

            HideStatusBard();
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            LoadDataFormFileOrInternet();
        }

        void dt_Tick(object sender, object e)
        {
            if (IsExit)
            {
                IsExit = false;
                dt.Stop();
                MsgVisibleStoryboard.Stop();
            }
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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

        async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            if (DownloadManager.GetInstance().downloadingInfoList.Count != 0)
            {
                MessageDialog messageDialog = new MessageDialog("退出将导致下载中任务失效？", "确认退出");
                messageDialog.Commands.Add(new UICommand("退出", new UICommandInvokedHandler(command =>
                {
                    DownloadManager downloadManager = DownloadManager.GetInstance();
                    commandBar.Visibility = Visibility.Collapsed;
                    SaveDataPanel.Visibility = Visibility.Visible;

                    ThreadPool.RunAsync(async (workItem) =>
                    {
                        await downloadManager.CancelDownloads();
                        await ToolClass.WriteXMLAsync<DownloadInfo>(downloadManager.downloadedInfoList, SaveDataType.DownloadedData);
                        Application.Current.Exit();
                    });

                })));
                messageDialog.Commands.Add(new UICommand("取消", new UICommandInvokedHandler(command =>
                {
                })));

                messageDialog.DefaultCommandIndex = 0;
                messageDialog.CancelCommandIndex = 1;
                await messageDialog.ShowAsync();
            }
            else
            {
                if (!IsExit)
                {
                    tipText.Text = HashMap.MsgCodeMap[MsgCode.Exit_App];
                    MsgVisibleStoryboard.Begin();
                    IsExit = true;
                    dt.Start();
                    e.Handled = true;
                }
                else
                {
                    dt.Stop();
                    MsgVisibleStoryboard.Stop();
                    await ToolClass.WriteXMLAsync<DownloadInfo>(DownloadManager.GetInstance().downloadedInfoList, SaveDataType.DownloadedData);
                    Application.Current.Exit();
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ShowMonthList.SelectedIndex = -1;
        }

        private static async void HideStatusBard()
        {
            var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            await statusBar.HideAsync();
        }

        private async void LoadDataFormFileOrInternet()
        {
            if (appSettings.IsFirstStart)
            {
                FlushData();
                appSettings.IsFirstStart = false;
            }
            else
            {
                if (appSettings.DataFlushWhileStart)
                {
                    FlushData();
                }
                else
                {
                    allvideolist = await ToolClass.ReadXMLAsync<VideoShowInfo>(SaveDataType.MediaData);
                    if (allvideolist == null)
                    {
                        FlushData();
                    }
                    else
                    {
                        monthlist = GetMonthsDataList(allvideolist, CurrentYear);
                        imagelist = GetImageList(allvideolist);

                        ShowPanel.MarkSource = imagelist;
                        ShowMonthList.ItemsSource = monthlist;
                    }
                }
            }

            ObservableCollection<DownloadInfo> downloadedInfoList = await ToolClass.ReadXMLAsync<DownloadInfo>(SaveDataType.DownloadedData);
            if (downloadedInfoList != null)
            {
                DownloadManager.GetInstance().downloadedInfoList = downloadedInfoList;
            }
        }

        private void FlushData()
        {
            LoadPanel.Visibility = Visibility.Visible;
            commandBar.Visibility = Visibility.Collapsed;

            if (CheckInternetIsConnect() == false)
               return;

            ThreadPool.RunAsync(async (workItem) =>
            {
                ObservableCollection<VideoShowInfo> templist = await donet_sdk.getVideo_listByJson("mid=6268");

                if (templist == null)
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        tipText.Text = HashMap.MsgCodeMap[MsgCode.Flush_Error];
                        MsgVisibleStoryboard.Begin();

                        LoadPanel.Visibility = Visibility.Collapsed;
                        noNetPath.Visibility = Visibility.Collapsed;
                        noNetText.Visibility = Visibility.Collapsed;
                        wifiPath.Visibility = Visibility.Collapsed;
                        wapPath.Visibility = Visibility.Collapsed;
                        loadText.Visibility = Visibility.Collapsed;

                        commandBar.Visibility = Visibility.Visible;
                        return;
                    });
                }
                else
                {
                    if (allvideolist != null)
                        allvideolist.Clear();
                    allvideolist = templist;
                }

                //只要刷新数据 则
                await ToolClass.WriteXMLAsync<VideoShowInfo>(allvideolist, SaveDataType.MediaData);

                monthlist = GetMonthsDataList(allvideolist, CurrentYear);
                imagelist = GetImageList(allvideolist);

                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ShowPanel.MarkSource = imagelist;
                    ShowMonthList.ItemsSource = monthlist;

                    LoadPanel.Visibility = Visibility.Collapsed;
                    noNetPath.Visibility = Visibility.Collapsed;
                    noNetText.Visibility = Visibility.Collapsed;
                    wifiPath.Visibility = Visibility.Collapsed;
                    wapPath.Visibility = Visibility.Collapsed;
                    loadText.Visibility = Visibility.Collapsed;

                    commandBar.Visibility = Visibility.Visible;
                });

            });
        }

        private bool CheckInternetIsConnect()
        {
            InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile == null)
            {
                noNetPath.Visibility = Visibility.Visible;
                noNetText.Visibility = Visibility.Visible;

                wifiPath.Visibility = Visibility.Collapsed;
                wapPath.Visibility = Visibility.Collapsed;
                loadText.Visibility = Visibility.Collapsed;
                return false;
            }

            if (InternetConnectionProfile.IsWlanConnectionProfile == false && InternetConnectionProfile.IsWwanConnectionProfile == false)
            {
                noNetPath.Visibility = Visibility.Visible;
                noNetText.Visibility = Visibility.Visible;

                wifiPath.Visibility = Visibility.Collapsed;
                wapPath.Visibility = Visibility.Collapsed;
                loadText.Visibility = Visibility.Collapsed;

                return false;
            }
            else if (InternetConnectionProfile.IsWlanConnectionProfile == true)
            {
                wifiPath.Visibility = Visibility.Visible;
                loadText.Visibility = Visibility.Visible;

                noNetPath.Visibility = Visibility.Collapsed;
                noNetText.Visibility = Visibility.Collapsed;
                wapPath.Visibility = Visibility.Collapsed;
            }
            else
            {
                wapPath.Visibility = Visibility.Visible;
                loadText.Visibility = Visibility.Visible;

                wifiPath.Visibility = Visibility.Collapsed;
                noNetPath.Visibility = Visibility.Collapsed;
                noNetText.Visibility = Visibility.Collapsed;
            }

            return true;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShowMonthList.SelectedItem == null)
                return;

            MonthsData data = ShowMonthList.SelectedItem as MonthsData;
            MonthToListParam param = new MonthToListParam()
            {
                YearIndex = data.YearIndex,
                MonthIndex = data.MonthIndex,
                AllVidellist = allvideolist
            };

            Frame.Navigate(typeof(VideoListPage), param);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (allvideolist == null)
                return;

            RadioButton btn = sender as RadioButton;
            CurrentYear = int.Parse(btn.Content.ToString());
            monthlist = GetMonthsDataList(allvideolist, CurrentYear);
            ShowMonthList.ItemsSource = monthlist;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoadPanel.Visibility == Visibility.Visible)
                return;

            FlushData();
        }

        private ObservableCollection<MonthsData> GetMonthsDataList(ObservableCollection<VideoShowInfo> videoList, int year)
        {
            List<MonthsData> tempList = new List<MonthsData>();
            foreach (VideoShowInfo video in videoList)
            {
                if (int.Parse(video.Sub_Index.Substring(0, 4)) == year)
                {
                    int monthIndex = int.Parse(video.Sub_Index.Substring(4, 2));
                    MonthsData monthsData = tempList.Where(data => data.MonthIndex == monthIndex).FirstOrDefault();
                    if (monthsData == null)
                    {
                        monthsData = new MonthsData()
                        {
                            YearIndex = year,
                            MonthIndex = monthIndex,
                            VideoCount = 1
                        };

                        tempList.Add(monthsData);
                    }
                    else
                    {
                        monthsData.VideoCount++;
                    }
                }
            }

            return new ObservableCollection<MonthsData>(tempList.OrderByDescending(data => data.MonthIndex));
        }

        private List<VideoNewShow> GetImageList(ObservableCollection<VideoShowInfo> videoList)
        {
            return ((from video in videoList.Take(4)
                     select new VideoNewShow
                     {
                         Vid = video.Vid,
                         Title = video.Title,
                         Img = video.Img,
                         Sub_Index = video.Sub_Index,
                         Flag = videoList.IndexOf(video) == 0 ? "最新" : video.Sub_Index.Substring(4, 2) + "." + video.Sub_Index.Substring(6, 2)
                     }).OrderByDescending(data => data.Sub_Index)).ToList();
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private async void ShowPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ShowPanel.CurrentVid) || ShowPanel.isBusy == true)
            {
                e.Handled = true;
                return;
            }

            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (InternetConnectionProfile==null)
            {
                tipText.Text = "当前无网络连接,无法播放!";
                MsgVisibleStoryboard.Begin();
                e.Handled = true;
                return;
            }
            else if (InternetConnectionProfile.IsWlanConnectionProfile == false && InternetConnectionProfile.IsWwanConnectionProfile == false)
            {
                tipText.Text = "当前无网络连接,无法播放!";
                MsgVisibleStoryboard.Begin();
                e.Handled = true;
                return;
            }

            commandBar.Visibility = Visibility.Collapsed;
            LoadMediaInfoPanel.Visibility = Visibility.Visible;

            ThreadPool.RunAsync(async (workItem) =>
            {
                VideoPlayInfo playInfo = await donet_sdk.getMobileVideoInfo(ShowPanel.CurrentVid);
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (playInfo == null)
                    {
                        tipText.Text = HashMap.MsgCodeMap[MsgCode.Cannot_PlayMedia];
                        MsgVisibleStoryboard.Begin();
                        LoadMediaInfoPanel.Visibility = Visibility.Collapsed;
                        commandBar.Visibility = Visibility.Visible;
                        return;
                    }

                    ListToPlayParam listToPlayParam = new ListToPlayParam
                    {
                        AllVidellist = allvideolist.ToList(),
                        PlayInfo = playInfo,
                        isfrommain = true
                    };

                    LoadMediaInfoPanel.Visibility = Visibility.Collapsed;
                    commandBar.Visibility = Visibility.Visible;

                    await Task.Delay(100);
                    Frame.Navigate(typeof(PlayVideoPage), listToPlayParam);
                });
            });
        }

        private void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            FlushData();
        }

        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingPage));
        }

        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(DownloadPage), 0);
        }
    }
}
