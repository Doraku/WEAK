using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WEAK.Windows.Windows
{
    public class ZoomControl : ContentControl
    {
        #region Fields

        public static readonly DependencyProperty ZoomProperty;

        #endregion

        #region Properties

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        #endregion

        #region Initialisation

        static ZoomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomControl), new FrameworkPropertyMetadata(typeof(ZoomControl)));

            ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(ZoomControl), new PropertyMetadata(1d, null, OnCoerceZoom));
        }

        #endregion

        #region Callbacks

        private static object OnCoerceZoom(DependencyObject d, object baseValue)
        {
            return baseValue is double ? Math.Max((double)baseValue, 0.01) : 1;
        }

        #endregion

        #region UIElement

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                double ratio = Zoom;
                Zoom += Zoom * 0.1 * (e.Delta > 0 ? 1d : -1d);
                ratio = Zoom / ratio;
                ScrollViewer viewer;
                if (this.TryGetFirstChild(out viewer))
                {
                    Point pos = e.GetPosition(this);
                    viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset * ratio + pos.X * (ratio - 1));
                    viewer.ScrollToVerticalOffset(viewer.VerticalOffset * ratio + pos.Y * (ratio - 1));
                }
                e.Handled = true;
            }
            else
            {
                base.OnPreviewMouseWheel(e);
            }
        }

        #endregion
    }
}
