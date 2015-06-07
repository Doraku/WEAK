using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace WEAK.Windows
{
    public static class WEAKEnvironment
    {
        #region Fields

        public static readonly bool IsInDesignMode;

        #endregion

        #region Initialisation

        static WEAKEnvironment()
        {
            IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }

        #endregion
    }
}
