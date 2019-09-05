/*
 * this code is from https://github.com/domaindrivendev/Swashbuckle.AspNetCore
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    internal class XmlCommentsMemberNameHelper
    {
        public static string GetMemberNameForMethod(MethodInfo method)
        {
            var stringBuilder = new StringBuilder("M:");
            stringBuilder.Append(QualifiedNameFor(method.DeclaringType, false));
            stringBuilder.Append("." + method.Name);
            var parameters = method.GetParameters();
            if (parameters.Any<ParameterInfo>())
            {
                var values = parameters.Select<ParameterInfo, string>(p =>
                {
                    if (!p.ParameterType.IsGenericParameter)
                        return QualifiedNameFor(p.ParameterType, true);
                    return string.Format("`{0}", p.ParameterType.GenericParameterPosition);
                });
                stringBuilder.Append("(" + string.Join(",", values) + ")");
            }
            return stringBuilder.ToString();
        }

        public static string GetMemberNameForType(Type type)
        {
            var stringBuilder = new StringBuilder("T:");
            stringBuilder.Append(QualifiedNameFor(type, false));
            return stringBuilder.ToString();
        }

        public static string GetMemberNameForMember(MemberInfo memberInfo)
        {
            var stringBuilder = new StringBuilder((memberInfo.MemberType & MemberTypes.Field) != (MemberTypes)0 ? "F:" : "P:");
            stringBuilder.Append(QualifiedNameFor(memberInfo.DeclaringType, false));
            stringBuilder.Append("." + memberInfo.Name);
            return stringBuilder.ToString();
        }

        private static string QualifiedNameFor(Type type, bool expandGenericArgs = false)
        {
            if (type.IsArray)
                return QualifiedNameFor(type.GetElementType(), expandGenericArgs) + "[]";
            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(type.Namespace))
                stringBuilder.Append(type.Namespace + ".");
            if (type.IsNested)
                stringBuilder.Append(type.DeclaringType.Name + ".");
            if (type.IsConstructedGenericType & expandGenericArgs)
            {
                var str = type.Name.Split('`').First<string>();
                stringBuilder.Append(str);
                var values = type.GetGenericArguments().Select<Type, string>(t =>
                {
                    if (!t.IsGenericParameter)
                        return QualifiedNameFor(t, true);
                    return string.Format("`{0}", t.GenericParameterPosition);
                });
                stringBuilder.Append("{" + string.Join(",", values) + "}");
            }
            else
                stringBuilder.Append(type.Name);
            return stringBuilder.ToString();
        }
    }
}