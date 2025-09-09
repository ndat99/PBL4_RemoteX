using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RemoteX.Client.Views
{
    public class BoolToAlignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMine)
            {
                return isMine ? System.Windows.HorizontalAlignment.Right : System.Windows.HorizontalAlignment.Left;
            }
            return System.Windows.HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMine)
            {
                return isMine
                    ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 215))  // Xanh dương
                    : new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100)); // Xám
            }
            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}