using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WEAK.Helper;

namespace WEAK.Object
{
    /// <summary>
    /// Provides a set of methods to creates instances of a given type and store them relatively to a key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TInstance">The type of the instance.</typeparam>
    public static class ReferenceManager<TKey, TInstance>
        where TKey : class
        where TInstance : class
    {
        #region Types

        private class Watcher
        {
            #region Fields

            private readonly TKey _key;

            #endregion

            #region Initialisation

            public Watcher(TKey key)
            {
                _key = key;
            }

            ~Watcher()
            {
                lock (_locker)
                {
                    _instances.Remove(_key);
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly object _locker;
        private static readonly Dictionary<TKey, WeakReference<TInstance>> _instances;
        private static readonly ConditionalWeakTable<TInstance, Watcher> _cleaners;

        #endregion

        #region Initialisation

        static ReferenceManager()
        {
            _locker = new object();
            _instances = new Dictionary<TKey, WeakReference<TInstance>>();
            _cleaners = new ConditionalWeakTable<TInstance, Watcher>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the instance relative to a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The instance or null if no instance has been associated with the given key.</returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        public static TInstance Get(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => key));
            }

            TInstance ret = null;

            lock (_locker)
            {
                if (_instances.ContainsKey(key))
                {
                    _instances[key].TryGetTarget(out ret);
                }
            }

            return ret;
        }

        /// <summary>
        /// Gets the instance relative to a key. If no instance exists, it will be created using the given factory.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="creator">The factory.</param>
        /// <returns>The existing instance or the one created using the factory.</returns>
        /// <exception cref="System.ArgumentNullException">key or creator is null.</exception>
        public static TInstance GetOrCreate(TKey key, Func<TKey, TInstance> creator)
        {
            if (key == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => key));
            }
            if (creator == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => creator));
            }

            TInstance ret = null;

            lock (_locker)
            {
                ret = Get(key);
                if (ret == null)
                {
                    ret = creator(key);

                    Watcher watcher;
                    if (_cleaners.TryGetValue(ret, out watcher))
                    {
                        GC.SuppressFinalize(watcher);
                        _cleaners.Remove(ret);
                    }

                    _instances[key] = new WeakReference<TInstance>(ret);
                    _cleaners.GetValue(ret, v => new Watcher(key));
                }
            }

            return ret;
        }

        /// <summary>
        /// Gets the instance relative to a key. If no instance exists, it will be created using the Factory class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The existing instance or the one created using the factory.</returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        public static TInstance GetOrCreate(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => key));
            }

            return GetOrCreate(key, k => Factory<TInstance>.CreateInstance());
        }

        /// <summary>
        /// Releases the instance linked to the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The instance or null if no instance has been associated with the given key.</returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        public static TInstance Release(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => key));
            }

            TInstance ret;

            lock (_locker)
            {
                ret = Get(key);
                if (ret != null)
                {
                    _instances.Remove(key);
                    Watcher watcher;
                    if (_cleaners.TryGetValue(ret, out watcher))
                    {
                        GC.SuppressFinalize(watcher);
                        _cleaners.Remove(ret);
                    }
                }
            }

            return ret;
        }

        #endregion
    }
}
