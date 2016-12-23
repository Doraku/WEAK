using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
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
    /// Disposing an instance of EventAggregator will clear all its subscriptions 
    /// but it will not stop running asynchrone tasks.
    /// </summary>
    public sealed partial class Publisher : IPublisher, IDisposable
    {
        #region Types

        private delegate void StrippedDelegate(object source, object arg);

        #endregion

        #region Fields

        private static readonly object _locker;
        private static readonly ConcurrentDictionary<Type, Action<StrongBox<Action<object>[]>>> _addRelay;
        private static readonly Dictionary<MethodInfo, StrippedDelegate> _strippedDelegates;
        private static readonly IntDispenser _idDispenser;

        private readonly int _id;
        private readonly SynchronizationContext _context;

        private int _isDisposed;

        #endregion

        #region Initialisation

        static Publisher()
        {
            _locker = new object();
            _addRelay = new ConcurrentDictionary<Type, Action<StrongBox<Action<object>[]>>>();
            _strippedDelegates = new Dictionary<MethodInfo, StrippedDelegate>();
            _idDispenser = new IntDispenser(-1);
        }

        /// <summary>
        /// Initializes a new instance of the WEAK.Communication.EventAggregator class.
        /// </summary>
        /// <param name="context">The SynchronizationContext to use for Context ExecutionMode.</param>
        /// <exception cref="ArgumentNullException">context is null.</exception>
        public Publisher(SynchronizationContext context)
        {
            context.CheckForArgumentNullException(nameof(context));

            _id = _idDispenser.GetFreeInt();
            _context = context;

            InnerPublisher<object>.Add(_id);

            _isDisposed = 0;
        }

        /// <summary>
        /// Initializes a new instance of the WEAK.Communication.EventAggregator class
        /// with the current SynchronizationContext or create one if null.
        /// </summary>
        public Publisher()
            : this(SynchronizationContext.Current ?? new SynchronizationContext())
        { }

        #endregion

        #region Methods

        private static IEnumerable<Type> GetTypes(Type type)
        {
            yield return type;

            foreach (Type inter in type.GetInterfaces())
            {
                yield return inter;
            }

            if (type.IsInterface)
            {
                yield return typeof(object);
            }
            else
            {
                while ((type = type.BaseType) != null)
                {
                    yield return type;
                }
            }
        }

        private static Action<StrongBox<Action<object>[]>> GetAddRelay(Type type)
        {
            ParameterExpression actions = Expression.Variable(typeof(StrongBox<Action<object>[]>));

            return Expression.Lambda<Action<StrongBox<Action<object>[]>>>(
                Expression.Call(
                    typeof(InnerPublisher<>).MakeGenericType(type).GetMethod(nameof(InnerPublisher<object>.AddRelay)),
                    actions),
                actions).Compile();
        }

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

        private static StrippedDelegate GetStrippedDelegate(MethodInfo method, Type targetType)
        {
            StrippedDelegate result;

            lock (_locker)
            {
                if (!_strippedDelegates.TryGetValue(method, out result))
                {
                    DynamicMethod dynamicDelegate = new DynamicMethod(
                        string.Format("weak_{0}", _strippedDelegates.Count),
                        typeof(void),
                        new[] { typeof(object), typeof(object) },
                        typeof(Publisher),
                        true);
                    ILGenerator il = dynamicDelegate.GetILGenerator();
                    if (!method.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                    }
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, method);
                    il.Emit(OpCodes.Ret);

                    result = (StrippedDelegate)dynamicDelegate.CreateDelegate(typeof(StrippedDelegate));
                    _strippedDelegates[method] = result;
                }
            }

            return result;
        }

        private static Action<object> GetWrapper(StrippedDelegate weakDelegate, WeakReference reference)
        {
            return arg =>
            {
                object target = reference.Target;

                if (target != null)
                {
                    weakDelegate(target, arg);
                }
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfIsDisposed()
        {
            if (_isDisposed != 0)
            {
                throw new ObjectDisposedException(nameof(Publisher));
            }
        }

        private Action<object> GetWrapper<T>(Action<T> action, ExecutionMode executionMode)
        {
            StrippedDelegate weakDelegate = GetStrippedDelegate(action.Method, action.Method.DeclaringType);

            Action<object> weakAction =
                action.Method.IsStatic
                ? arg => weakDelegate(null, arg)
                : GetWrapper(weakDelegate, new WeakReference(action.Target));
            SendOrPostCallback callback = new SendOrPostCallback(weakAction);

            switch (executionMode)
            {
                case ExecutionMode.Async:
                    return o => Task.Factory.StartNew(weakAction, o);

                case ExecutionMode.LongRunning:
                    return o => Task.Factory.StartNew(weakAction, o, TaskCreationOptions.LongRunning);

                case ExecutionMode.Context:
                    return o => _context.Send(callback, o);

                case ExecutionMode.ContextAsync:
                    return o => _context.Post(callback, o);

                default:
                    return weakAction;
            }
        }

        #endregion

        #region IPublisher

        public IDisposable Subscribe<T>(Action<T> action, ExecutionMode executionMode)
        {
            ThrowIfIsDisposed();

            action.CheckForArgumentNullException(nameof(action));

            Action<object> wrapper = GetWrapper(action, executionMode);

            InnerPublisher<T>.Subscribe(_id, wrapper, IsFirst(executionMode));

            return new Subscription(_id, wrapper);
        }

        public void Publish<T>(T arg)
        {
            ThrowIfIsDisposed();

            InnerPublisher<T>.Actions.Value[_id]?.Invoke(arg);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                InnerPublisher<object>.Clear(_id);
                _idDispenser.ReleaseInt(_id);

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
