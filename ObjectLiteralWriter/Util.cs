using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using ObjectLiteralWriter;

namespace ObjectLiteralWriter
{
    public class PublicPropReflectingComparer : IEqualityComparer<object>
    {
        new public bool Equals(object x, object y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentException("Equals can not operate on nulls");  
            }

            var type = x.GetType();
            if (type != y.GetType())
            {
                throw new ArgumentException("Equals can only operate on object of sames type");
            }

            return type.GetProperties().All(prop =>
            {
                object val1 = prop.GetValue(x, null);
                object val2 = prop.GetValue(y, null);

                if (val1 == null)
                {
                    return val2 == null;
                }

                var a = new[] {1, 2, 3}.Distinct();

                return val1.Equals(val2);
            });
        }
        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return obj
                .GetType()
                .GetProperties()
                .Where(propInfo => propInfo.PropertyType == typeof(string) || !propInfo.PropertyType.IsClass)
                .Select(propInfo => propInfo.GetValue(obj, null))
                .Aggregate(17, (current, myValue) => current * 23 + (myValue != null ? myValue.GetHashCode() : 0));
        }
    }

    public static class Util
    {
        private static readonly char[] StringLiteralOpeners = new[] { '\'', '"' };

        /// <summary>
        /// This function is provided for convenience, but it is not the main responsibility of the
        /// package, and as such, it is not tested and my not handle extreme corner cases.
        /// </summary>
        /// <param name="source">Flat source with only \r\n</param>
        /// <returns>Indented source</returns>
        public static string Indent(this string source)
        {
            var sb = new StringBuilder();
            int indentCounter = 0;

            char? lastStringLiteralOpener = null;

            for (int count1 = 0; count1 < source.Length; count1++)
            {
                char currentChar = source[count1];
                bool isLastChar = (count1 + 1 == source.Length);
                char? nextChar = isLastChar ? (char?)null : source[count1 + 1];

                sb.Append(currentChar);

                bool isEscaped = IsCharEscaped(source, count1);

                var isStringEnd = lastStringLiteralOpener == currentChar;
                if (isStringEnd && !isEscaped)
                {
                    lastStringLiteralOpener = null;
                    continue;
                }

                var currentlyInString = lastStringLiteralOpener != null;
                if (currentlyInString)
                {
                    continue;
                }

                var isStringStart = StringLiteralOpeners.Contains(currentChar);
                if (isStringStart)
                {
                    lastStringLiteralOpener = currentChar;
                    continue;
                }

                var isIncreaseIndent = currentChar == '{';
                if (isIncreaseIndent)
                {
                    indentCounter += 1;
                    continue;
                }

                var isDecreaseIndent = currentChar == '}';
                if (isDecreaseIndent)
                {
                    indentCounter -= 1;
                    if (indentCounter < 0)
                    {
                        indentCounter = 0;
                    }
                    continue;
                }

                var isNewLinePairEnd = currentChar == '\n';
                if (isNewLinePairEnd)
                {
                    var isNextCharAClosingCurlyBrace = nextChar == '}';
                    if (isNextCharAClosingCurlyBrace)
                    {
                        sb.Append(' ', (indentCounter - 1) * 4);
                    }
                    else
                    {
                        sb.Append(' ', indentCounter * 4);
                    }
                    continue;
                }
            }

            return sb.ToString();
        }

        private static bool IsCharEscaped(string source, int position)
        {
            var prevCharPositiion = position - 1;
            var preceedingBackslashCount = 0;
            while (prevCharPositiion > -1)
            {
                if (source[prevCharPositiion] != '\\')
                {
                    break;
                }
                preceedingBackslashCount += 1;
                prevCharPositiion -= 1;
            }

            var isPreceededByUnevenNumberOfBackslashes = (preceedingBackslashCount % 2) == 1;

            return isPreceededByUnevenNumberOfBackslashes;
        }

        public static object GetDefaultValue(this Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }

            return null;
        }

        /// <summary>
        /// Given IEnumerable of objects, extracts fields values from each 
        /// and flattens the ones that are themselves enumerables
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        public static IEnumerable<object> ExtractEntitiesFromAnonymousContext(IEnumerable<object> raw)
        {
            return raw
                .SelectMany(obj =>
                {
                    var type = obj.GetType();
                    var fieldValues = type
                                        .GetProperties()
                                        .Select(prop => prop.GetValue(obj,null))
                                        .Where(val => val != null);
                    return fieldValues;
                })
                .SelectMany(fieldVal =>
                {
                    var enumerable = fieldVal as IEnumerable;
                    if (enumerable != null)
                    {
                        return enumerable.Cast<object>();
                    }

                    return new [] {fieldVal};
                });
        } 
 
        public static IObjectLiteralWriter GetLiteralWriterForEntityFramework(bool skipDefaultValues = true)
        {
            var writer = new ObjectLiteralWriter()
            {
                CustomMemberWriter = (propInfo, fieldInfo, target) =>
                {
                    if (fieldInfo != null && fieldInfo.Name == "_entityWrapper")
                    {
                        return "";
                    }

                    if (propInfo == null)
                    {
                        return null;
                    }

                    //Exclude navigation properties
                    if (propInfo.GetGetMethod().IsVirtual)
                    {
                        return "";
                    }

                    if (skipDefaultValues)
                    {
                        var value = propInfo.GetValue(target,null);
                        if (value == null)
                        {
                            return "";
                        }

                        var defaultValue = propInfo.PropertyType.GetDefaultValue();
                        if (value.Equals(defaultValue))
                        {
                            return "";
                        }
                    }

                    return null;
                }
            };

            return writer;
        }
    }
}