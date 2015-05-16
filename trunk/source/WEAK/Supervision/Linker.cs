using System;
using System.Reflection;

namespace WEAK.Supervision
{
    public static class Linker<TInterface>
    {
        #region Fields

        private static bool _isSingleton;
        private static Type _concreteType = null;
        private static MethodInfo _factoryMethod;
        private static MethodInfo _referencesMethod;
        private static Lazy<TInterface> _singleton;

        #endregion

        #region Initialisation

        static Linker()
        {
            if (!typeof(TInterface).IsInterface)
            {
                throw new InvalidOperationException(string.Format("Type \"{0}\" is not an interface.", typeof(TInterface)));
            }
        }

        #endregion

        #region Methods

        public static void Register<TConcrete>(bool isSingleton)
            where TConcrete : class, TInterface
        {
            if (_concreteType != null)
            {
                throw new InvalidOperationException(string.Format("Interface type \"{0}\" has already been mapped to type \"{1}\".", typeof(TInterface), _concreteType));
            }

            _isSingleton = isSingleton;
            _concreteType = typeof(TConcrete);
            _factoryMethod = typeof(Factory<>).MakeGenericType(_concreteType).GetMethod("CreateInstance");
            if (_isSingleton)
            {
                _singleton = new Lazy<TInterface>(() => (TInterface)typeof(Singleton<>).MakeGenericType(_concreteType).GetProperty("Instance").GetValue(null), true);
            }
            else
            {
                _referencesMethod = typeof(ReferenceManager<,>).MakeGenericType(typeof(string), _concreteType).GetMethod("GetOrCreate", new[] { typeof(string), _concreteType.MakeByRefType() });
            }
        }

        public static void Register<TConcrete>()
            where TConcrete : class, TInterface
        {
            Register<TConcrete>(false);
        }

        public static TInterface Resolve()
        {
            if (_concreteType == null)
            {
                throw new Exception(string.Format("Interface type \"{0}\" is not yet mapped", typeof(TInterface)));
            }

            return _isSingleton ? _singleton.Value : (TInterface)_factoryMethod.Invoke(null, new object[0]);
        }

        public static TInterface Resolve(string key)
        {
            if (_concreteType == null)
            {
                throw new Exception(string.Format("Interface type \"{0}\" is not yet mapped", typeof(TInterface)));
            }

            if (_isSingleton)
            {
                return _singleton.Value;
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cannot be null or empty.", Helper.GetMemberName(() => key));
            }

            object[] ret = new object[] { key, null };
            _referencesMethod.Invoke(null, ret);

            return (TInterface)ret[1];
        }

        #endregion
    }
}
