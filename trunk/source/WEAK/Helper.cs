using System;
using System.Linq.Expressions;

namespace WEAK
{
    public static class Helper
    {
        public static string GetMemberName<T>(Expression<Func<T>> expression)
        {
            return (expression.Body as MemberExpression).Member.Name;
        }
    }
}
