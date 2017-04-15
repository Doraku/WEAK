using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shell;

namespace WEAK.Windows.Themes
{
    internal sealed partial class WindowBehavior : ResourceDictionary
    {
        #region Methods

        private static void ResizeGrip(FrameworkElement titleElement)
        {
            Window window = Window.GetWindow(titleElement);

            WindowChrome windowChrome = WindowChrome.GetWindowChrome(window).CloneCurrentValue() as WindowChrome;
            windowChrome.CaptionHeight = Math.Max(0,
                    window.Margin.Top
                    + window.BorderThickness.Top
                    + titleElement.ActualHeight
                    - windowChrome.ResizeBorderThickness.Top);

            WindowChrome.SetWindowChrome(window, windowChrome);
        }

        #endregion

        #region Callbacks

        private static void OnStateChanged(object sender, EventArgs args)
        {
            Window window = sender as Window;
            WindowChrome windowChrome = WindowChrome.GetWindowChrome(window).CloneCurrentValue() as WindowChrome;
            switch (window.WindowState)
            {
                case WindowState.Normal:
                    window.Margin = new Thickness(0);
                    windowChrome.GlassFrameThickness = new Thickness(1);
                    break;

                case WindowState.Maximized:
                    window.Margin = new Thickness(7);
                    windowChrome.GlassFrameThickness = new Thickness(0);
                    break;
            }
            WindowChrome.SetWindowChrome(window, windowChrome);
        }

        private void OnMinimize(object sender, EventArgs args)
        {
            Window window = Window.GetWindow(sender as FrameworkElement);
            window.WindowState = WindowState.Minimized;
        }

        private void OnNormalMaximize(object sender, RoutedEventArgs args)
        {
            Window window = Window.GetWindow(sender as FrameworkElement);
            switch (window.WindowState)
            {
                case WindowState.Normal:
                    window.WindowState = WindowState.Maximized;
                    break;

                case WindowState.Maximized:
                    window.WindowState = WindowState.Normal;
                    break;
            }
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(sender as FrameworkElement).Close();
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            ResizeGrip(sender as FrameworkElement);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(sender as FrameworkElement);

            (sender as FrameworkElement).Loaded -= OnLoaded;
            window.StateChanged += OnStateChanged;

            ResizeGrip(sender as FrameworkElement);
            OnStateChanged(window, null);
        }

        #endregion
    }
}
