using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TryClient.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// “用户控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=234236 上提供

namespace TryClient
{
    public sealed partial class PictureView : UserControl
    {
        public bool isBusy = false;
        ObservableCollection<VideoNewShow> ShowUIList = new ObservableCollection<VideoNewShow>();
        public string CurrentVid { get; set; }
        public string CurrentTitle { get; set; }
        TranslateTransform tt;

        public PictureView()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty MarkSourceProperty =
           DependencyProperty.Register("MarkSource", typeof(List<VideoNewShow>), typeof(PictureView), new PropertyMetadata(null, new PropertyChangedCallback(MarkSourcePropertyChangedCallback)));
        public List<VideoNewShow> MarkSource
        {
            get { return (List<VideoNewShow>)GetValue(MarkSourceProperty); }
            set { SetValue(MarkSourceProperty, value); }
        }

        static void MarkSourcePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PictureView obj = sender as PictureView;
            if (obj.MarkSource.Count == 0)
                return;

            obj.ShowUIList.Clear();
            obj.ShowUIList.Add(obj.MarkSource[obj.MarkSource.Count - 1]);
            obj.ShowUIList.Add(obj.MarkSource[0]);
            obj.ShowUIList.Add(obj.MarkSource[1]);

            obj.stackPanel.ItemsSource = obj.ShowUIList;
            obj.myTranslateTransform.X = -obj.ActualWidth;
            obj.CurrentIndex = 0;
            obj.CurrentVid = obj.MarkSource[0].Vid;
            obj.CurrentTitle = obj.MarkSource[0].Title;

            obj.DisposeStoryboard();
            obj.InitStoryboard();
        }

        int CurrentIndex = 0;//(-1,0,1)
        int index = 0;//标识当前图片下标

        Storyboard sb = null;
        DoubleAnimation da = null;
        DispatcherTimer timer = null;

        private void InitStoryboard()
        {
            if (sb == null)
                sb = new Storyboard();
            if (da == null)
                da = new DoubleAnimation();
            if (timer == null)
                timer = new DispatcherTimer();

            da.Duration = TimeSpan.FromMilliseconds(300);
            Storyboard.SetTarget(da, stackPanel);
            Storyboard.SetTargetProperty(da, @"(UIElement.RenderTransform).(TranslateTransform.X)");
            sb.FillBehavior = FillBehavior.HoldEnd;
            //sb.RepeatBehavior = new RepeatBehavior(1);
            sb.Children.Add(da);
            sb.Completed += sb_Completed;

            timer.Interval = TimeSpan.FromMilliseconds(3000);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void DisposeStoryboard()
        {
            if (sb != null)
            {
                sb.Stop();
                sb = null;
            }

            if (da != null)
                da = null;

            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        void timer_Tick(object sender, object e)
        {
            isBusy = true;
            MoveToNext();
        }

        void sb_Completed(object sender, object e)
        {

            if (CurrentIndex == -1)//左滑到-1处（该时机处理“换位”）
            {
                VideoNewShow fe = stackPanel.Items[0] as VideoNewShow;
                index = MarkSource.IndexOf(fe);
                ShowUIList.RemoveAt(2);
                ShowUIList.Insert(0, MarkSource[GetInsertIndex(index, MarkSource.Count)]);
            }

            if (CurrentIndex == 1)//右滑到1处（该时机处理“换位”）
            {
                VideoNewShow fe = stackPanel.Items[2] as VideoNewShow;
                index = MarkSource.IndexOf(fe);
                ShowUIList.RemoveAt(0);
                ShowUIList.Add(MarkSource[GetAddIndex(MarkSource.IndexOf(fe), MarkSource.Count)]);
            }
            CurrentIndex = 0;
            CurrentVid = ShowUIList[1].Vid;
            CurrentTitle = ShowUIList[1].Title;
            tt = stackPanel.RenderTransform as TranslateTransform;
            tt.X = (-1 + CurrentIndex) * this.ActualWidth;
            listBox.SelectedIndex = index;
            isBusy = false;
            timer.Start(); 
        }  

        private int GetInsertIndex(int middleIndex, int listCount)
        {
            return middleIndex - 1 < 0 ? middleIndex - 1 + listCount : middleIndex - 1;
        }

        private int GetAddIndex(int middleIndex, int listCount)
        {
            return middleIndex + 1 >= listCount ? middleIndex + 1 - listCount : middleIndex + 1;
        }

        //滑动开始
        private void stackPanel_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (isBusy)
            {
                e.Complete();
                e.Handled = true;
                return;
            }

            timer.Stop();
            Debug.WriteLine("{0}-ManipulationStarted事件发生", DateTime.Now.ToString("H:m:s"));
        }

        //滑动过程事件
        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (isBusy || e.IsInertial)
            {
                e.Complete();
                e.Handled = true;
                return;
            }
            Debug.WriteLine("{0}-ManipulationDelta事件发生", DateTime.Now.ToString("H:m:s"));

            tt = stackPanel.RenderTransform as TranslateTransform;
            tt.X += e.Delta.Translation.X;
        }

        //滑动结束
        private void Canvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (isBusy)
            {
                e.Handled = true;
                return;
            }
            Debug.WriteLine("{0}-ManipulationCompleted事件发生", DateTime.Now.ToString("H:m:s"));

            //右滑
            if (e.Cumulative.Translation.X > 0)
            {
                isBusy = true;
                if (!e.IsInertial && Math.Abs(e.Cumulative.Translation.X) < this.ActualWidth / 3)
                {
                    MoveToCurrent();
                }
                else
                {
                    MoveToPre();
                }
            }
            else if (e.Cumulative.Translation.X < 0)//左滑
            {
                isBusy = true;
                if (!e.IsInertial && Math.Abs(e.Cumulative.Translation.X) < this.ActualWidth / 3)
                {
                    MoveToCurrent();
                }
                else
                {
                    MoveToNext();
                }
            }
        }

        private void MoveToPre()
        {
            CurrentIndex--;

            da.To = 0;
            sb.Begin();
        }

        private void MoveToCurrent()
        {
            da.To = (-1) * this.ActualWidth;
            sb.Begin();
        }

        private void MoveToNext()
        {
            CurrentIndex++;

            da.To = (-2) * this.ActualWidth;
            sb.Begin();
        } 
    }
}
