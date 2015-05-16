using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using WEAK.Windows.Base;

namespace WEAK.Windows
{
    public static class WEAKEnvironment
    {
        #region Fields

        private static readonly IList<IModule> _loadedModules;

        public static readonly bool IsInDesignMode;

        #endregion

        #region Initialisation

        static WEAKEnvironment()
        {
            _loadedModules = new List<IModule>();

            IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }

        #endregion

        #region Methods

        public static bool Load<T>()
            where T : class, IModule, new()
        {
            lock ((_loadedModules as ICollection).SyncRoot)
            {
                if (_loadedModules.OfType<T>().Any())
                {
                    return false;
                }
                else
                {
                    T module = new T();

                    module.Open();
                    _loadedModules.Add(module);

                    return true;
                }
            }
        }

        public static bool Unload<T>()
            where T : class, IModule, new()
        {
            lock ((_loadedModules as ICollection).SyncRoot)
            {
                T module = _loadedModules.OfType<T>().FirstOrDefault();
                if (module != null)
                {
                    module.Close();
                    _loadedModules.Remove(module);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion
    }
}
