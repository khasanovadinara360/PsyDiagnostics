using System;
using System.Globalization;
using System.Windows.Data;

namespace PsyDiagnostics.Helpers
{
    public class PredictionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            int v = (int)value;

            return v == 1 ? "Высокий риск" : "Низкий риск";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}