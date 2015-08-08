using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TryClient.Helps;
using TryClient.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “用户控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=234236 上提供

namespace TryClient.UserControls
{
    public sealed partial class MediaPlayControl : UserControl
    {
        //阻止屏幕变暗
        DisplayRequest dispRequest = null;
        //故事版动画进行中
        bool IsStoryboaredBusy = false;
        //是否切换了清晰度
        bool IsResolutionChanged = false;

        //用于更新进度
        DispatcherTimer timer = null;

        //用于切换当前视频清晰度后，记录当前播放位置
        TimeSpan timeSpan = TimeSpan.FromSeconds(0);

        public MediaPlayControl()
        {
            this.InitializeComponent();
            this.DataContext = this;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, object e)
        {
            SetCurrentPlayInfo();
        }

        public event EventHandler PlayNextMedia;
        public event EventHandler PlayLastMedia;
        public event EventHandler MediaEnded;
        public event EventHandler<ResolutionEventArgs> ResolutionChanged;
        public event EventHandler<MediaStateEventArgs> MediaStateChanged;


        public static readonly DependencyProperty IsInternetFileProperty = DependencyProperty.Register("IsInternetFile", typeof(bool), typeof(MediaPlayControl), new PropertyMetadata(false, new PropertyChangedCallback(IsInternetFilePropertyChangedEvent)));
        public bool IsInternetFile
        {
            get { return (bool)GetValue(IsInternetFileProperty); }
            set { SetValue(IsInternetFileProperty, value); }
        }
        static void IsInternetFilePropertyChangedEvent(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MediaPlayControl obj = sender as MediaPlayControl;
            if(obj.IsInternetFile==true)
            {
                obj.resolutionBtn.SetBinding(Button.IsEnabledProperty, new Binding { Source = obj, Path = new PropertyPath("IsSupportHD")});
            }  
        }

        public static readonly DependencyProperty IsSupportHDProperty = DependencyProperty.Register("IsSupportHD", typeof(bool), typeof(MediaPlayControl), new PropertyMetadata(true, new PropertyChangedCallback(IsSupportHDPropertyChangedEvent)));
        public bool IsSupportHD
        {
            get { return (bool)GetValue(IsSupportHDProperty); }
            set { SetValue(IsSupportHDProperty, value); }
        }
        static void IsSupportHDPropertyChangedEvent(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MediaPlayControl obj = sender as MediaPlayControl;
        }

        public static readonly DependencyProperty MediaTitleProperty = DependencyProperty.Register("MediaTitle", typeof(string), typeof(MediaPlayControl), new PropertyMetadata("第356期 我们的世界"));
        public string MediaTitle
        {
            get { return (string)GetValue(MediaTitleProperty); }
            set { SetValue(MediaTitleProperty, value); }
        }

        public static readonly DependencyProperty ResolutionStrProperty = DependencyProperty.Register("ResolutionStr", typeof(string), typeof(MediaPlayControl), new PropertyMetadata("高清"));
        public string ResolutionStr
        {
            get { return (string)GetValue(ResolutionStrProperty); }
            set { SetValue(ResolutionStrProperty, value); }
        }

        public static readonly DependencyProperty ResolutionIndexProperty = DependencyProperty.Register("ResolutionIndex", typeof(int), typeof(MediaPlayControl), new PropertyMetadata(3, new PropertyChangedCallback(ResolutionIndexChangedEvent)));
        public int ResolutionIndex
        {
            get { return (int)GetValue(ResolutionIndexProperty); }
            set { SetValue(ResolutionIndexProperty, value); }
        }
        static void ResolutionIndexChangedEvent(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MediaPlayControl obj = sender as MediaPlayControl;
            if ((int)(e.NewValue) == 2)
            {
                obj.ResolutionStr = "标清";
                obj.ResolutionList.SelectedIndex = 1;
            }
            else
            {
                obj.ResolutionStr = "高清";
                obj.ResolutionList.SelectedIndex = 0;
            }
        }

        public static readonly DependencyProperty MediaSourceProperty = DependencyProperty.Register("MediaSource", typeof(Uri), typeof(MediaPlayControl), new PropertyMetadata(default(Uri), new PropertyChangedCallback(MediaSourcePropertyChangedEvent)));
        public Uri MediaSource
        {
            get { return (Uri)GetValue(MediaSourceProperty); }
            set { SetValue(MediaSourceProperty, value); }
        }
        static void MediaSourcePropertyChangedEvent(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MediaPlayControl obj = sender as MediaPlayControl;
            obj.ResetMediaShowInfo();  
        }

