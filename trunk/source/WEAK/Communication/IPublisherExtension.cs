using System;
using System.Collections.Generic;
using System.Reflection;

namespace WEAK.Communication
{
    /// <summary>
    /// Provides set of static methods to automatically subscribe or unsubscribe methods marked with the AutoHookUpAttribute on a IPublisher instance.
    /// </summary>
    public static class IPublisherExtension
    {
        #region Methods

        /// <summary>
        /// Subscribes automatically static methods of a Type marked with the AutoHookUpAttribute on a IPublisher instance.
        /// </summary>
        /// <param name="publisher">The IPublisher instance.</param>
        /// <param name="type">The Type.</param>
        /// <exception cref="System.ArgumentNullException">publisher or type is null.</exception>
        /// <exception cref="System.ArgumentException">The AutoHookUp attribute is used on an uncompatible method of the Type.</exception>
        public static IEnumerable<IDisposable> HookUp(this IPublisher publisher, Type type)
        {
            if (publisher == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => publisher));
            }
            if (type == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => type));
            }

            List<IDisposable> ret = new List<IDisposable>();

            while (type != null)
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    AutoHookUpAttribute attribute = Attribute.GetCustomAttribute(method, typeof(AutoHookUpAttribute)) as AutoHookUpAttribute;
                    if (attribute != null)
                    {
                        if (method.GetParameters().Length != 1)
                        {
                            throw new ArgumentException(string.Format("Can't apply AutoHookUpAttribute to method \"{0}\".", method.Name));
                        }

                        Type argType = method.GetParameters()[0].ParameterType;
                        ret.Add((IDisposable)typeof(IPublisher).GetMethod("Subscribe").MakeGenericMethod(argType).Invoke(
                            publisher,
                            new object[]
                            {
                                Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(argType), method),
                                attribute.PublishingMode
                            }));
                    }
                }

                type = type.BaseType;
            }

            return ret;
        }

        /// <summary>
        /// Subscribes automatically methods of an instance and its Type marked with the AutoHookUpAttribute on a IPublisher instance.
        /// </summary>
        /// <param name="publisher">The IPublisher instance.</param>
        /// <param name="target">The instance.</param>
        /// <exception cref="System.ArgumentNullException">publisher or target is null.</exception>
        /// <exception cref="System.ArgumentException">The AutoHookUp attribute is used on an uncompatible method of the instance.</exception>
        public static IEnumerable<IDisposable> HookUp(this IPublisher publisher, object target)
        {
            if (publisher == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => publisher));
            }
            if (target == null)
            {
                throw new ArgumentNullException(Helper.GetMemberName(() => target));
            }

            List<IDisposable> ret = new List<IDisposable>();

            Type type = target.GetType();
            while (type != null)
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    AutoHookUpAttribute attribute = Attribute.GetCustomAttribute(method, typeof(AutoHookUpAttribute)) as AutoHookUpAttribute;
                    if (attribute != null)
                    {
                        if (method.GetParameters().Length != 1)
                        {
                            throw new ArgumentException(string.Format("Can't apply AutoHookUpAttribute to method \"{0}\".", method.Name));
                        }

                        Type argType = method.GetParameters()[0].ParameterType;
                        ret.Add((IDisposable)typeof(IPublisher).GetMethod("Subscribe").MakeGenericMethod(argType).Invoke(
                            publisher,
                            new object[]
                            {
                                Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(argType), target, method),
                                attribute.PublishingMode
                            }));
                    }
                }

                type = type.BaseType;
            }

            ret.AddRange(HookUp(publisher, target.GetType()));

            return ret;
        }

        #endregion
    }
}
