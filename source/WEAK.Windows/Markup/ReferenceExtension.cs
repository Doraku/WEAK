using System;
using System.Windows.Markup;

namespace WEAK.Windows.Markup
{
    [ContentProperty("Name")]
    sealed public class ReferenceExtension : Reference
    {
        #region Initialisation

        public ReferenceExtension()
            : base()
        { }

        public ReferenceExtension(string name)
            : base(name)
        { }

        #endregion

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (WEAKEnvironment.IsInDesignMode)
            {
                return null;
            }

            return base.ProvideValue(serviceProvider);
        }

        #endregion
    }
}
