using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public static class DbContextCommentExtensions
    {
        private static ConcurrentDictionary<Assembly, XPathNavigator> dic = new ConcurrentDictionary<Assembly, XPathNavigator>();

        public static void ApplyComment(this ModelBuilder modelBuilder)
        {
            foreach (var g in modelBuilder.Model.GetEntityTypes().GroupBy(e => e.ClrType.Assembly))
            {
                var assembly = g.Key;
                var navigator = GetXPathNavigator(assembly);
                if (navigator != null)
                {
                    foreach (var type in g)
                    {
                        type.SetComment(navigator, type.ClrType);
                        foreach (var property in type.GetProperties())
                        {
                            if (property.IsShadowProperty)
                            {
                                continue;
                            }
                            var info = (MemberInfo)property.PropertyInfo ?? property.FieldInfo;
                            property.SetComment(navigator, info);
                        }
                    }
                }
            }
        }

        private static XPathNavigator GetXPathNavigator(Assembly assembly)
        {
            XPathNavigator navigator = null;
            if (!dic.ContainsKey(assembly))
            {
                var xml = Regex.Replace(assembly.Location, @"\.(dll|exe)$", ".xml");
                if (File.Exists(xml))
                {
                    var xmlDocument = new XmlDocument() { XmlResolver = null };
                    xmlDocument.Load(xml);
                    navigator = xmlDocument.CreateNavigator();
                    dic[assembly] = navigator;
                }
                else
                {
                    dic[assembly] = navigator = null;
                }
            }
            else
            {
                navigator = dic[assembly];
            }
            return navigator;
        }

        private static void SetComment(this IMutableAnnotatable annotatable, XPathNavigator navigator, MemberInfo memberInfo)
        {
            var comment = navigator.GetComment(memberInfo);
            if (!string.IsNullOrWhiteSpace(comment))
            {
                annotatable.SetAnnotation("ClrComment", comment);
            }
            else
            {
                annotatable.RemoveAnnotation("ClrComment");
            }
        }

        private static string GetComment(this XPathNavigator navigator, MemberInfo memberInfo)
        {
            if (navigator == null)
            {
                return null;
            }
            string comment = null, name = null;
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    name = XmlCommentsMemberNameHelper.GetMemberNameForMember(fieldInfo);
                    break;
                case MethodInfo methodInfo:
                    name = XmlCommentsMemberNameHelper.GetMemberNameForMethod(methodInfo);
                    break;
                case PropertyInfo propertyInfo:
                    name = XmlCommentsMemberNameHelper.GetMemberNameForMember(propertyInfo);
                    break;
                case Type type:
                    name = XmlCommentsMemberNameHelper.GetMemberNameForType(type);
                    break;
                default:
                    return null;
            }
            var xpathNavigator1 = navigator.SelectSingleNode(string.Format("/doc/members/member[@name='{0}']", name));
            var xpathNavigator2 = xpathNavigator1?.SelectSingleNode("summary");
            if (xpathNavigator2 != null)
            {
                comment = XmlCommentsTextHelper.Humanize(xpathNavigator2.InnerXml);
            }
            if (string.IsNullOrWhiteSpace(comment))
            {
                if (xpathNavigator1?.SelectSingleNode("inheritdoc") != null)
                {
                    switch (memberInfo)
                    {
                        case MethodInfo methodInfo:
                            var baseDefinition = methodInfo.GetBaseDefinition();
                            comment = GetXPathNavigator(baseDefinition.DeclaringType.Assembly)?.GetComment(baseDefinition);
                            break;
                        case PropertyInfo propertyInfo:
                            var declaringType = propertyInfo.GetMethod.GetBaseDefinition()?.DeclaringType;
                            if (declaringType != null)
                            {
                                comment = GetXPathNavigator(declaringType.Assembly)?.GetComment(declaringType.GetProperty(propertyInfo.Name));
                            }
                            break;
                        case Type type:
                            var baseType = type.BaseType;
                            comment = GetXPathNavigator(baseType.Assembly)?.GetComment(baseType);
                            break;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(comment))
            {
                var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    comment = descriptionAttribute.Description;
                }
            }
            return comment;
        }
    }
}