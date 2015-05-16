using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace WEAK.Supervision
{
    public class Factory<T>
    {
        #region Fields

        private static readonly ConstructorInfo _constructorInfo;

        #endregion

        #region Initialisation

        static Factory()
        {
            _constructorInfo = typeof(T).GetConstructors().OrderBy(i => i.GetParameters().Length).FirstOrDefault();

            if (_constructorInfo == null)
            {
                throw new InvalidOperationException(string.Format("No suitable constructor found for type \"{0}\".", typeof(T)));
            }
        }

        #endregion

        #region Methods

        public static T CreateInstance()
        {
            T ret;

            using (ThreadLocal<List<Type>> local = new ThreadLocal<List<Type>>(() => new List<Type>()))
            {
                List<Type> initialisingTypes = local.Value;

                if (initialisingTypes.Contains(typeof(T)))
                {
                    throw new InvalidOperationException(string.Format("Cyclic reference for type \"{0}\"", typeof(T)));
                }

                initialisingTypes.Add(typeof(T));

                List<object> parameters = new List<object>();

                foreach (ParameterInfo info in _constructorInfo.GetParameters())
                {
                    if (info.ParameterType.IsInterface)
                    {
                        parameters.Add(typeof(Linker<>).MakeGenericType(info.ParameterType).GetMethod("Resolve").Invoke(null, new object[0]));
                    }
                    else
                    {
                        parameters.Add(typeof(Factory<>).MakeGenericType(info.ParameterType).GetMethod("CreateInstance").Invoke(null, new object[0]));
                    }
                }

                ret = (T)_constructorInfo.Invoke(parameters.ToArray());

                initialisingTypes.Remove(typeof(T));
            }

            return ret;
        }

        #endregion
    }
}
