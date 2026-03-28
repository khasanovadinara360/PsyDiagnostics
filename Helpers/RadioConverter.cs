using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace PsyDiagnostics.Helpers
{
    public class RadioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var rb = parameter as RadioButton;
            if (rb == null)
                return false;

            return value.ToString() == rb.Tag?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rb = parameter as RadioButton;
            if (rb != null && (bool)value)
                return rb.Tag;

            return Binding.DoNothing;
        }
    }
}