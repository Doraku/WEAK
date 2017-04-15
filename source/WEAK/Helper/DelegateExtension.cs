using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WEAK.Helper
{
    /// <summary>
    /// Provides extension method expected to be used on Delegate.
    /// </summary>
    public static class DelegateExtension
    {
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
            typeof(TDelegate).CheckForArgumentException(nameof(delegateAction), typeof(Delegate).IsAssignableFrom, "Parameter is not a Delegate type");

            Delegate unboxedDelegate = delegateAction as Delegate;

            if (unboxedDelegate == null)
            {
                return delegateAction;
            }

            Delegate weakAction = null;

            foreach (Delegate child in unboxedDelegate.GetInvocationList())
            {
                if (child.Method.IsStatic)
                {
                    weakAction = Delegate.Combine(weakAction, child);
                }
                else
                {
                    Type targetType = child.Target.GetType();

                    targetType.CheckForArgumentException(nameof(delegateAction), t => !t.IsValueType, "Delegate contains method from a value type");

                    ParameterExpression targetVariable = Expression.Variable(targetType);

                    MethodInfo factoryMethod = typeof(DelegateExtension)
                        .GetMethod(nameof(GetTryGetTargetExpression), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(targetType);

                    List<ParameterExpression> parameters = child.Method.GetParameters().Select(p => Expression.Parameter(p.ParameterType)).ToList();
                    Expression<TDelegate> labmdaInstance;
                    Expression getTargetExpression = factoryMethod.Invoke(null, new object[] { child.Target, targetVariable }) as Expression;

                    if (child.Method.ReturnType == typeof(void))
                    {
                        labmdaInstance = Expression.Lambda<TDelegate>(
                            Expression.Block(
                                new[] { targetVariable },
                                Expression.IfThen(
                                    getTargetExpression,
                                    Expression.Call(targetVariable, child.Method, parameters))),
                            parameters);
                    }
                    else
                    {
                        labmdaInstance = Expression.Lambda<TDelegate>(
                            Expression.Block(
                                child.Method.ReturnType,
                                new[] { targetVariable },
                                Expression.Condition(
                                    getTargetExpression,
                                    Expression.Call(targetVariable, child.Method, parameters),
                                    Expression.Default(child.Method.ReturnType))),
                            parameters);
                    }

                    weakAction = Delegate.Combine(weakAction, labmdaInstance.Compile() as Delegate);
                }
            }

            return weakAction as TDelegate;
        }

        #endregion
    }
}
