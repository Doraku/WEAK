using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using WEAK.Helper;

namespace WEAK.Communication
{
    /// <summary>
    /// Provides a base implementation of the IPublisher interface
    /// using a SynchronizationContext for the Context ExecutionMode,
    /// Task for Asynch ExecutionMode
    /// and Task with LongRunning for LongRunning ExecutionMode.
    /// Subscribing twice a method with same or different ExecutionMode will only keep the first one.
    /// Disposing an instance of EventAggregator will clear all its subscriptions 
    /// but it will not stop running asynchrone tasks.
    /// </summary>
    public sealed class EventAggregator : IPublisher, IDisposable
    {
        #region Types

        private delegate void WeakDelegate(object source, object arg);

        private static class Publisher<T>
        {
            #region Fields

            public static readonly List<Action<object>> Actions;

            #endregion

            #region Initialisation

            static Publisher()
            {
                Actions = new List<Action<object>>();

                lock (_locker)
                {
                    Add(_instances.Count - 1);
                    RegisterType(typeof(T));
                }
            }

            #endregion

            #region Methods

            public static void Subscribe(int id, Action<object> action, bool isFirst)
            {
                if (action != null)
                {
                    Action<object> temp = Actions[id];
                    if (temp != null && temp != action
                        && !temp.GetInvocationList().Contains(action))
                    {
                        if (isFirst)
                        {
                            action = (Action<object>)Delegate.Combine(action, temp);
                        }
                        else
                        {
                            action = (Action<object>)Delegate.Combine(temp, action);
                        }
                    }

                    Actions[id] = action;
                }
            }

            public static void Unsubscribe(int id, Action<object> action)
            {
                Actions[id] -= action;
            }

            public static void Clear(int id)
            {
                Actions[id] = null;
            }

            public static void Add(int id)
            {
                Actions.AddRange(Enumerable.Repeat<Action<object>>(null, id + 1 - Actions.Count));
            }

            #endregion
        }

        private sealed class Subscription<T> : IDisposable
        {
            #region Fields

            private readonly int _publisherId;
            private readonly Action<object> _action;

            private bool _isDisposed;

            #endregion

            #region Initialisation

            public Subscription(int publisherId, Action<object> action)
            {
                _publisherId = publisherId;
                _action = action;

                _isDisposed = false;
            }

            ~Subscription()
            {
                Dispose();
            }

            #endregion

            #region IDisposable

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    lock (_locker)
                    {
                        foreach (Type type in _relayTypes[typeof(T)])
                        {
                            typeof(Publisher<>).MakeGenericType(type).GetMethod("Unsubscribe").Invoke(null, new object[] { _publisherId, _action });
                        }
                    }

                    _isDisposed = true;
                    GC.SuppressFinalize(this);
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly object _locker;
        private static readonly List<WeakReference<EventAggregator>> _instances;
        private static readonly Dictionary<Type, LinkedList<Type>> _subTypes;
        private static readonly Dictionary<Type, LinkedList<Type>> _relayTypes;
        private static readonly Dictionary<MethodInfo, WeakDelegate> _weakDelegates;

        private readonly SynchronizationContext _context;
        private readonly Dictionary<MethodInfo, Dictionary<WeakReference, Action<object>>> _registrations;
        private readonly List<Action<object>> _firstActions;

        private int _id;
        private bool _isDisposed = false;

        #endregion

        #region Initialisation

        static EventAggregator()
        {
            _locker = new object();
            _instances = new List<WeakReference<EventAggregator>>();
            _subTypes = new Dictionary<Type, LinkedList<Type>>();
            _relayTypes = new Dictionary<Type, LinkedList<Type>>();
            _weakDelegates = new Dictionary<MethodInfo, WeakDelegate>();

            _subTypes[typeof(object)] = new LinkedList<Type>();
            _subTypes[typeof(object)].AddLast(typeof(object));
            _relayTypes[typeof(object)] = new LinkedList<Type>();
            _relayTypes[typeof(object)].AddLast(typeof(object));
        }

