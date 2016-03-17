#region Header

/*
Copyright 2015 Wim van der Vegt

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Namespace: Swiss
Filename: ObjectUtils.cs
*/

#endregion Header

namespace Swiss
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Linq.Expressions;

    /// <summary>
    /// See http://forums.asp.net/t/1321907.aspx
    /// 
    /// Usage: string pricePropertyName = ObjectUtils.GetMemberName&lt;IProduct&gt;(p =&gt; p.Price);
    /// 
    /// This work had no explicit license specified.
    /// </summary>
    public static class ObjectUtils
    {
        /// <summary>
        /// This does some magic, it returns the name of a property so it is no longer 
        /// a problematic string value problematic when refactoring.
        /// 
        /// Usage: string pricePropertyName = ObjectUtils.GetMemberName&lt;IProduct&gt;(p =&gt; p.Price);
        /// </summary>
        /// <typeparam name="T">The type to which the property belongs</typeparam>
        /// <param name="action">-</param>
        /// <returns>The property name</returns>
        public static string GetMemberName<T>(Expression<Func<T, object>> action)
        {
            var lambda = (LambdaExpression)action;

            if (lambda.Body is UnaryExpression)
            {
                var unary = (UnaryExpression)lambda.Body;
                var operand = unary.Operand;

                if (ExpressionType.MemberAccess == operand.NodeType)
                {
                    var memberExpr = (MemberExpression)operand;

                    return memberExpr.Member.Name;
                }
                else if (ExpressionType.Call == operand.NodeType)
                {
                    var methodExpr = (MethodCallExpression)operand;

                    return methodExpr.Method.Name;
                }
            }
            else
            {
                var memberExpr = (MemberExpression)lambda.Body;

                return memberExpr.Member.Name;
            }

            throw new InvalidOperationException();
        }
    }
}
