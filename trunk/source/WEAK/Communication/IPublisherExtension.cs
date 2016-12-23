using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WEAK.Helper;

namespace WEAK.Communication
{
    /// <summary>
    /// Provides set of static methods to automatically subscribe or unsubscribe methods marked with the SubscribeAttribute on a IPublisher instance.
    /// </summary>
    public static class IPublisherExtension
    {
        #region Fields

        private const BindingFlags _defaultFlags =
           BindingFlags.Static
           | BindingFlags.NonPublic
           | BindingFlags.Public
           | BindingFlags.InvokeMethod
           | BindingFlags.DeclaredOnly;

        #endregion

        #region Methods

        private static IDisposable AutoSubscribe(IPublisher publisher, Type type, object target, BindingFlags flags)
        {
            List<IDisposable> subscriptions = new List<IDisposable>();

            while (type != null)
            {
                foreach (MethodInfo method in type.GetMethods(flags | _defaultFlags).Where(m => !m.IsAbstract))
                {
                    SubscribeAttribute attribute = method.GetCustomAttribute<SubscribeAttribute>(true);
                    if (attribute != null)
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length != 1)
                        {
                            throw new NotSupportedException($"Can't apply SubscribeAttribute to method \"{method.Name}\": incorrect number of parameters \"{parameters.Length}\".");
                        }
                        if (method.ReturnType != typeof(void))
                        {
                            throw new NotSupportedException($"Can't apply SubscribeAttribute to method \"{method.Name}\": return type must be void \"{method.ReturnType}\".");
                        }

                        Type argType = parameters[0].ParameterType;
                        subscriptions.Add((IDisposable)typeof(IPublisher).GetMethod(nameof(publisher.Subscribe)).MakeGenericMethod(argType).Invoke(
                            publisher,
                            new object[]
                            {
                                method.IsStatic ?
                                    Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(argType), method)
                                    : Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(argType), target, method),
                                attribute.PublishingMode
                            }));
                    }
                }

                type = type.BaseType;
            }

            return subscriptions.Merge();
        }

        /// <summary>
        /// Subscribes automatically static methods of a Type marked with the SubscribeAttribute on a IPublisher instance.
        /// </summary>
        /// <typeparam name="T">The Type.</typeparam>
        /// <param name="publisher">The IPublisher instance.</param>
        /// <returns>A IDisposable to unregister.</returns>
        /// <exception cref="System.ArgumentNullException">publisher or type is null.</exception>
        /// <exception cref="System.NotSupportedException">The Subscribe attribute is used on an uncompatible method of the instance.</exception>
        public static IDisposable AutoSubscribe<T>(this IPublisher publisher)
            where T : class
        {
            publisher.CheckForArgumentNullException(nameof(publisher));

            return AutoSubscribe(publisher, typeof(T), null, BindingFlags.Default);
        }

        /// <summary>
        /// Subscribes automatically methods of an instance and its Type marked with the SubscribeAttribute on a IPublisher instance.
        /// </summary>
        /// <typeparam name="T">The Type.</typeparam>
        /// <param name="publisher">The IPublisher instance.</param>
        /// <param name="target">The instance.</param>
        /// <returns>A IDisposable to unregister.</returns>
        /// <exception cref="System.ArgumentNullException">publisher or target is null.</exception>
        /// <exception cref="System.NotSupportedException">The Subscribe attribute is used on an uncompatible method of the instance.</exception>
        public static IDisposable AutoSubscribe<T>(this IPublisher publisher, T target)
            where T : class
        {
            publisher.CheckForArgumentNullException(nameof(publisher));
            target.CheckForArgumentNullException(nameof(target));

            return AutoSubscribe(publisher, target.GetType(), target, BindingFlags.Instance);
        }

        #endregion
    }
}
