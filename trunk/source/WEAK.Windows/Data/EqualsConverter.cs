using System;
using System.Globalization;
using System.Windows.Data;

namespace WEAK.Windows.Data
{
    public sealed class EqualsConverter : IMultiValueConverter
    {
        #region IMultiValueConverter

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Length == 2 && Equals(values[0], values[1]);
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
