using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WEAK.Communication;
using WEAK.Object;

namespace WEAK.Module
{
    public sealed class UnitManager : IDisposable
    {
        #region Types

        private class LoadRequest
        { }

        private class LoadResult
        {
            #region Fields

            public readonly Type Type;
            public readonly IUnit Unit;
            public readonly Exception LoadException;
            public readonly Exception UnloadException;

            #endregion

            #region Initialisation

            public LoadResult(Type type, IUnit unit, Exception loadException, Exception unloadException)
            {
                Type = type;
                Unit = unit;
                LoadException = loadException;
                UnloadException = unloadException;
            }

            #endregion
        }

        private sealed class UnitLoader<T> : IDisposable
            where T : IUnit
        {
            #region Fields

            private readonly IPublisher _publisher;
            private readonly IDisposable _subscription;

            #endregion

            #region Initialisation

            public UnitLoader(IPublisher publisher)
            {
                _publisher = publisher;
                _subscription = publisher.Subscribe(this);
            }

            #endregion

            #region Callbacks

            [Subscribe(ExecutionMode.LongRunning)]
            private void On(LoadRequest loadRequest)
            {
                T unit = default(T);
                try
                {
                    unit = Factory<T>.CreateInstance();

                    unit.Load();

                    _publisher.Publish(new LoadResult(typeof(T), unit, null, null));
                }
                catch (Exception damn)
                {
                    if (unit != null)
                    {
                        try
                        {
                            unit.Unload();

                            _publisher.Publish(new LoadResult(typeof(T), unit, damn, null));
                        }
                        catch (Exception damnAgain)
                        {
                            _publisher.Publish(new LoadResult(typeof(T), unit, damn, damnAgain));
                        }
                    }
                }
            }

            #endregion

            #region IDisposable

            public void Dispose()
            {
                _subscription.Dispose();

                GC.SuppressFinalize(this);
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IPublisher _publisher;
        private readonly object _locker;
        private readonly Dictionary<Type, IDisposable> _pendingUnits;
        private readonly Dictionary<Type, IUnit> _loadedUnits;
        private readonly CountdownEvent _handle;
        private readonly IDisposable _subscription;

        private bool _isDisposed;

        #endregion

        #region Properties

        IEnumerable<Type> PendingUnits
        {
            get
            {
                lock (_locker)
                {
                    return _pendingUnits.Keys.ToList();
                }
            }
        }

        IEnumerable<Type> LoadedUnits
        {
            get
            {
                lock (_locker)
                {
                    return _loadedUnits.Keys.ToList();
                }
            }
        }

        #endregion

        #region Initialisation

        public UnitManager()
        {
            _publisher = new EventAggregator();
            _locker = new object();
            _pendingUnits = new Dictionary<Type, IDisposable>();
            _loadedUnits = new Dictionary<Type, IUnit>();
            _handle = new CountdownEvent(0);
            _subscription = _publisher.Subscribe(this);
        }

        ~UnitManager()
        {
            Dispose();
        }

        #endregion

        #region Methods

        public void Add<T>()
            where T : class, IUnit
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }

            Type type = typeof(T);

            lock (_locker)
            {
                if (!_pendingUnits.ContainsKey(type)
                    && !_loadedUnits.OfType<T>().Any())
                {
                    _pendingUnits.Add(type, new UnitLoader<T>(_publisher));
                }
            }
        }

        public void Add(Assembly assembly)
        {
            Type interfaceType = typeof(IUnit);
            MethodInfo methodInfo = typeof(UnitManager).GetMethod("Add", Type.EmptyTypes);

            foreach (Type type in assembly.GetTypes())
            {
                if (interfaceType.IsAssignableFrom(type)
                    && type.IsClass
                    && !type.IsAbstract)
                {
                    methodInfo.MakeGenericMethod(type).Invoke(this, null);
                }
            }
        }

        public bool Remove<T>()
            where T : class, IUnit
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }

            Type type = typeof(T);
            IUnit unit;

            lock (_locker)
            {
                if (_pendingUnits.ContainsKey(type))
                {
                    _pendingUnits.Remove(type);

                    return true;
                }

                if (!_loadedUnits.TryGetValue(type, out unit))
                {
                    return false;
                }

                _loadedUnits.Remove(type);
            }

            // TODO: check dependencies
            unit.Unload();

            return true;
        }

        public void Load()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }

            lock (_locker)
            {
                _handle.Reset(_pendingUnits.Count);

                _publisher.Publish(new LoadRequest());

                //_handle.Wait();
            }
        }

        public void Unload()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }
        }

        #endregion

        #region Callbacks

        [Subscribe(ExecutionMode.Context)]
        private void On(LoadResult result)
        {
            if (result.Unit != null)
            {
                _pendingUnits[result.Type].Dispose();
                _pendingUnits.Remove(result.Type);
                if (result.LoadException == null)
                {
                    _loadedUnits.Add(result.Type, result.Unit);
                }
            }

            _handle.Signal();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Unload();

                _subscription.Dispose();
                _handle.Dispose();
                (_publisher as IDisposable).Dispose();

                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
