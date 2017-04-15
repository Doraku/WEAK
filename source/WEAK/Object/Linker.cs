using System;
using System.Reflection;
using WEAK.Helper;

namespace WEAK.Object
{
    /// <summary>
    /// Provides a set of methods to associated concrete implementation to interface or abstract class and create instance from those base types using the Factory class.
    /// </summary>
    /// <typeparam name="T">The abstract or interface type.</typeparam>
    public static class Linker<T>
    {
        #region Fields

        private static readonly object _locker;

        private static bool _isSingleton;
        private static Type _concreteType = null;
        private static MethodInfo _factoryMethod;
        private static MethodInfo _referencesMethod;
        private static Lazy<T> _singleton;

        #endregion

        #region Initialisation

        static Linker()
        {
            _locker = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to register an implementation type.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type.</typeparam>
        /// <param name="isSingleton">A value to indicate if the resolved type should be a singleton or not.</param>
        /// <returns>true if the concrete type was correctly registered, false if an other type was already registered.</returns>
        /// <exception cref="System.InvalidOperationException">The type is not an interface or an abstract class.</exception>
        public static bool TryRegister<TConcrete>(bool isSingleton)
            where TConcrete : class, T
        {
            if (!typeof(T).IsAbstract)
            {
                throw new InvalidOperationException(string.Format("Type \"{0}\" is not an interface or an abstract class.", typeof(T)));
            }

            bool ret = false;

            lock (_locker)
            {
                if (_concreteType == null)
                {
                    _isSingleton = isSingleton;
                    _concreteType = typeof(TConcrete);
                    if (_isSingleton)
                    {
                        _singleton = new Lazy<T>(() => (T)typeof(Singleton<>).MakeGenericType(_concreteType).GetProperty("Instance").GetValue(null), true);
                    }
                    else
                    {
                        _factoryMethod = typeof(Factory<>).MakeGenericType(_concreteType).GetMethod("CreateInstance");
                        _referencesMethod = typeof(ReferenceManager<,>).MakeGenericType(typeof(string), _concreteType).GetMethod("GetOrCreate", new[] { typeof(string) });
                    }

                    ret = true;
                }
            }

            return ret;
        }

        /// <summary>
        /// Tries to register an implementation type not as a singleton.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type.</typeparam>
        /// <returns>true if the concrete type was correctly registered, false if an other type was already registered.</returns>
        /// <exception cref="System.InvalidOperationException">The type is not an interface or an abstract class.</exception>
        public static bool TryRegister<TConcrete>()
            where TConcrete : class, T
        {
            return TryRegister<TConcrete>(false);
        }

        /// <summary>
        /// Register an implementation type.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type.</typeparam>
        /// <param name="isSingleton">A value to indicate if the resolved type should be a singleton or not.</param>
        /// <exception cref="System.InvalidOperationException">The type is not an interface or an abstract class or the type has already been mapped.</exception>
        public static void Register<TConcrete>(bool isSingleton)
            where TConcrete : class, T
        {
            if (!TryRegister<TConcrete>(isSingleton))
            {
                throw new InvalidOperationException(string.Format("Type \"{0}\" has already been mapped to type \"{1}\".", typeof(T), _concreteType));
            }
        }


        /// <summary>
        /// Register an implementation type not as a singleton.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type.</typeparam>
        /// <exception cref="System.InvalidOperationException">The type is not an interface or an abstract class or the type has already been mapped.</exception>
        public static void Register<TConcrete>()
            where TConcrete : class, T
        {
            Register<TConcrete>(false);
        }

        /// <summary>
        /// Resolves the type with its concrete type and return an instance.
        /// </summary>
        /// <returns>An instance of the type.</returns>
        /// <exception cref="System.InvalidOperationException">The type is not an interface or an abstract class
        /// or the type has not been mapped yet
        /// or the concrete type does not have a suitable constructor
        /// or a cyclic reference has been detected.</exception>
        public static T Resolve()
        {
            if (!typeof(T).IsAbstract)
            {
                throw new InvalidOperationException(string.Format("Type \"{0}\" is not an interface or an abstract class.", typeof(T)));
            }
            if (_concreteType == null)
            {
                throw new InvalidOperationException(string.Format("Type \"{0}\" is not yet mapped", typeof(T)));
            }

            return _isSingleton ? _singleton.Value : (T)_factoryMethod.Invoke(null, new object[0]);
        }

        /// <summary>
        /// Resolves the type with its concrete type and return an instance based on the provided key.
        /// </summary>
        /// <param name="key">The key of the instance.</param>
        /// <returns>An instance of the type.</returns>
        /// <exception cref="System.InvalidOperationException">The type is not an interface or an abstract class
        /// or the type has not been mapped yet
        /// or the concrete type does not have a suitable constructor
        /// or a cyclic reference has been detected.</exception>
        /// <exception cref="System.ArgumentException">key is null or empty.</exception>
        public static T Resolve(string key)
        {
            if (!typeof(T).IsAbstract)
            {
                throw new InvalidOperationException(string.Format("Type \"{0}\" is not an interface or an abstract class.", typeof(T)));
            }
            if (_concreteType == null)
            {
                throw new InvalidOperationException(string.Format("Type \"{0}\" is not yet mapped", typeof(T)));
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(key));
            }

            if (_isSingleton)
            {
                return _singleton.Value;
            }

            return (T)_referencesMethod.Invoke(null, new object[] { key });
        }

        #endregion
    }
}
