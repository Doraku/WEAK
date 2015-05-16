using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace WEAK.Windows.Data
{
    public sealed class ContainsConverter : IMultiValueConverter
    {
        #region Methods

        public static bool Convert(IList source, object item)
        {
            return source != null && source.Contains(item);
        }

        #endregion

        #region IMultiValueConverter

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Length == 2 && Convert(values[0] as IList, values[1]);
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
