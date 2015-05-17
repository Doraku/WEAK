using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WEAK.Object
{
    /// <summary>
    /// Provides instance of the given type using the first public constructor with the least number of parameters.
    /// Each parameter will be created with its own Factory of its type.
    /// Parameter of interface or abstract type will be tried to be resolved through the Linker class.
    /// If the type is abstract, creation of the instance will pass through the Linker class.
    /// </summary>
    /// <typeparam name="T">The type of the instances produced by the factory.</typeparam>
    public class Factory<T>
    {
        #region Fields

        private static readonly Lazy<ConstructorInfo> _constructorInfo;

        [ThreadStatic]
        private static bool _isInitialising;

        #endregion

        #region Initialisation

        static Factory()
        {
            _isInitialising = false;
            if (!typeof(T).IsAbstract)
            {
                _constructorInfo = new Lazy<ConstructorInfo>(
                    () =>
                    {
                        ConstructorInfo ret = typeof(T).GetConstructors().OrderBy(i => i.GetParameters().Length).FirstOrDefault();

                        if (ret == null)
                        {
                            throw new InvalidOperationException(string.Format("No suitable constructor found for type \"{0}\".", typeof(T)));
                        }

                        return ret;
                    },
                    true);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the factory type by using the first public constructor with the least number of parameters.
        /// Each parameter will be created with its own Factory of its type.
        /// Parameter of interface or abstract type will be tried to be resolved through the Linker class.
        /// If the type is abstract, creation of the instance will pass through the Linker class.
        /// </summary>
        /// <returns>The new instance.</returns>
        /// <exception cref="System.InvalidOperationException">A cyclic reference has been detected or a parameter of an interface of abstract type has not been mapped through the Linker class.</exception>
        public static T CreateInstance()
        {
            T ret;

            if (_isInitialising)
            {
                throw new InvalidOperationException(string.Format("Cyclic reference for type \"{0}\"", typeof(T)));
            }

            if (_constructorInfo == null)
            {
                ret = Linker<T>.Resolve();
            }
            else
            {
                try
                {
                    _isInitialising = true;

                    List<object> parameters = new List<object>();

                    foreach (ParameterInfo info in _constructorInfo.Value.GetParameters())
                    {
                        if (info.ParameterType.IsInterface
                            || info.ParameterType.IsAbstract)
                        {
                            parameters.Add(typeof(Linker<>).MakeGenericType(info.ParameterType).GetMethod("Resolve", new Type[0]).Invoke(null, new object[0]));
                        }
                        else
                        {
                            parameters.Add(typeof(Factory<>).MakeGenericType(info.ParameterType).GetMethod("CreateInstance", new Type[0]).Invoke(null, new object[0]));
                        }
                    }

                    ret = (T)_constructorInfo.Value.Invoke(parameters.ToArray());
                }
                catch (TargetInvocationException damn)
                {
                    throw damn.InnerException;
                }
                finally
                {
                    _isInitialising = false;
                }
            }

            return ret;
        }

        #endregion
    }
}
