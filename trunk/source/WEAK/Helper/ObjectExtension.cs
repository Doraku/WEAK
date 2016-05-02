﻿using System;

namespace WEAK.Helper
{
    /// <summary>
    /// Provides extension methods on Object.
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// Checks and throws an ArgumentNullException if the passed parameter is null.
        /// </summary>
        /// <typeparam name="T">The type of the param to check.</typeparam>
        /// <param name="param">The param to check.</param>
        /// <param name="paramName">The name of the param to check.</param>
        /// <exception cref="ArgumentNullException">param is null.</exception>
        public static void CheckParameter<T>(this T param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Checks and throws an ArgumentNullException if the passed parameter is null
        /// or an ArgumentException if it doesn't passed the validation predicate.
        /// </summary>
        /// <typeparam name="T">The type of the param to check.</typeparam>
        /// <param name="param">The param to check.</param>
        /// <param name="paramName">The name of the param to check.</param>
        /// <param name="validation">The predicate to  check the param against.</param>
        /// <param name="message">The message used for the ArgumentException.</param>
        /// <exception cref="ArgumentNullException">param is null.</exception>
        /// <exception cref="ArgumentException">param does not check the validation predicate.</exception>
        public static void CheckParameter<T>(this T param, string paramName, Predicate<T> validation, string message)
        {
            param.CheckParameter(paramName);

            if (!validation?.Invoke(param) ?? false)
            {
                throw new ArgumentException(message, paramName);
            }
        }
    }
}