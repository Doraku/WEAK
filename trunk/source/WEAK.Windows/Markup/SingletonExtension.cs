using System;
using System.Windows.Markup;
using WEAK.Object;

namespace WEAK.Windows.Markup
{
    sealed public class SingletonExtension : MarkupExtension
    {
        #region Fields

        private readonly Type _type;

        #endregion

        #region Initialisation

        public SingletonExtension(Type type)
        {
            _type = type;
        }

        #endregion

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return typeof(Singleton<>).MakeGenericType(_type).GetProperty("Instance").GetGetMethod().Invoke(null, null);
        }

        #endregion
    }
}
