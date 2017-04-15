using System;

namespace WEAK.Object
{
    /// <summary>
    /// Provides a mean to retrieve an unique instance for a given type, initialising it lazily throught the Factory class.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    public static class Singleton<T>
    {
        #region Fields

        private static readonly Lazy<T> _lazyInstance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique instance.
        /// </summary>
        public static T Instance
        {
            get { return _lazyInstance.Value; }
        }

        #endregion

        #region Initialisation

        static Singleton()
        {
            _lazyInstance = new Lazy<T>(Factory<T>.CreateInstance, true);
        }

        #endregion
    }
}
