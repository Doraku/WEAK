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

        private static readonly Lazy<Func<T>> _factory;

        [ThreadStatic]
        private static bool _isInitialising;

        #endregion

        #region Initialisation

        static Factory()
        {
            _isInitialising = false;

            if (!typeof(T).IsAbstract)
            {
                _factory = new Lazy<Func<T>>(
                    () =>
                    {
                        ConstructorInfo constructor = typeof(T).GetConstructors().OrderBy(i => i.GetParameters().Length).FirstOrDefault();
                        if (constructor == null)
                        {
                            throw new InvalidOperationException(string.Format("No suitable constructor found for type \"{0}\".", typeof(T)));
                        }

                        List<Func<object>> parameters = new List<Func<object>>();

                        foreach (ParameterInfo info in constructor.GetParameters())
                        {
                            KeyAttribute attribute = info.GetCustomAttribute<KeyAttribute>();
                            MethodInfo creator;
                            object[] innerParameters;
                            if (attribute == null)
                            {
                                creator = typeof(Factory<>).MakeGenericType(info.ParameterType).GetMethod("CreateInstance", Type.EmptyTypes);
                                innerParameters = new object[0];
                            }
                            else
                            {
                                creator = typeof(ReferenceManager<,>).MakeGenericType(typeof(string), info.ParameterType).GetMethod("GetOrCreate", new[] { typeof(string) });
                                innerParameters = new object[] { attribute.Value };
                            }

                            parameters.Add(() => creator.Invoke(null, innerParameters));
                        }

                        return () => (T)constructor.Invoke(parameters.Select(i => i()).ToArray());
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

            if (_factory == null)
            {
                ret = Linker<T>.Resolve();
            }
            else
            {
                try
                {
                    _isInitialising = true;

                    ret = _factory.Value();
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
