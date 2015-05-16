using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace WEAK.Supervision
{
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
                _instances.Remove(_key);
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

        public static bool Get(TKey key, out TInstance value)
        {
            value = null;

            lock (_locker)
            {
                if (_instances.ContainsKey(key))
                {
                    _instances[key].TryGetTarget(out value);
                }
            }

            return value != null;
        }

        public static bool GetOrCreate(TKey key, Func<TKey, TInstance> creator, out TInstance value)
        {
            value = null;

            lock (_locker)
            {
                if (Get(key, out value))
                {
                    return true;
                }

                value = creator(key);
                _instances[key] = new WeakReference<TInstance>(value);
                _cleaners.GetValue(value, (TInstance v) => { return new Watcher(key); });

                return false;
            }
        }

        public static bool GetOrCreate(TKey key, out TInstance value)
        {
            return GetOrCreate(key, (k) => Factory<TInstance>.CreateInstance(), out value);
        }

        public static bool Release(TKey key, out TInstance value)
        {
            lock (_locker)
            {
                if (Get(key, out value))
                {
                    _instances.Remove(key);
                }
            }

            return value != null;
        }

        #endregion
    }
}
