using System;
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

        private static readonly Dictionary<MethodInfo, ConditionalWeakTable<object, Delegate>> _delegates;
        private static readonly object _locker;

        #endregion

        #region Initialisation

        static DelegateExtension()
        {
            _delegates = new Dictionary<MethodInfo, ConditionalWeakTable<object, Delegate>>();
            _locker = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a wrap around a delegate, removing strong reference to its target so the delegate would not keep the object alive.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="delegateAction">The delegate to wrap.</param>
        /// <returns>The weak delegate.
        /// If delegate action was null, not a delegate, or a delegate of a static method, it will simply return the object delegateAction.</returns>
        public static TDelegate ToWeak<TDelegate>(this TDelegate delegateAction)
            where TDelegate : class
        {
            Delegate unboxedDelegate = delegateAction as Delegate;
            if (unboxedDelegate == null || unboxedDelegate.Method.IsStatic)
            {
                return delegateAction;
            }

            lock (_locker)
            {
                if (_delegates.ContainsKey(unboxedDelegate.Method))
                {
                    Delegate cachedAction;
                    if (_delegates[unboxedDelegate.Method].TryGetValue(unboxedDelegate.Target, out cachedAction))
                    {
                        return cachedAction as TDelegate;
                    }
                }
                else
                {
                    _delegates[unboxedDelegate.Method] = new ConditionalWeakTable<object, Delegate>();
                }

                ParameterExpression targetVariable = Expression.Variable(unboxedDelegate.Target.GetType());
                List<ParameterExpression> parameters = unboxedDelegate.Method.GetParameters().Select(p => Expression.Parameter(p.ParameterType)).ToList();
                WeakReference weakTarget = new WeakReference(unboxedDelegate.Target);
                Expression<TDelegate> labmdaInstance;
                BinaryExpression getTargetExpression =
                    Expression.Assign(
                        targetVariable,
                        Expression.Convert(
                            Expression.Call(
                                Expression.Constant(weakTarget, typeof(WeakReference)),
                                typeof(WeakReference).GetProperty(nameof(weakTarget.Target)).GetGetMethod()),
                            unboxedDelegate.Target.GetType()));

                if (unboxedDelegate.Method.ReturnType == typeof(void))
                {
                    labmdaInstance = Expression.Lambda<TDelegate>(
                        Expression.Block(
                            new[] { targetVariable },
                            getTargetExpression,
                            Expression.IfThen(
                                Expression.Not(Expression.Equal(targetVariable, Expression.Constant(null))),
                                Expression.Call(targetVariable, unboxedDelegate.Method, parameters))),
                        parameters);
                }
                else
                {
                    LabelTarget returnLabel = Expression.Label(unboxedDelegate.Method.ReturnType);
                    labmdaInstance = Expression.Lambda<TDelegate>(
                        Expression.Block(
                            unboxedDelegate.Method.ReturnType,
                            new[] { targetVariable },
                            getTargetExpression,
                            Expression.IfThen(
                                Expression.Not(Expression.Equal(targetVariable, Expression.Constant(null))),
                                Expression.Return(returnLabel, Expression.Call(targetVariable, unboxedDelegate.Method, parameters), unboxedDelegate.Method.ReturnType)),
                            Expression.Label(returnLabel, Expression.Default(unboxedDelegate.Method.ReturnType))),
                        parameters);
                }

                TDelegate weakAction = labmdaInstance.Compile();

                _delegates[unboxedDelegate.Method].Add(unboxedDelegate.Target, weakAction as Delegate);

                return weakAction;
            }
        }

        #endregion
    }
}
