using System;
using System.Reflection;

namespace WEAK.Communication
{
    public static class IPublisherExtension
    {
        #region Methods

        public static void HookUp(this IPublisher publisher, Type type)
        {
            if (publisher == null)
            {
                return;
            }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (Attribute.GetCustomAttribute(method, typeof(AutoHookUpAttribute)) != null)
                {
                    if (method.GetParameters().Length != 1
                        || !typeof(IRequest).IsAssignableFrom(method.GetParameters()[0].ParameterType))
                    {
                        throw new Exception(string.Format("Can't apply attribute AutoSubscribe to method \"{0}\".", method.Name));
                    }

                    Type argType = method.GetParameters()[0].ParameterType;
                    typeof(IPublisher).GetMethod("Subscribe").MakeGenericMethod(argType).Invoke(
                        publisher,
                        new object[] { Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(argType), method) });
                }
            }
        }

        public static void HookUp(this IPublisher publisher, object target)
        {
            if (publisher == null || target == null)
            {
                return;
            }

            foreach (MethodInfo method in target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (Attribute.GetCustomAttribute(method, typeof(AutoHookUpAttribute)) != null)
                {
                    if (method.GetParameters().Length != 1
                        || !typeof(IRequest).IsAssignableFrom(method.GetParameters()[0].ParameterType))
                    {
                        throw new Exception(string.Format("Can't apply attribute AutoHookUp to method \"{0}\".", method.Name));
                    }

                    Type argType = method.GetParameters()[0].ParameterType;
                    typeof(IPublisher).GetMethod("Subscribe").MakeGenericMethod(argType).Invoke(
                        publisher,
                        new object[] { Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(argType), target, method) });
                }
            }

            HookUp(publisher, target.GetType());
        }

        public static void UnHookUp(this IPublisher publisher, Type type)
        {
            if (publisher == null)
            {
                return;
            }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (Attribute.GetCustomAttribute(method, typeof(AutoHookUpAttribute)) != null)
                {
                    Type argType = method.GetParameters()[0].ParameterType;
                    typeof(IPublisher).GetMethod("Unsubscribe").MakeGenericMethod(argType).Invoke(
                        publisher,
                        new object[] { Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(argType), method) });
                }
            }
        }

        public static void UnHookUp(this IPublisher publisher, object target)
        {
            if (publisher == null)
            {
                return;
            }

            foreach (MethodInfo method in target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (Attribute.GetCustomAttribute(method, typeof(AutoHookUpAttribute)) != null)
                {
                    Type argType = method.GetParameters()[0].ParameterType;
                    typeof(IPublisher).GetMethod("Unsubscribe").MakeGenericMethod(argType).Invoke(
                        publisher,
                        new object[] { Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(argType), target, method) });
                }
            }

            UnHookUp(publisher, target.GetType());
        }

        #endregion
    }
}
