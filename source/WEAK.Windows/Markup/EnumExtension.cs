using System;
using System.Windows.Markup;

namespace WEAK.Windows.Markup
{
    sealed public class EnumExtension : MarkupExtension
    {
        #region Fields

        private readonly Type _enumType;

        #endregion

        #region Initialisation

        public EnumExtension(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Specified type must be an enum.");
            }

            _enumType = enumType;
        }

        #endregion

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_enumType);
        }

        #endregion
    }
}