        public static readonly DependencyProperty MediaStorageFileProperty = DependencyProperty.Register("MediaStorageFile", typeof(StorageFile), typeof(MediaPlayControl), new PropertyMetadata(null, new PropertyChangedCallback(MediaStorageFilePropertyChangedEvent)));
        public StorageFile MediaStorageFile
        {
            get { return (StorageFile)GetValue(MediaStorageFileProperty); }
            set { SetValue(MediaStorageFileProperty, value); }
        }
        static async void MediaStorageFilePropertyChangedEvent(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MediaPlayControl obj = sender as MediaPlayControl;
            var stream = await obj.MediaStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);

            if (null != obj.MediaStorageFile)
            {
                obj.mediaElement.SetSource(stream, obj.MediaStorageFile.ContentType);
            }
        }

        public static readonly DependencyProperty IsDispRequestProperty = DependencyProperty.Register("IsDispRequest", typeof(bool), typeof(MediaPlayControl), new PropertyMetadata(null, new PropertyChangedCallback(IsDispRequestPropertyChangedEvent)));
        public bool IsDispRequest
        {
            get { return (bool)GetValue(IsDispRequestProperty); }
            set { SetValue(IsDispRequestProperty, value); }
        }
        static void IsDispRequestPropertyChangedEvent(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MediaPlayControl obj = sender as MediaPlayControl;
            if ((bool)(e.NewValue))
            {
                if (obj.dispRequest == null)
                {
                    obj.dispRequest = new DisplayRequest();
                }
                obj.dispRequest.RequestActive();
            }
            else
            {
                if (obj.dispRequest != null)
                    obj.dispRequest.RequestRelease();
            }
        }

        //更新时间显示与进度显示
        private void SetCurrentPlayInfo()
        {
            if (mediaElement.NaturalDuration.TimeSpan.Seconds == 0)
                return;

            CurrentTime.Text = mediaElement.Position.ToString(@"mm\:ss");
            PlayProgress.Value = mediaElement.Position.TotalSeconds * 100 / mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void PlayControlVisibleStoryboard_Completed(object sender, object e)
        {
            IsStoryboaredBusy = false;
        }

        private void PlayControlCollapsedStoryboard_Completed(object sender, object e)
        {
            IsStoryboaredBusy = false;
        }

        //显示或隐藏控制条
        private void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsStoryboaredBusy == true || mediaElement.CurrentState == MediaElementState.Paused)
                return;

            IsStoryboaredBusy = true;

