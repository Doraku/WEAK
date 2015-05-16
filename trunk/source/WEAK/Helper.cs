using System;
using System.Linq.Expressions;

namespace WEAK
{
    /// <summary>
    /// Provides a set of static methods to get extra informations automatically to reduce typo.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Gets the member name of an expression, useful to get the name of a variable, field or property.
        /// </summary>
        /// <typeparam name="T">The type of the member.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>The member name of the expression.</returns>
        /// <exception cref="System.ArgumentNullException">expression is null.</exception>
        /// <exception cref="System.ArgumentException">expression Body property is not a MemberExpression or its Member property is null.</exception>
        public static string GetMemberName<T>(Expression<Func<T>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(GetMemberName(() => expression));
            }

            MemberExpression memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null
                || memberExpression.Member == null)
            {
                throw new ArgumentException("expression Body property is not a MemberExpression or its Member property is null.", GetMemberName(() => expression));
            }

            return memberExpression.Member.Name;
        }
    }
}
