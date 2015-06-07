using System;

namespace WEAK.Object
{
    /// <summary>
    /// Specifies the key to use when resolving a constructor parameter in the Factory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class KeyAttribute : Attribute
    {
        #region Fields

        private readonly string _value;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value of the attribute.
        /// </summary>
        public string Value
        {
            get { return _value; }
        }

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialise a new instance of KeyAttribute.
        /// </summary>
        /// <param name="publishingMode"></param>
        public KeyAttribute(string value)
        {
            _value = value;
        }

        #endregion
    }
}
