using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace WEAK.Windows.AttachedProperty
{
    public static class WindowBehavior
    {
        #region Fields

        public static readonly DependencyProperty IsBorderlessProperty;
        public static readonly DependencyProperty TitleTemplateProperty;
        public static readonly DependencyProperty CloseButtonVisibilityProperty;
        public static readonly DependencyProperty MinimizeButtonVisibilityProperty;
        public static readonly DependencyProperty NormalMaximizeButtonVisibilityProperty;

        #endregion

        #region Initialisation

        static WindowBehavior()
        {
            IsBorderlessProperty = DependencyProperty.RegisterAttached("IsBorderless", typeof(bool), typeof(WindowBehavior), new PropertyMetadata(false, OnIsBorderlessPropertyChanged));
            TitleTemplateProperty = DependencyProperty.RegisterAttached("TitleTemplate", typeof(ControlTemplate), typeof(WindowBehavior));
            CloseButtonVisibilityProperty = DependencyProperty.RegisterAttached("CloseButtonVisibility", typeof(Visibility), typeof(WindowBehavior), new PropertyMetadata(Visibility.Visible));
            MinimizeButtonVisibilityProperty = DependencyProperty.RegisterAttached("MinimizeButtonVisibility", typeof(Visibility), typeof(WindowBehavior), new PropertyMetadata(Visibility.Visible));
            NormalMaximizeButtonVisibilityProperty = DependencyProperty.RegisterAttached("NormalMaximizeButtonVisibility", typeof(Visibility), typeof(WindowBehavior), new PropertyMetadata(Visibility.Visible));
        }

        #endregion

        #region Methods

        public static bool GetIsBorderless(this DependencyObject target)
        {
            return (bool)target.GetValue(IsBorderlessProperty);
        }

        public static void SetIsBorderless(this DependencyObject target, bool value)
        {
            target.SetValue(IsBorderlessProperty, value);
        }

        public static ControlTemplate GetTitleTemplate(this DependencyObject target)
        {
            return target.GetValue(TitleTemplateProperty) as ControlTemplate;
        }

        public static void SetTitleTemplate(this DependencyObject target, ControlTemplate value)
        {
            target.SetValue(TitleTemplateProperty, value);
        }

        public static Visibility GetCloseButtonVisibility(this DependencyObject target)
        {
            return (Visibility)target.GetValue(CloseButtonVisibilityProperty);
        }

        public static void SetCloseButtonVisibility(this DependencyObject target, Visibility value)
        {
            target.SetValue(CloseButtonVisibilityProperty, value);
        }

        public static Visibility GetMinimizeButtonVisibility(this DependencyObject target)
        {
            return (Visibility)target.GetValue(MinimizeButtonVisibilityProperty);
        }

        public static void SetMinimizeButtonVisibility(this DependencyObject target, Visibility value)
        {
            target.SetValue(MinimizeButtonVisibilityProperty, value);
        }

        public static Visibility GetNormalMaximizeButtonVisibility(this DependencyObject target)
        {
            return (Visibility)target.GetValue(NormalMaximizeButtonVisibilityProperty);
        }

        public static void SetNormalMaximizeButtonVisibility(this DependencyObject target, Visibility value)
        {
            target.SetValue(NormalMaximizeButtonVisibilityProperty, value);
        }

        #endregion

        #region Callbacks

        private static void OnIsBorderlessPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            Window window = target as Window;
            if (window != null && args.OldValue != args.NewValue)
            {
                if (window.GetIsBorderless())
                {
                    window.WindowStyle = WindowStyle.SingleBorderWindow;
                    window.AllowsTransparency = false;
                    window.BorderThickness = new Thickness(1);
                    window.Margin = new Thickness(0);
                    window.Padding = new Thickness(0);
                    WindowChrome.SetWindowChrome(window, new WindowChrome
                    {
                        GlassFrameThickness = new Thickness(1),
                        UseAeroCaptionButtons = false,
                        NonClientFrameEdges = NonClientFrameEdges.None,
                        ResizeBorderThickness = new Thickness(6),
                        CornerRadius = new CornerRadius(0)
                    });
                    if (window.Template == null)
                    {
                        ControlTemplate template = Application.Current.TryFindResource("WEAK_WindowBehavior_DefaultTemplate") as ControlTemplate;
                        if (template == null)
                        {
                            Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("/WEAK.Windows;component/Themes/WindowBehavior.xaml", UriKind.Relative)) as ResourceDictionary);
                        }
                        if (window.GetTitleTemplate() == null)
                        {
                            window.SetTitleTemplate(window.FindResource("WEAK_WindowBehavior_DefaultTitleTemplate") as ControlTemplate);
                        }
                        window.Template = window.FindResource("WEAK_WindowBehavior_DefaultTemplate") as ControlTemplate;
                    }
                }
            }
        }

        #endregion
    }
}
