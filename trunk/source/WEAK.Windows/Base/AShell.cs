using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WEAK.Communication;
using WEAK.Object;

namespace WEAK.Windows.Base
{
    public class AShell : Window
    {
        #region Private fields

        private IPublisher _publisher;
        private Dictionary<Type, IModule> _loadedModules = new Dictionary<Type, IModule>();

        #endregion

        #region Properties

        protected IPublisher Publisher
        {
            get { return _publisher; }
            set
            {
                if (_publisher != value)
                {
                    _publisher = value;
                    Publisher.HookUp(this);
                }
            }
        }

        #endregion

        #region Initialisation

        public AShell()
        { }

        #endregion

        #region Methods

        //protected bool Load<T>()
        //    where T : IModule, new()
        //{
        //    if (_loadedModules.ContainsKey(typeof(T)))
        //    {
        //        return false;
        //    }

        //    IModule module = Singleton<T>.Instance;
        //    module.Publisher = Publisher;
        //    module.Initialize();

        //    _loadedModules[typeof(T)] = module;
        //    return true;
        //}

        //protected bool Unload<T>()
        //    where T : IModule
        //{
        //    if (!_loadedModules.ContainsKey(typeof(T)))
        //    {
        //        return false;
        //    }

        //    _loadedModules[typeof(T)].Close();
        //    _loadedModules[typeof(T)].Publisher = null;
        //    _loadedModules.Remove(typeof(T));
        //    return true;
        //}

        #endregion

        #region Callbacks

        [AutoHookUp(ExecutionMode.Context)]
        protected void On(SetRegionContentRequest arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }

            ContentControl region = FindName(arg.RegionName) as ContentControl;
            if (region == null)
            {
                throw new Exception(string.Format("ContentControl \"{0}\" not found on shell.", arg.RegionName));
            }

            region.Content = arg.View;
        }

        //protected override void OnClosed(EventArgs e)
        //{
        //    foreach (IModule module in _loadedModules.Values)
        //    {
        //        module.Close();
        //        module.Publisher = null;
        //    }
        //    _loadedModules.Clear();

        //    if (Publisher != null)
        //    {
        //        Publisher.Publish(new ApplicationExitRequest());
        //    }

        //    base.OnClosed(e);
        //}

        #endregion
    }
}
