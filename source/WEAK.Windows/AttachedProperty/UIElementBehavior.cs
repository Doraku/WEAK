using System;
using System.Windows;
using System.Windows.Input;
using WEAK.Helper;

namespace WEAK.Windows.AttachedProperty
{
    public static class UIElementBehavior
    {
        #region Fields

        static public readonly DependencyProperty InputBindingsProperty = DependencyProperty.RegisterAttached("InputBindings", typeof(InputBindingCollection), typeof(UIElementBehavior), new PropertyMetadata(null, OnInputBindingsChanged));

        #endregion

        #region Methods

        public static void SetInputBindings(this UIElement source, InputBindingCollection value)
        {
            source.SetValue(InputBindingsProperty, value);
        }

        public static InputBindingCollection GetInputBindings(this UIElement source)
        {
            return source.GetValue(InputBindingsProperty) as InputBindingCollection;
        }

        #endregion

        #region Callbacks

        private static void OnInputBindingsChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            UIElement control = source as UIElement;
            if (control == null)
            {
                throw new ArgumentException(string.Format("Not of type \"{0}\" or null.", typeof(UIElement).Name), nameof(source));
            }

            control.InputBindings.Clear();

            if (e.NewValue is InputBindingCollection)
            {
                control.InputBindings.AddRange(e.NewValue as InputBindingCollection);
            }
        }

        #endregion
    }
}