        /// <summary>
        /// Initializes a new instance of the WEAK.Communication.EventAggregator class.
        /// </summary>
        /// <param name="context">The SynchronizationContext to use for Context ExecutionMode.</param>
        /// <exception cref="ArgumentNullException">context is null.</exception>
        public EventAggregator(SynchronizationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => context));
            }

            _context = context;
            _registrations = new Dictionary<MethodInfo, Dictionary<WeakReference, Action<object>>>();
            _firstActions = new List<Action<object>>();

            lock (_locker)
            {
                _id = _instances.Count;
                _instances.Add(new WeakReference<EventAggregator>(this));
                foreach (Type type in _subTypes.Keys)
                {
                    typeof(Publisher<>).MakeGenericType(type).GetMethod("Add").Invoke(null, new object[] { _id });
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the WEAK.Communication.EventAggregator class
        /// with the current SynchronizationContext or create one if null.
        /// </summary>
        public EventAggregator()
            : this(SynchronizationContext.Current ?? new SynchronizationContext())
        { }

        ~EventAggregator()
        {
            Dispose();
        }

        #endregion

        #region Methods

        private static bool IsFirst(ExecutionMode executionMode)
        {
            switch (executionMode)
            {
                case ExecutionMode.Async:
                case ExecutionMode.ContextAsync:
                case ExecutionMode.LongRunning:
                    return true;

                case ExecutionMode.Direct:
                case ExecutionMode.Context:
                default:
                    return false;
            }
        }

        private static void RegisterType(Type main, Type sub)
        {
            if (!_subTypes.ContainsKey(main)
                || !_subTypes.ContainsKey(sub))
            {
                return;
            }

            foreach (Type type in _subTypes[sub])
            {
                if (!_subTypes[main].Contains(type))
                {
                    _subTypes[main].AddLast(type);
                }
            }
        }

        private static void RegisterType(Type type)
        {
            if (_subTypes.ContainsKey(type))
            {
                return;
            }

            _subTypes[type] = new LinkedList<Type>();
            _subTypes[type].AddLast(type);
            _subTypes[type].AddLast(typeof(object));

            foreach (Type inter in type.GetInterfaces())
            {
                RegisterType(inter);
                RegisterType(type, inter);
            }

            if (type.BaseType != null)
            {
                RegisterType(type.BaseType);
                RegisterType(type, type.BaseType);
            }

            foreach (Type sub in _subTypes[type])
            {
                if (!_relayTypes.ContainsKey(sub))
                {
                    _relayTypes[sub] = new LinkedList<Type>();
                    _relayTypes[sub].AddLast(sub);
                }

                if (!_relayTypes[sub].Contains(type))
                {
                    _relayTypes[sub].AddLast(type);
                    foreach (WeakReference<EventAggregator> reference in _instances)
                    {
                        EventAggregator publisher;
                        if (!reference.TryGetTarget(out publisher)
                            || publisher == null)
                        {
                            continue;
                        }

                        List<Action<object>> actions = (List<Action<object>>)typeof(Publisher<>).MakeGenericType(sub).GetField("Actions").GetValue(null);

                        if (actions[publisher._id] != null)
                        {
                            foreach (Action<object> action in actions[publisher._id].GetInvocationList())
                            {
                                typeof(Publisher<>).MakeGenericType(type).GetMethod("Subscribe").Invoke(null, new object[] { publisher._id, action, publisher._firstActions.Contains(action) });
                            }
                        }
                    }
                }
            }
        }

        private static WeakDelegate GetWeakDelegate(MethodInfo method, Type targetType)
        {
            if (!_weakDelegates.ContainsKey(method))
            {
                DynamicMethod dynamicDelegate = new DynamicMethod(
                    string.Format("weak_{0}", _weakDelegates.Count),
                    typeof(void),
                    new Type[] { typeof(object), typeof(object) },
                    typeof(EventAggregator),
                    true);
                ILGenerator il = dynamicDelegate.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, method);
                il.Emit(OpCodes.Ret);

                _weakDelegates[method] = (WeakDelegate)dynamicDelegate.CreateDelegate(typeof(WeakDelegate));
            }

            return _weakDelegates[method];
        }

        private Action<object> TryGetWrapper<T>(Action<T> action)
        {
            if (_registrations.ContainsKey(action.Method))
            {
                foreach (WeakReference reference in _registrations[action.Method].Keys)
                {
                    if (reference.Target == (action.Method.IsStatic ? action.Method.DeclaringType : action.Target))
                    {
                        return _registrations[action.Method][reference];
                    }
                }
            }

            return null;
        }

        private Action<object> GetWrapper(WeakDelegate weakDelegate, MethodInfo method, WeakReference reference)
        {
            return (arg) =>
            {
                object target = reference.Target;
                if (target != null)
                {
                    weakDelegate(target, arg);
                }
                else
                {
                    Clean(method, reference);
                }
            };
        }

        private Action<object> GetWrapper<T>(Action<T> action, ExecutionMode executionMode)
        {
            Action<object> ret = TryGetWrapper(action);

            if (ret == null)
            {
                _registrations[action.Method] = new Dictionary<WeakReference, Action<object>>();

                WeakDelegate weakDelegate = GetWeakDelegate(action.Method, action.Method.DeclaringType);

                WeakReference key;
                Action<object> weakAction = null;
                if (action.Method.IsStatic)
                {
                    key = new WeakReference(action.Method.DeclaringType);

                    weakAction = (arg) => weakDelegate(null, arg);
                }
                else
                {
                    key = new WeakReference(action.Target);

                    weakAction = GetWrapper(weakDelegate, action.Method, key);
                }

                switch (executionMode)
                {
                    case ExecutionMode.Direct:
                        ret = weakAction;
                        break;

                    case ExecutionMode.Async:
                        ret = (o) => Task.Factory.StartNew(weakAction, o);
                        _firstActions.Add(ret);
                        break;

                    case ExecutionMode.LongRunning:
                        ret = (o) => Task.Factory.StartNew(weakAction, o, TaskCreationOptions.LongRunning);
                        _firstActions.Add(ret);
                        break;

                    case ExecutionMode.Context:
                        SendOrPostCallback callback = new SendOrPostCallback(weakAction);
                        ret = (o) => _context.Send(callback, o);
                        break;

                    case ExecutionMode.ContextAsync:
                        callback = new SendOrPostCallback(weakAction);
                        ret = (o) => _context.Post(callback, o);
                        _firstActions.Add(ret);
                        break;
                }

                _registrations[action.Method][key] = ret;
            }

            return ret;
        }

        private void Clean(MethodInfo method, WeakReference key)
        {
            Action<object> action = null;
            lock (_locker)
            {
                if (_registrations.ContainsKey(method) && _registrations[method].ContainsKey(key))
                {
                    action = _registrations[method][key];
                }

                if (action != null)
                {
                    _registrations[method].Remove(key);
                    if (_registrations[method].Count == 0)
                    {
                        _registrations.Remove(method);
                    }
                    _firstActions.Remove(action);

                    foreach (Type type in _relayTypes[method.GetParameters()[0].ParameterType])
                    {
                        typeof(Publisher<>).MakeGenericType(type).GetMethod("Unsubscribe").Invoke(null, new object[] { _id, action });
                    }
                }
            }
        }

        #endregion

        #region IPublisher

        IDisposable IPublisher.Subscribe<T>(Action<T> action, ExecutionMode executionMode)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }
            if (action == null)
            {
                throw new ArgumentNullException(Logging.GetMemberName(() => action));
            }

            lock (_locker)
            {
                Publisher<T>.Add(_id);

                Action<object> wrapper = GetWrapper(action, executionMode);

                foreach (Type type in _relayTypes[typeof(T)])
                {
                    typeof(Publisher<>).MakeGenericType(type).GetMethod("Subscribe").Invoke(null, new object[] { _id, wrapper, IsFirst(executionMode) });
                }

                return new Subscription<T>(_id, wrapper);
            }
        }

        void IPublisher.Publish<T>(T arg)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }

            Action<object> action = Publisher<T>.Actions[_id];
            if (action != null)
            {
                action(arg);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            foreach (Type type in _subTypes.Keys)
            {
                typeof(Publisher<>).MakeGenericType(type).GetMethod("Clear").Invoke(null, new object[] { _id });
            }
            _registrations.Clear();

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
