using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace WEAK.Windows.AttachedProperty
{
    static public class TextBoxBehavior
    {
        #region Internal types

        public enum RegexType
        {
            None,
            Binary,
            Hexadecimal,
            Integer,
            Double
        }

        #endregion

        #region Pattern property

        /// <summary>
        /// DepedencyProperty of the Pattern property
        /// </summary>
        static public readonly DependencyProperty PatternProperty = DependencyProperty.RegisterAttached("Pattern", typeof(string), typeof(TextBoxBehavior), new PropertyMetadata(null, OnPatternChanged));

        /// <summary>
        /// Set the value of the Pattern property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to set the value</param>
        /// <param name="value">Value of the property</param>
        static public void SetPattern(DependencyObject source, string value)
        {
            source.SetValue(PatternProperty, value);
        }

        /// <summary>
        /// Get the value of the Pattern property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to get the value</param>
        /// <returns>Value of the property</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        static public string GetPattern(DependencyObject source)
        {
            return source.GetValue(PatternProperty) as string;
        }

        /// <summary>
        /// Callback when the Pattern property is changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        static public void OnPatternChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source is TextBox))
            {
                throw new ArgumentException("Source is not a TextBox");
            }

            if (GetRegexType(source) != RegexType.None)
            {
                throw new Exception("A RegexType is already defined, you can't use a custom pattern");
            }

            SetNumberValue(source, null);
            SetNumberMinValue(source, null);
            SetNumberMaxValue(source, null);
            TextBox control = source as TextBox;
            if (string.IsNullOrEmpty(e.NewValue as string))
            {
                control.TextChanged -= OnTextChanged;
            }
            else
            {
                control.TextChanged += OnTextChanged;
                if (!ValidatePattern(control, control.Text))
                {
                    control.Text = string.Empty;
                }
            }
        }

        #endregion

        #region RegexType property

        /// <summary>
        /// DepedencyProperty of the RegexType property
        /// </summary>
        static public readonly DependencyProperty RegexTypeProperty = DependencyProperty.RegisterAttached("RegexType", typeof(RegexType), typeof(TextBoxBehavior), new PropertyMetadata(RegexType.None, OnRegexTypeChanged));

        /// <summary>
        /// Set the value of the RegexType property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to set the value</param>
        /// <param name="value">Value of the property</param>
        static public void SetRegexType(DependencyObject source, RegexType value)
        {
            source.SetValue(RegexTypeProperty, value);
        }

        /// <summary>
        /// Get the value of the RegexType property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to get the value</param>
        /// <returns>Value of the property</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        static public RegexType GetRegexType(DependencyObject source)
        {
            return (RegexType)source.GetValue(RegexTypeProperty);
        }

        /// <summary>
        /// Callback when the RegexType property is changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        static public void OnRegexTypeChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source is TextBox))
            {
                throw new ArgumentException("Source is not a TextBox");
            }

            if (!string.IsNullOrEmpty(GetPattern(source)))
            {
                throw new Exception("A pattern is already defined, you can't use a RegexType");
            }

            RegexType regexType = (RegexType)e.NewValue;
            if (GetIsByteData(source) && (regexType != RegexType.Binary || regexType != RegexType.Hexadecimal))
            {
                throw new Exception("This RegexType is not compatible with the byte data mod");
            }

            TextBox control = source as TextBox;
            if (regexType == RegexType.None)
            {
                control.TextChanged -= OnTextChanged;
            }
            else
            {
                control.TextChanged += OnTextChanged;
                if (!control.ValidatePattern(control.Text))
                {
                    control.Text = string.Empty;
                }
            }
        }

        #endregion

        #region NumberValue property

        /// <summary>
        /// DepedencyProperty of the NumberValue property
        /// </summary>
        static public readonly DependencyProperty NumberValueProperty = DependencyProperty.RegisterAttached("NumberValue", typeof(double?), typeof(TextBoxBehavior), new PropertyMetadata(null, OnNumberValueChanged));

        /// <summary>
        /// Set the value of the NumberValue property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to set the value</param>
        /// <param name="value">Value of the property</param>
        static public void SetNumberValue(DependencyObject source, double? value)
        {
            source.SetValue(NumberValueProperty, value);
        }

        /// <summary>
        /// Get the value of the NumberValue property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to get the value</param>
        /// <returns>Value of the property</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        static public double? GetNumberValue(DependencyObject source)
        {
            return source.GetValue(NumberValueProperty) as double?;
        }

        /// <summary>
        /// Callback when the NumberValue property is changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        static public void OnNumberValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source is TextBox))
            {
                throw new ArgumentException("Source is not a TextBox");
            }

            if (!source.BeginUpdate())
            {
                return;
            }

            TextBox control = source as TextBox;
            if (!(e.NewValue as double?).HasValue)
            {
                control.Text = string.Empty;
            }
            else
            {
                int numberBase = control.GetNumberBase();

                double value = (e.NewValue as double?).Value;
                if (value != control.BoundNumber(value))
                {
                    value = control.BoundNumber(value);
                    SetNumberValue(control, control.BoundNumber(value));
                }

                string text = numberBase == 10 ? Convert.ToString(value) : Convert.ToString((int)value, numberBase);
                if (control.ValidatePattern(text))
                {
                    control.Text = text;
                }
                else
                {
                    SetNumberValue(control, null);
                }
            }

            source.EndUpdate();
        }

        #endregion

        #region NumberMinMaxValue property

        /// <summary>
        /// DepedencyProperty of the NumberMinValue property
        /// </summary>
        static public readonly DependencyProperty NumberMinValueProperty = DependencyProperty.RegisterAttached("NumberMinValue", typeof(double?), typeof(TextBoxBehavior), new PropertyMetadata(null, OnNumberMinMaxValueChanged));

        /// <summary>
        /// Set the value of the NumberMinValue property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to set the value</param>
        /// <param name="value">Value of the property</param>
        static public void SetNumberMinValue(DependencyObject source, double? value)
        {
            source.SetValue(NumberMinValueProperty, value);
        }

        /// <summary>
        /// Get the value of the NumberMinValue property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to get the value</param>
        /// <returns>Value of the property</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        static public double? GetNumberMinValue(DependencyObject source)
        {
            return source.GetValue(NumberMinValueProperty) as double?;
        }

        /// <summary>
        /// DepedencyProperty of the NumberMaxValue property
        /// </summary>
        static public readonly DependencyProperty NumberMaxValueProperty = DependencyProperty.RegisterAttached("NumberMaxValue", typeof(double?), typeof(TextBoxBehavior), new PropertyMetadata(null, OnNumberMinMaxValueChanged));

        /// <summary>
        /// Set the value of the NumberMaxValue property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to set the value</param>
        /// <param name="value">Value of the property</param>
        static public void SetNumberMaxValue(DependencyObject source, double? value)
        {
            source.SetValue(NumberMaxValueProperty, value);
        }

        /// <summary>
        /// Get the value of the NumberMaxValue property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to get the value</param>
        /// <returns>Value of the property</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        static public double? GetNumberMaxValue(DependencyObject source)
        {
            return source.GetValue(NumberMaxValueProperty) as double?;
        }

        /// <summary>
        /// Callback when the NumberMinMaxValue property is changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        static public void OnNumberMinMaxValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source is TextBox))
            {
                throw new ArgumentException("Source is not a TextBox");
            }

            if (GetNumberMinValue(source).HasValue
                && GetNumberMaxValue(source).HasValue
                && GetNumberMinValue(source).Value > GetNumberMaxValue(source).Value)
            {
                throw new ArgumentException("MinValue is superior to MaxValue");
            }

            if (GetNumberValue(source).HasValue)
            {
                SetNumberValue(source, source.BoundNumber(GetNumberValue(source).Value));
            }
        }

        #endregion

        #region IsByteData property

        /// <summary>
        /// DepedencyProperty of the IsByteData property
        /// </summary>
        static public readonly DependencyProperty IsByteDataProperty = DependencyProperty.RegisterAttached("IsByteData", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false, OnIsByteDataChanged));

        /// <summary>
        /// Set the value of the IsByteData property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to set the value</param>
        /// <param name="value">Value of the property</param>
        static public void SetIsByteData(DependencyObject source, bool value)
        {
            source.SetValue(IsByteDataProperty, value);
        }

        /// <summary>
        /// Get the value of the IsByteData property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to get the value</param>
        /// <returns>Value of the property</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        static public bool GetIsByteData(DependencyObject source)
        {
            return (bool)source.GetValue(IsByteDataProperty);
        }

        /// <summary>
        /// Callback when the IsByteData property is changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        static public void OnIsByteDataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source is TextBox))
            {
                throw new ArgumentException("Source is not a TextBox");
            }

            if (GetRegexType(source) != RegexType.Binary
                && GetRegexType(source) != RegexType.Hexadecimal)
            {
                throw new ArgumentException("Byte data mod is only avaiable with RegexType Binary and Hexadecimal");
            }

            if (!source.BeginUpdate())
            {
                return;
            }

            TextBox control = source as TextBox;
            if ((bool)e.NewValue)
            {
                control.Text = ProcessByteArray(GetByteData(control), GetByteSpacer(control), control.GetNumberBase());
            }
            else
            {
                SetByteData(control, null);
                control.Text = null;
            }

            source.EndUpdate();
        }

        #endregion

        #region ByteSpacer property

        /// <summary>
        /// DepedencyProperty of the ByteSpacer property
        /// </summary>
        static public readonly DependencyProperty ByteSpacerProperty = DependencyProperty.RegisterAttached("ByteSpacer", typeof(string), typeof(TextBoxBehavior), new PropertyMetadata(" ", OnByteSpacerChanged));

        /// <summary>
        /// Set the value of the ByteSpacer property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to set the value</param>
        /// <param name="value">Value of the property</param>
        static public void SetByteSpacer(DependencyObject source, string value)
        {
            source.SetValue(ByteSpacerProperty, value);
        }

        /// <summary>
        /// Get the value of the ByteSpacer property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to get the value</param>
        /// <returns>Value of the property</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        static public string GetByteSpacer(DependencyObject source)
        {
            return source.GetValue(ByteSpacerProperty) as string;
        }

        /// <summary>
        /// Callback when the ByteSpacer property is changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        static public void OnByteSpacerChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source is TextBox))
            {
                throw new ArgumentException("Source is not a TextBox");
            }

            if (!source.BeginUpdate())
            {
                return;
            }

            TextBox control = source as TextBox;
            control.Text = ProcessByteArray(e.NewValue as byte[], GetByteSpacer(control), control.GetNumberBase());

            source.EndUpdate();
        }

        #endregion

        #region ByteData property

        /// <summary>
        /// DepedencyProperty of the ByteData property
        /// </summary>
        static public readonly DependencyProperty ByteDataProperty = DependencyProperty.RegisterAttached("ByteData", typeof(IEnumerable<byte>), typeof(TextBoxBehavior), new PropertyMetadata(null, OnByteDataChanged));

        /// <summary>
        /// Set the value of the ByteData property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to set the value</param>
        /// <param name="value">Value of the property</param>
        static public void SetByteData(DependencyObject source, IEnumerable<byte> value)
        {
            source.SetValue(ByteDataProperty, value);
        }

        /// <summary>
        /// Get the value of the ByteData property
        /// </summary>
        /// <param name="source">DependencyObject for which we want to get the value</param>
        /// <returns>Value of the property</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        static public IEnumerable<byte> GetByteData(DependencyObject source)
        {
            return source.GetValue(ByteDataProperty) as IEnumerable<byte>;
        }

        /// <summary>
        /// Callback when the ByteData property is changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        static public void OnByteDataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source is TextBox))
            {
                throw new ArgumentException("Source is not a TextBox");
            }

            if (!source.BeginUpdate())
            {
                return;
            }

            TextBox control = source as TextBox;
            if (!(e.NewValue is IEnumerable<byte>))
            {
                control.Text = string.Empty;
            }
            else
            {
                control.Text = ProcessByteArray(e.NewValue as IEnumerable<byte>, GetByteSpacer(control), control.GetNumberBase());
            }

            source.EndUpdate();
        }

        #endregion

        #region Methods

        static private double BoundNumber(this DependencyObject control, double value)
        {
            double minValue = GetNumberMinValue(control).HasValue ? GetNumberMinValue(control).Value : double.MinValue;
            double maxValue = GetNumberMaxValue(control).HasValue ? GetNumberMaxValue(control).Value : double.MaxValue;
            return Math.Max(minValue, Math.Min(maxValue, value));
        }

        static private bool ValidatePattern(this TextBox control, string text)
        {
            string pattern = GetPattern(control);
            if (!string.IsNullOrEmpty(pattern))
            {
                return Regex.IsMatch(text, pattern);
            }

            int numberBase = 10;
            switch (GetRegexType(control))
            {
                case RegexType.Binary:
                    pattern = "^[0-1]*$";
                    numberBase = 2;
                    break;

                case RegexType.Hexadecimal:
                    pattern = "^[0-9a-fA-F]*$";
                    numberBase = 16;
                    break;

                case RegexType.Integer:
                    pattern = "^-?[0-9]*$";
                    numberBase = 10;
                    break;

                case RegexType.Double:
                    pattern = "^-?[0-9]*[\\.,]?[0-9]*$";
                    numberBase = 10;
                    break;

                default:
                    control.TextChanged -= OnTextChanged;
                    return true;
            }

            if (GetIsByteData(control))
            {
                string rawByteDate = text.Replace(GetByteSpacer(control), string.Empty);
                if (Regex.IsMatch(rawByteDate, pattern))
                {
                    int byteSize = numberBase == 2 ? 8 : 2;
                    int cursorOldPos = control.Text.Substring(0, control.SelectionStart).Replace(GetByteSpacer(control), string.Empty).Length;
                    int cursorPos = cursorOldPos + cursorOldPos / byteSize - (cursorOldPos % byteSize == 0 ? 1 : 0);

                    string spacedByteData;
                    SetByteData(control, ProcessRawByteString(rawByteDate.ToUpper(), numberBase, GetByteSpacer(control), out spacedByteData));
                    control.Text = spacedByteData;

                    control.SelectionStart = Math.Max(0, Math.Min(cursorPos, control.Text.Length));

                    return true;
                }
            }
            else if (Regex.IsMatch(text, pattern))
            {
                if (Regex.IsMatch(text, "^-?[\\.,]?$"))
                {
                    SetNumberValue(control, null);
                    return true;
                }

                double value;
                try
                {
                    if (numberBase == 10)
                    {
                        value = Convert.ToDouble(text, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        value = Convert.ToInt64(text, numberBase);
                    }
                }
                catch (OverflowException)
                {
                    SetNumberValue(control, control.BoundNumber(text.Contains("-") ?
                        (numberBase == 10 ? double.MinValue : long.MinValue) : (numberBase == 10 ? double.MaxValue : long.MaxValue)));
                    if (numberBase == 10)
                    {
                        control.Text = Convert.ToString(GetNumberValue(control).Value);
                    }
                    else
                    {
                        control.Text = Convert.ToString((long)GetNumberValue(control).Value, numberBase);
                    }
                    return true;
                }

                if (value != control.BoundNumber(value))
                {
                    SetNumberValue(control, control.BoundNumber(value));
                    if (numberBase == 10)
                    {
                        control.Text = Convert.ToString(GetNumberValue(control).Value);
                    }
                    else
                    {
                        control.Text = Convert.ToString((long)GetNumberValue(control).Value, numberBase);
                    }
                    return true;
                }
                else
                {
                    SetNumberValue(control, value);
                    return true;
                }
            }

            return false;
        }

        static private int GetNumberBase(this DependencyObject control)
        {
            switch (GetRegexType(control))
            {
                case RegexType.Binary:
                    return 2;

                case RegexType.Hexadecimal:
                    return 16;

                case RegexType.Integer:
                case RegexType.Double:
                default:
                    return 10;
            }
        }

        static private IEnumerable<byte> ProcessRawByteString(string rawByteData, int numberBase, string spacer, out string spacedByteData)
        {
            spacedByteData = string.Empty;

            if (string.IsNullOrEmpty(rawByteData))
            {
                return null;
            }

            List<byte> bytes = new List<byte>();

            int byteSize = numberBase == 2 ? 8 : 2;
            int index = byteSize;
            string byteString;
            while (index < rawByteData.Length)
            {
                byteString = rawByteData.Substring(index - byteSize, byteSize);
                spacedByteData += byteString + spacer;
                bytes.Add(Convert.ToByte(byteString, numberBase));
                index += byteSize;
            }
            byteString = rawByteData.Substring(index - byteSize);
            spacedByteData += byteString;
            bytes.Add(Convert.ToByte(byteString, numberBase));

            return bytes.ToArray();
        }

        static private string ProcessByteArray(IEnumerable<byte> bytes, string spacer, int numberBase)
        {
            if (bytes == null || bytes.Count() == 0)
            {
                return string.Empty;
            }

            int byteSize = numberBase == 2 ? 8 : 2;
            string spacedByteData = string.Empty;
            foreach (byte b in bytes)
            {
                spacedByteData += Convert.ToString(b, numberBase).PadLeft(byteSize, '0') + spacer;
            }

            spacedByteData = spacedByteData.Remove(spacedByteData.Length - spacer.Length);

            return spacedByteData.ToUpper();
        }

        #endregion

        #region Callbacks

        static private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox control = sender as TextBox;

            if (!control.BeginUpdate())
            {
                return;
            }

            if (!control.ValidatePattern(control.Text))
            {
                int selectionStart = control.SelectionStart;
                string text = control.Text;
                foreach (TextChange change in e.Changes)
                {
                    text = text.Remove(change.Offset, change.AddedLength);
                    if (selectionStart > change.Offset)
                    {
                        selectionStart -= change.AddedLength;
                    }
                }
                control.Text = text;
                control.SelectionStart = selectionStart;
            }

            control.EndUpdate();
        }

        #endregion
    }
}