            if (PlayBtn.Visibility == Visibility.Collapsed)
            {
                PlayControlVisibleStoryboard.Begin();
            }
            else
            {
                PlayControlCollapsedStoryboard.Begin();
            }
        }

        //开始播放
        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            IsDispRequest = true;

            BufferingText.Visibility = Visibility.Collapsed;
            PlayProgress.IsEnabled = true;

            CurrentTime.Text = timeSpan.ToString(@"mm\:ss");
            TotalTime.Text = mediaElement.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            if (timeSpan != TimeSpan.FromSeconds(0))
            {
                if (mediaElement.CanSeek)
                {
                    mediaElement.Position = timeSpan;
                    PlayProgress.Value = timeSpan.TotalSeconds * 100 / mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                }
            }
        }

        //状态变换
        private void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (mediaElement.CurrentState)
            {
                case MediaElementState.Buffering:
                    BufferingText.Visibility = Visibility.Collapsed;
                    if (timer != null)
                        timer.Stop();
                    break;
                case MediaElementState.Closed:
                    if (timer != null)
                        timer.Stop();
                    break;
                case MediaElementState.Opening:
                    if (timer != null)
                        timer.Start();
                    break;
                case MediaElementState.Paused:
                    IsDispRequest = false; 
                    if (timer != null)
                        timer.Stop();
                    break;
                case MediaElementState.Playing:
                    IsDispRequest = true; 
                    if (timer != null)
                        timer.Start();
                    break;
                case MediaElementState.Stopped:
                    if (timer != null)
                        timer.Stop();
                    break;
                default:
                    break;
            }

            if (MediaStateChanged != null)
                MediaStateChanged(this, new MediaStateEventArgs { state = mediaElement.CurrentState });
        }

        //结束播放
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            IsDispRequest = false; 

            if (MediaEnded != null)
                MediaEnded(this, new EventArgs());
        }

        //清晰度按钮
        private void resolutionBtn_Click(object sender, RoutedEventArgs e)
        {
            GeneralTransform transform = base.TransformToVisual(resolutionBtn);
            Windows.Foundation.Point tPoint = transform.TransformPoint(new Windows.Foundation.Point(0.0, 0.0));

            ResolutionSelection.VerticalOffset = Math.Abs(tPoint.Y) + resolutionBtn.ActualHeight + 2;
            ResolutionSelection.HorizontalOffset = Math.Abs(tPoint.X);

            ResolutionSelection.IsOpen = true;
            ResolutionSelection.UpdateLayout();
        }

        //清晰度切换
        private void ResolutionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox.SelectedItem == null || resolutionBtn == null || mediaElement == null || ResolutionSelection == null)
                return;

            switch ((listBox.SelectedItem as ListBoxItem).Content.ToString())
            {
                case "高清":
                    timeSpan = mediaElement.Position > TimeSpan.FromSeconds(0) ? mediaElement.Position : TimeSpan.FromSeconds(0);
                    ResolutionIndex = 3;
                    break;

                case "标清":
                    timeSpan = mediaElement.Position > TimeSpan.FromSeconds(0) ? mediaElement.Position : TimeSpan.FromSeconds(0);
                    ResolutionIndex = 2;
                    break;
            }

            ResolutionSelection.IsOpen = false;
            IsResolutionChanged = true;

            if (ResolutionChanged != null)
                ResolutionChanged(this, new ResolutionEventArgs() { ResolutionIndex = ResolutionIndex });
        }

        //播放按钮
        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.CurrentState == MediaElementState.Playing)
            {
                if (mediaElement.CanPause)
                    mediaElement.Pause();
                else
                    PlayBtn.IsChecked = false;
            }
            else
            {
                mediaElement.Play();
                PlayBtn.IsChecked = false;
            }
        }

        //进度变化
        private void PlayProgress_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (mediaElement.CanSeek)
            {
                mediaElement.Position = TimeSpan.FromSeconds(mediaElement.NaturalDuration.TimeSpan.TotalSeconds * PlayProgress.Value / 100);
                CurrentTime.Text = mediaElement.Position.ToString(@"mm\:ss");
                if (mediaElement.CurrentState == MediaElementState.Paused)
                {
                    PlayProgress.Value = mediaElement.Position.TotalSeconds * 100 / mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                }
            }
        }

        //上一期
        private void LastVideo_Click(object sender, RoutedEventArgs e)
        { 
            if (PlayLastMedia != null)
                PlayLastMedia(this, new EventArgs());
        }

        //下一期
        private void NextVideo_Click(object sender, RoutedEventArgs e)
        { 
            if (PlayNextMedia != null)
                PlayNextMedia(this, new EventArgs());
        }

        //快退
        private void Backward_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Position >= TimeSpan.FromSeconds(5) && mediaElement.CanSeek)
            {
                mediaElement.Position = mediaElement.Position - TimeSpan.FromSeconds(5);
                if (mediaElement.CurrentState==MediaElementState.Paused)
                {
                    CurrentTime.Text = mediaElement.Position.ToString(@"mm\:ss");
                    PlayProgress.Value = mediaElement.Position.TotalSeconds * 100 / mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                }
            }
        }

        //快进
        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.TimeSpan - mediaElement.Position >= TimeSpan.FromSeconds(5) && mediaElement.CanSeek)
            {
                mediaElement.Position = mediaElement.Position + TimeSpan.FromSeconds(5);
                if (mediaElement.CurrentState == MediaElementState.Paused)
                {
                    CurrentTime.Text = mediaElement.Position.ToString(@"mm\:ss");
                    PlayProgress.Value = mediaElement.Position.TotalSeconds * 100 / mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                }
            }
        }

        private void ResetMediaShowInfo()
        {
            if (timer != null)
                timer.Stop();

            //与清晰度切换无关 只要播放源变化 则显示 开始播放则隐藏
            if (IsInternetFile)
            {
                BufferingText.Visibility = Visibility.Visible;
            }

            PlayBtn.IsChecked = false;
            PlayProgress.IsEnabled = false;

            //切换清晰度时，保持当前信息显示即可
            if (IsResolutionChanged == false)
            { 
                mediaElement.Position = TimeSpan.FromSeconds(0);
                PlayProgress.Value = 0;
                timeSpan = TimeSpan.FromSeconds(0);
                CurrentTime.Text = timeSpan.ToString(@"mm\:ss");
                TotalTime.Text = timeSpan.ToString(@"mm\:ss");
            }
            else
            {
                IsResolutionChanged = false;
            }
        }

    }
}
