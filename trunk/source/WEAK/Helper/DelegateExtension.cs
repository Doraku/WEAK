using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WEAK.Helper
{
    /// <summary>
    /// Provides extension method expected to be used on Delegate.
    /// </summary>
    public static class DelegateExtension
    {
        #region Fields

        private static readonly ConcurrentDictionary<MethodInfo, ConditionalWeakTable<object, Delegate>> _delegates;

        #endregion

        #region Initialisation

        static DelegateExtension()
        {
            _delegates = new ConcurrentDictionary<MethodInfo, ConditionalWeakTable<object, Delegate>>();
        }

        #endregion

        #region Methods

        private static Expression GetTryGetTargetExpression<T>(T target, ParameterExpression targetVariable)
            where T : class
        {
            WeakReference<T> weakTarget = new WeakReference<T>(target);

            return Expression.Call(
                Expression.Constant(weakTarget, typeof(WeakReference<T>)),
                typeof(WeakReference<T>).GetMethod(nameof(weakTarget.TryGetTarget)),
                targetVariable);
        }
        /// <summary>
        /// Creates a wrap around a delegate, removing strong reference to its target so the delegate would not keep the object alive.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="delegateAction">The delegate to wrap.</param>
        /// <returns>The weak delegate.
        /// If delegate action was null, not a delegate, a delegate of a static method or a value type, it will simply return the object delegateAction.</returns>
        public static TDelegate ToWeak<TDelegate>(this TDelegate delegateAction)
            where TDelegate : class
        {
            Delegate unboxedDelegate = delegateAction as Delegate;
            if (unboxedDelegate == null
                || unboxedDelegate.Method.IsStatic
                || unboxedDelegate.Target.GetType().IsValueType
                || unboxedDelegate.GetInvocationList().Length > 1)
            {
                return delegateAction;
            }

            ConditionalWeakTable<object, Delegate> delegates =
                _delegates.GetOrAdd(unboxedDelegate.Method, _ => new ConditionalWeakTable<object, Delegate>());

            lock (delegates)
            {
                Delegate cachedAction;
                if (delegates.TryGetValue(unboxedDelegate.Target, out cachedAction))
                {
                    return cachedAction as TDelegate;
                }

                ParameterExpression targetVariable = Expression.Variable(unboxedDelegate.Target.GetType());

                MethodInfo factoryMethod = typeof(DelegateExtension)
                    .GetMethod(nameof(GetTryGetTargetExpression), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(unboxedDelegate.Target.GetType());

                List<ParameterExpression> parameters = unboxedDelegate.Method.GetParameters().Select(p => Expression.Parameter(p.ParameterType)).ToList();
                Expression<TDelegate> labmdaInstance;
                Expression getTargetExpression = factoryMethod.Invoke(null, new object[] { unboxedDelegate.Target, targetVariable }) as Expression;

                if (unboxedDelegate.Method.ReturnType == typeof(void))
                {
                    labmdaInstance = Expression.Lambda<TDelegate>(
                        Expression.Block(
                            new[] { targetVariable },
                            Expression.IfThen(
                                getTargetExpression,
                                Expression.Call(targetVariable, unboxedDelegate.Method, parameters))),
                        parameters);
                }
                else
                {
                    labmdaInstance = Expression.Lambda<TDelegate>(
                        Expression.Block(
                            unboxedDelegate.Method.ReturnType,
                            new[] { targetVariable },
                            Expression.Condition(
                                getTargetExpression,
                                Expression.Call(targetVariable, unboxedDelegate.Method, parameters),
                                Expression.Default(unboxedDelegate.Method.ReturnType))),
                        parameters);
                }

                TDelegate weakAction = labmdaInstance.Compile();

                delegates.Add(unboxedDelegate.Target, weakAction as Delegate);

                return weakAction;
            }
        }

        #endregion
    }
}
