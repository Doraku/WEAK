using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WEAK
{
    public static class DelegateExtension
    {
        #region Fields

        private static readonly Dictionary<MethodInfo, Dictionary<WeakReference, Delegate>> _delegates;

        #endregion

        #region Initialisation

        static DelegateExtension()
        {
            _delegates = new Dictionary<MethodInfo, Dictionary<WeakReference, Delegate>>();
        }

        #endregion

        #region Methods

        public static Delegate CleanWeak(this Delegate action)
        {
            if (action == null)
            {
                return null;
            }

            Delegate cleanedAction = null;

            foreach (Delegate del in action.GetInvocationList())
            {
                Closure closure = action.Target as Closure;
                if (closure != null && closure.Constants.Length == 1)
                {
                    WeakReference target = closure.Constants[0] as WeakReference;
                    if (target != null && !target.IsAlive)
                    {
                        continue;
                    }
                }
                cleanedAction = Delegate.Combine(cleanedAction, del);
            }

            return cleanedAction;
        }

        public static Delegate GetWeak(this Delegate action)
        {
            if (action == null)
            {
                return null;
            }

            if (action.Method.IsStatic)
            {
                return action;
            }

            WeakReference key;
            lock (_delegates)
            {
                if (_delegates.ContainsKey(action.Method))
                {
                    key = _delegates[action.Method].Keys.FirstOrDefault(w => w.Target == action.Target);
                    if (key != null)
                    {
                        return _delegates[action.Method][key];
                    }
                }
                else
                {
                    _delegates[action.Method] = new Dictionary<WeakReference, Delegate>();
                }

                ParameterExpression v = Expression.Variable(action.Target.GetType(), "v");
                List<ParameterExpression> parameters = new List<ParameterExpression>();
                foreach (ParameterInfo param in action.Method.GetParameters())
                {
                    parameters.Add(Expression.Parameter(param.ParameterType, "p" + parameters.Count.ToString()));
                }
                key = new WeakReference(action.Target);
                Delegate weakAction;
                object labmdaInstance;
                if (action.Method.ReturnType == typeof(void))
                {
                    labmdaInstance = typeof(Expression).GetMethods().Where(m => m.Name == "Lambda").Skip(2).First().MakeGenericMethod(action.GetType()).Invoke(null,
                        new object[] {
                            Expression.Block(
                                new[] { v },
                                Expression.Assign(v,
                                    Expression.Convert(
                                        Expression.Call(
                                            Expression.Constant(
                                                key, typeof(WeakReference)),
                                                typeof(WeakReference).GetProperty("Target").GetGetMethod()),
                                                action.Target.GetType())),
                                Expression.IfThen(Expression.Not(Expression.Equal(v, Expression.Constant(null))),
                                    Expression.Call(v, action.Method, parameters))),
                            parameters
                        });
                }
                else
                {
                    LabelTarget r = Expression.Label(action.Method.ReturnType, "r");
                    labmdaInstance = typeof(Expression).GetMethods().Where(m => m.Name == "Lambda").Skip(2).First().MakeGenericMethod(action.GetType()).Invoke(null,
                        new object[] {
                            Expression.Block(
                                action.Method.ReturnType,
                                new[] { v },
                                Expression.Assign(v,
                                    Expression.Convert(
                                        Expression.Call(
                                            Expression.Constant(
                                                key, typeof(WeakReference)),
                                                typeof(WeakReference).GetProperty("Target").GetGetMethod()),
                                                action.Target.GetType())),
                                Expression.IfThen(Expression.Not(Expression.Equal(v, Expression.Constant(null))),
                                    Expression.Return(r, Expression.Call(v, action.Method, parameters), action.Method.ReturnType)),
                                Expression.Return(r, Expression.Default(action.Method.ReturnType), action.Method.ReturnType),
                                Expression.Label(r, Expression.Default(action.Method.ReturnType))),
                            parameters
                        });
                }

                weakAction = (Delegate)typeof(Expression<>).MakeGenericType(action.GetType()).GetMethods().First(m => m.Name == "Compile").Invoke(labmdaInstance, new object[] { });
                _delegates[action.Method][key] = weakAction;
                return weakAction;
            }
        }

        #endregion
    }
}
