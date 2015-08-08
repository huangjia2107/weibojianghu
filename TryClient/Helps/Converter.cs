using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TryClient.Helps
{
    public class MonthIndexToStrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            int monthIndex = (int)value;

            return HashMap.MonthMap[monthIndex];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            bool IsTrue = (bool)value;
            if (IsTrue)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToNoVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            bool IsTrue = (bool)value;
            if (IsTrue)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PlayTimesFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            double times = Int64.Parse(value.ToString().Replace(",", ""));

            if (times > 100000000)
                return string.Format("{0:#.##}亿", times / 100000000);
            else if (times > 10000)
                return string.Format("{0:#.#}万", times / 10000);
            else
                return times;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BufferingProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            double progress = (double)value;
            return string.Format("{0:#}%",progress*100 );
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            MediaElementState state = (MediaElementState)value;
            if (state == MediaElementState.Buffering)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    } 

    public class VisibilityToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            Visibility visibility = (Visibility)value;
            if (visibility == Visibility.Visible)
                return 0;
            else
                return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            Visibility visibility = (Visibility)value;
            if (visibility == Visibility.Visible)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ResolutionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            int ResolutionValue = (int)value;  
            return "["+HashMap.ResolutionMap[ResolutionValue]+"]";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DownloadStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            DownloadStatus downloadStatus = (DownloadStatus)value;
            return HashMap.DownStatusMap[downloadStatus];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            double percent= (double)value;
            return percent+"%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    } 
}
