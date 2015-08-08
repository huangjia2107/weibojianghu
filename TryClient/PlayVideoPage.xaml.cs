using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TryClient.Helps;
using TryClient.Models;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.System.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TryClient
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayVideoPage : Page
    {
        ListToPlayParam listToPlayParam = null;
        List<VideoShowInfo> AllVidellist { get; set; }
        VideoPlayInfo CurMediaInfo { get; set; }
        int CurMediaIndex { get; set; }

        CoreDispatcher dispatcher = null;
        AppSettings appSettings = null;
        int CurrentResolutionIndex = 3;

        public PlayVideoPage()
        {
            this.InitializeComponent();
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            appSettings = new AppSettings();
            CurrentResolutionIndex = appSettings.PlayResolution == true ? 3 : 2;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            listToPlayParam = (ListToPlayParam)e.Parameter;
            AllVidellist = listToPlayParam.AllVidellist;
            CurMediaInfo = listToPlayParam.PlayInfo;
            CurMediaIndex = listToPlayParam.AllVidellist.FindIndex(info => info.Vid == listToPlayParam.PlayInfo.vid);

            if (DisplayInformation.GetForCurrentView().CurrentOrientation != DisplayOrientations.Landscape)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            StartLoadVideo(listToPlayParam.PlayInfo);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;

            if (DisplayInformation.GetForCurrentView().CurrentOrientation != DisplayOrientations.Portrait)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (listToPlayParam.isfrommain == true)
                Frame.Navigate(typeof(MainPage));
            else
                Frame.Navigate(typeof(VideoListPage), listToPlayParam.param);

            e.Handled = true;
        }

        private void StartLoadVideo(VideoPlayInfo PlayInfo)
        {
            CurMediaIndex = listToPlayParam.AllVidellist.FindIndex(info => info.Vid == PlayInfo.vid);
            CurMediaInfo = PlayInfo;
            mediaPlayControl.MediaTitle = PlayInfo.Subject;

            //不支持高清
            if (PlayInfo.rfiles[3].filesize == null)
            {
                mediaPlayControl.IsSupportHD = false;

                movieparam param = PlayInfo.rfiles[2];
                mediaPlayControl.ResolutionIndex = 2;
                mediaPlayControl.MediaSource = new Uri(param.url);
            }
            else
            {
                mediaPlayControl.IsSupportHD = true;
                movieparam param = PlayInfo.rfiles[CurrentResolutionIndex];
                mediaPlayControl.ResolutionIndex = CurrentResolutionIndex;
                mediaPlayControl.MediaSource = new Uri(param.url);
            }
        }

        private void NewVisibleStoryboard_Completed(object sender, object e)
        {
            if (tipText.Text == HashMap.MsgCodeMap[MsgCode.NewestMedia_Completed] || tipText.Text == HashMap.MsgCodeMap[MsgCode.FirstMedia_Completed])
            {
                if (listToPlayParam.isfrommain == true)
                    Frame.Navigate(typeof(MainPage));
                else
                    Frame.Navigate(typeof(VideoListPage), listToPlayParam.param);
            }
        }

        //当前期播放结束
        private void mediaPlayControl_MediaEnded(object sender, EventArgs e)
        {
            PlayCurVideoCompleted(CurMediaIndex, appSettings.MediaPlayOrder);
        }

        //IsDesc=true 当前->最新期  (降序 大-->小)
        private void PlayCurVideoCompleted(int MediaIndex, bool IsDesc)
        {
            //当前-->第一期
            if (IsDesc == false)
            {
                if (MediaIndex == AllVidellist.Count - 1)
                {
                    tipText.Text = HashMap.MsgCodeMap[MsgCode.FirstMedia_Completed];
                    NewVisibleStoryboard.Begin();
                }
                else if (MediaIndex < AllVidellist.Count - 1 && MediaIndex >= 0)
                {
                    ThreadPool.RunAsync(async (workItem) =>
                    {
                        VideoPlayInfo playInfo = await donet_sdk.getMobileVideoInfo(AllVidellist[MediaIndex + 1].Vid);

                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (playInfo == null)
                            {
                                tipText.Text = HashMap.MsgCodeMap[MsgCode.Cannot_PlayMedia];
                                NewVisibleStoryboard.Begin();
                                PlayLastVideo(++MediaIndex);
                                return;
                            }
                            StartLoadVideo(playInfo);
                        });
                    });
                }
            }
            else//当前-->最新期
            {
                if (MediaIndex == 0)
                {
                    tipText.Text = HashMap.MsgCodeMap[MsgCode.NewestMedia_Completed];
                    NewVisibleStoryboard.Begin();
                }
                else if (MediaIndex > 0)
                {
                    ThreadPool.RunAsync(async (workItem) =>
                    {
                        VideoPlayInfo playInfo = await donet_sdk.getMobileVideoInfo(AllVidellist[MediaIndex - 1].Vid);

                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (playInfo == null)
                            {
                                tipText.Text = HashMap.MsgCodeMap[MsgCode.Cannot_PlayMedia];
                                NewVisibleStoryboard.Begin();
                                PlayNextVideo(--MediaIndex); //继续尝试下一期进行播放
                                return;
                            }
                            StartLoadVideo(playInfo);
                        });
                    });
                }
            }

        }

        //降序  当前-->最新
        private void PlayNextVideo(int MediaIndex)
        {
            if (MediaIndex == 0)
            {
                tipText.Text = HashMap.MsgCodeMap[MsgCode.CurMedia_Newest];
                NewVisibleStoryboard.Begin();
            }
            else if (MediaIndex > 0)
            {
                ThreadPool.RunAsync(async (workItem) =>
                {
                    VideoPlayInfo playInfo = await donet_sdk.getMobileVideoInfo(AllVidellist[MediaIndex - 1].Vid);

                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (playInfo == null)
                        {
                            tipText.Text = HashMap.MsgCodeMap[MsgCode.Cannot_PlayMedia];
                            NewVisibleStoryboard.Begin();
                            if (appSettings.MediaPlayOrder == false)//升序
                                PlayLastVideo(--MediaIndex); //继续尝试上一期进行播放
                            else
                                PlayNextVideo(--MediaIndex); //继续尝试下一期进行播放
                            return;
                        }
                        StartLoadVideo(playInfo);
                    });
                });
            }
        }

        //升序 当前-->第一期
        private void PlayLastVideo(int MediaIndex)
        {
            if (MediaIndex == AllVidellist.Count - 1)
            {
                tipText.Text = HashMap.MsgCodeMap[MsgCode.CurMedia_First];
                NewVisibleStoryboard.Begin();
            }
            else if (MediaIndex < AllVidellist.Count - 1 && MediaIndex >= 0)
            {
                ThreadPool.RunAsync(async (workItem) =>
                {
                    VideoPlayInfo playInfo = await donet_sdk.getMobileVideoInfo(AllVidellist[MediaIndex + 1].Vid);

                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (playInfo == null)
                        {
                            tipText.Text = HashMap.MsgCodeMap[MsgCode.Cannot_PlayMedia];
                            NewVisibleStoryboard.Begin();
                            if (appSettings.MediaPlayOrder == false)//升序
                                PlayLastVideo(++MediaIndex); //继续尝试上一期进行播放
                            else
                                PlayNextVideo(++MediaIndex); //继续尝试下一期进行播放
                            return;
                        }
                        StartLoadVideo(playInfo);
                    });
                });
            }
        }

        //上一期
        private void mediaPlayControl_PlayLastMedia(object sender, EventArgs e)
        {
            if (appSettings.MediaPlayOrder == true)//当前-->最新期
                PlayLastVideo(CurMediaIndex);  //++
            else
                PlayNextVideo(CurMediaIndex);  //--
        }

        //下一期
        private void mediaPlayControl_PlayNextMedia(object sender, EventArgs e)
        {
            if (appSettings.MediaPlayOrder == true)//当前-->最新期（--）
                PlayNextVideo(CurMediaIndex);   //--
            else
                PlayLastVideo(CurMediaIndex);    //++
        }

        //切换清晰度
        private void mediaPlayControl_ResolutionChanged(object sender, ResolutionEventArgs e)
        {
            if (mediaPlayControl.IsSupportHD == true)
            {
                CurrentResolutionIndex = e.ResolutionIndex;
                appSettings.PlayResolution = e.ResolutionIndex == 3 ? true : false;
            }
            mediaPlayControl.MediaSource = new Uri(CurMediaInfo.rfiles[e.ResolutionIndex].url);
        }
    }
}
