using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace WEAK.Communication
{
    /// <summary>
    /// Provides a base implementation of the IPublisher interface
    /// using a SynchronizationContext for the Context RequestPublishingMode,
    /// Task for Asynch RequestPublishingMode
    /// and Task with LongRunning for LongRunning RequestPublishingMode.
    /// Disposing an instance of EventAggregator will clear all its subscriptions 
    /// but it will not stop running asynchrone tasks.
    /// </summary>
    public sealed class EventAggregator : IPublisher, IDisposable
    {
        #region Types

        private delegate void WeakDelegate(object source, object arg);

        private static class Publisher<A>
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
                    RegisterType(typeof(A));
                }
            }

            #endregion

            #region Methods

            public static void Subscribe(int id, Action<object> action)
            {
                Action<object> temp = Actions[id];
                if (temp != null && temp != action
                    && !temp.GetInvocationList().Contains(action))
                {
                    action = (Action<object>)Delegate.Combine(temp, action);
                }

                Actions[id] = action;
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

        #endregion

        #region Fields

        private static readonly object _locker;
        private static readonly List<WeakReference<EventAggregator>> _instances;
        private static readonly Dictionary<Type, LinkedList<Type>> _subTypes;
        private static readonly Dictionary<Type, LinkedList<Type>> _relayTypes;
        private static readonly Dictionary<MethodInfo, WeakDelegate> _weakDelegates;

        private readonly SynchronizationContext _context;
        private readonly Dictionary<MethodInfo, Dictionary<WeakReference, Action<object>>> _registrations;

        private int _id;
        private volatile bool _isDisposed = false;

        #endregion

        #region Initialisation

        static EventAggregator()
        {
            _locker = new object();
            _instances = new List<WeakReference<EventAggregator>>();
            _subTypes = new Dictionary<Type, LinkedList<Type>>();
            _relayTypes = new Dictionary<Type, LinkedList<Type>>();
            _weakDelegates = new Dictionary<MethodInfo, WeakDelegate>();

            _subTypes[typeof(IRequest)] = new LinkedList<Type>();
            _subTypes[typeof(IRequest)].AddLast(typeof(IRequest));
            _relayTypes[typeof(IRequest)] = new LinkedList<Type>();
            _relayTypes[typeof(IRequest)].AddLast(typeof(IRequest));
        }

        /// <summary>
        /// Initializes a new instance of the WEAK.Communication.EventAggregator class.
        /// </summary>
        /// <param name="context">The SynchronizationContext to use for Context RequestPublishingMode.</param>
        /// <exception cref="ArgumentNullException">context is null.</exception>
        public EventAggregator(SynchronizationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => context));
            }

            _context = context;
            _registrations = new Dictionary<MethodInfo, Dictionary<WeakReference, Action<object>>>();

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
            (this as IDisposable).Dispose();
        }

        #endregion

        #region Methods

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
            if (_subTypes.ContainsKey(type) ||
                !type.GetInterfaces().Contains(typeof(IRequest)))
            {
                return;
            }

            _subTypes[type] = new LinkedList<Type>();
            _subTypes[type].AddLast(type);
            _subTypes[type].AddLast(typeof(IRequest));

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
                        if (!reference.TryGetTarget(out publisher) || publisher == null)
                        {
                            continue;
                        }

                        List<Action<object>> actions = (List<Action<object>>)typeof(Publisher<>).MakeGenericType(sub).GetField("Actions").GetValue(null);
                        if (actions.Count > publisher._id)
                        {
                            typeof(Publisher<>).MakeGenericType(type).GetMethod("Subscribe").Invoke(null, new object[] { publisher._id, actions[publisher._id] });
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
                    "weak_" + method.Name,
                    typeof(void),
                    new Type[] { typeof(object), typeof(object) },
                    targetType,
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

        private Action<object> GetWrapper<T>(Action<T> action)
            where T : IRequest
        {
            WeakReference key;
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
            else
            {
                _registrations[action.Method] = new Dictionary<WeakReference, Action<object>>();
            }

            Action<object> weakAction = null;
            if (action.Method.IsStatic)
            {
                key = new WeakReference(action.Method.DeclaringType);

                weakAction = (arg) => action((T)arg);
            }
            else
            {
                key = new WeakReference(action.Target);

                weakAction = GetWrapper(GetWeakDelegate(action.Method, action.Target.GetType()), action.Method, key);
            }

            _registrations[action.Method][key] = weakAction;

            return weakAction;
        }

        private void Clean(MethodInfo method, WeakReference key)
        {
            Action<IRequest> action = null;
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

                    foreach (Type type in _relayTypes[method.GetParameters()[0].ParameterType])
                    {
                        typeof(Publisher<>).MakeGenericType(type).GetMethod("Unsubscribe").Invoke(null, new object[] { _id, action });
                    }
                }
            }
        }

        #endregion

        #region IPublisher

        void IPublisher.Subscribe<T>(Action<T> action)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }
            if (action == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => action));
            }

            lock (_locker)
            {
                Publisher<T>.Add(_id);

                Action<object> wrapper = GetWrapper(action);
                foreach (Type type in _relayTypes[typeof(T)])
                {
                    typeof(Publisher<>).MakeGenericType(type).GetMethod("Subscribe").Invoke(null, new object[] { _id, wrapper });
                }
            }
        }

        void IPublisher.Unsubscribe<T>(Action<T> action)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }
            if (action == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => action));
            }

            lock (_locker)
            {
                Publisher<T>.Add(_id);

                Action<IRequest> wrapper = GetWrapper(action);
                foreach (Type type in _relayTypes[typeof(T)])
                {
                    typeof(Publisher<>).MakeGenericType(type).GetMethod("Unsubscribe").Invoke(null, new object[] { _id, wrapper });
                }
            }
        }

        void IPublisher.Publish<T>(T request)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Resource was disposed.");
            }
            if (request == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => request));
            }

            Action<object> action = Publisher<T>.Actions[_id];
            if (action != null)
            {
                switch (request.PulishingMode)
                {
                    case RequestPublishingMode.Direct:
                        action(request);
                        break;

                    case RequestPublishingMode.Async:
                        Task.Factory.StartNew(action, request);
                        break;

                    case RequestPublishingMode.Context:
                        _context.Send(new SendOrPostCallback(action), request);
                        break;

                    case RequestPublishingMode.LongRunning:
                        Task.Factory.StartNew(action, request, TaskCreationOptions.LongRunning);
                        break;
                }
            }
        }

        #endregion

        #region IDisposable

        void IDisposable.Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            foreach (Type type in _subTypes.Keys)
            {
                typeof(Publisher<>).MakeGenericType(type).GetMethod("Clear").Invoke(null, new object[] { _id });
            }
            _registrations.Clear();

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
