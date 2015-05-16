using System;

namespace WEAK.Supervision
{
    public class Singleton<T>
    {
        #region Fields

        internal static readonly Lazy<T> LazyInstance;

        #endregion

        #region Properties

        public static T Instance
        {
            get { return LazyInstance.Value; }
        }

        #endregion

        #region Initialisation

        static Singleton()
        {
            LazyInstance = new Lazy<T>(Factory<T>.CreateInstance, true);
        }

        #endregion
    }
}
