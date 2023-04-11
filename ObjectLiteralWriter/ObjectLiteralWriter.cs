using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ObjectLiteralWriter
{
    public interface IObjectLiteralWriter
    {
        string GetLiteral(
            object target,
            string propertyName = null,
            bool includeSemicolon = false,
            Type asType = null,
            bool useCollectionGenericTypeForItemLiterals = false,
            string indentation = null);

        /// <summary>
        /// Function will get a chance to examine values 
        /// that are being converted to literals.
        /// Return literal string to write it.
        /// Return null to fallback to default literal writer.
        /// Return empty string to skip writing this value.
        /// </summary>
        Func<Type, object, string> CustomLiteralWriter
        {
            get;
            set;
        }

        /// <summary>
        /// Function will get a chance to examine type members
        /// that are being converted to literals.
        /// Return literal string to write it.
        /// Return null to fallback to default literal writer.
        /// Return empty string to skip writing this member.
        /// </summary>
        Func<PropertyInfo, FieldInfo, object, string> CustomMemberWriter
        {
            get;
            set;
        }

        /// <summary>
        /// Function determines order of properties
        /// </summary>
        Func<IEnumerable<PropertyInfo>, object, IEnumerable<PropertyInfo>> PropertyOrderer { get; set; }

        /// <summary>
        /// Function determines order of fields
        /// </summary>
        Func<IEnumerable<FieldInfo>, object, IEnumerable<FieldInfo>> FieldOrderer { get; set; }


        IObjectLiteralWriter Clone();
    }

    public class ObjectLiteralWriter : IObjectLiteralWriter
    {
        private StringBuilder _builder;

        public IObjectLiteralWriter Clone()
        {
            return new ObjectLiteralWriter()
            {
                SkipMembersWithDefaultValue = this.SkipMembersWithDefaultValue,
                CustomLiteralWriter = this.CustomLiteralWriter,
                CustomMemberWriter = this.CustomMemberWriter,
                PropertyOrderer = this.PropertyOrderer,
                FieldOrderer = this.FieldOrderer
            };
        }

        public bool SkipMembersWithDefaultValue { get; set; } = true;

        /// <summary>
        /// Function will get a chance to examine values 
        /// that are being converted to literals.
        /// Return literal string to write it.
        /// Return null to fallback to default literal writer.
        /// Return empty string to skip writing this value.
        /// </summary>
        public Func<Type, object, string> CustomLiteralWriter { get; set; }

        /// <summary>
        /// Function will get a chance to examine type members
        /// that are being converted to literals.
        /// Return literal string to write it.
        /// Return null to fallback to default literal writer.
        /// Return empty string to skip writing this member.
        /// </summary>
        public Func<PropertyInfo, FieldInfo, object, string> CustomMemberWriter { get; set; }

        /// <summary>
        /// Function determines order of properties
        /// </summary>
        public Func<IEnumerable<PropertyInfo>, object, IEnumerable<PropertyInfo>> PropertyOrderer { get; set; } = (props, target) => props.OrderBy(x => x.Name);

        /// <summary>
        /// Function determines order of fields
        /// </summary>
        public Func<IEnumerable<FieldInfo>, object, IEnumerable<FieldInfo>> FieldOrderer { get; set; } = (fields, target) => fields.OrderBy(x => x.Name);

        public string GetLiteral(
            object target,
            string propertyName = null,
            bool includeSemicolon = false,
            Type asType = null,
            bool useCollectionGenericTypeForItemLiterals = false,
            string indentation = null)
        {
            _builder = new StringBuilder();

            AppendVarNameIfPresent(target, propertyName);

            AppendLiteral(target, asType, useCollectionGenericTypeForItemLiterals);

            if (includeSemicolon)
            {
                _builder.Append(";");
            }

            if (indentation != null)
            {
                return _builder.ToString().IndentLiteral(indentation);
            }

            return _builder.ToString();
        }

        private void AppendLiteral(object target, Type asType = null, bool useCollectionGenericTypeForItemLiterals = false)
        {
            if (target == null)
            {
                _builder.Append("null");
                return;
            }

            var targetType = target.GetType();

            if (asType != null)
            {
                if (asType.IsAssignableFrom(targetType) == false)
                {
                    throw new ArgumentException("Target is not instance of " + asType.Name);
                }

                targetType = asType;
            }

            if (CustomLiteralWriter != null)
            {
                string customWriterResult = CustomLiteralWriter(targetType, target);
                if (customWriterResult != null)
                {
                    _builder.Append(customWriterResult);
                    return;
                }
            }

            if (targetType == typeof(object))
            {
                _builder.Append("new object()");
                return;
            }

            if (targetType.IsNumeric())
            {
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                try
                {
                    Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                    var literal = target.ToString() + targetType.GetNumericSuffix();
                    _builder.Append(literal);
                    return;
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                }             
            }

            if (targetType == typeof(char))
            {
                _builder.Append("'" + target.ToString() + "'");
                return;
            }

            if (targetType == typeof(string))
            {
                _builder.Append("\"" + target.ToString() + "\"");
                return;
            }

            if (targetType.IsPrimitive)
            {
                _builder.Append(target.ToString().ToLowerInvariant());
                return;
            }

            if (targetType.IsEnum)
            {
                AppendEnum(targetType, (Enum)target);
                return;
            }

            if (targetType == typeof(Guid))
            {
                AppendGuid((Guid)target);
                return;
            }

            if (targetType == typeof(DateTimeOffset))
            {
                AppendDateTimeOffset((DateTimeOffset)target);
                return;
            }

            if (targetType == typeof(DateTime))
            {
                AppendDateTime((DateTime)target);
                return;
            }

            if (targetType == typeof(TimeSpan))
            {
                AppendTimeSpan((TimeSpan)target);
                return;
            }

            if (targetType.IsDateOnly())
            {
                AppendDateOnly(target);
                return;
            }

            if (targetType.IsTimeOnly())
            {
                AppendTimeOnly(target);
                return;
            }

            if (targetType.IsSubclassOfIDictionaryT())
            {
                AppendDictionary(targetType, target, useCollectionGenericTypeForItemLiterals);
                return;
            }

            if (targetType.IsSubclassOfIEnumerableT())
            {
                AppendEnumerable(targetType, target, useCollectionGenericTypeForItemLiterals);
                return;
            }

            if (targetType.IsValueTupleT())
            {
                AppendValueTuple(targetType, target);
                return;
            }

            if (targetType.IsReferenceTupleT())
            {
                AppendReferenceTuple(targetType, target);
                return;
            }

            AppendClass(targetType, target);
        }

        private void AppendGuid(Guid target)
        {
            _builder.AppendFormat("Guid.Parse(\"{0}\")", target);
        }

        private void AppendReferenceTuple(Type targetType, object target)
        {
            var genericTypes = targetType.GetGenericArguments();
            var argumentNames = genericTypes.Select(GetClassConstructorName).ToArray();
            var argumentValues = Enumerable
                .Range(1, genericTypes.Length)
                .Select(count => count == 8 ? "Rest" : ("Item" + count))
                .Select(propertyName =>
                {
                    var value = targetType.GetProperty(propertyName).GetValue(target, null);
                    return this.Clone().GetLiteral(value);
                });

            _builder.AppendFormat("new Tuple<{0}>({1})",
                string.Join(", ", argumentNames),
                string.Join(", ", argumentValues));
        }

        private void AppendValueTuple(Type targetType, object target)
        {
            List<string> arguments = new List<string>();

            for (var count = 0; count < targetType.GetGenericArguments().Length; count++)
            {
                // ValueTuples still used Rest tuples when there is more than 7 items,
                // but the literal does not use it, so we have to flatten
                if (count == 7) 
                {
                    target = targetType.GetField("Rest").GetValue(target);
                    targetType = target.GetType();
                    count = -1;
                    continue;
                }

                var value = targetType
                                .GetField($"Item{count + 1}")
                                .GetValue(target);

                arguments.Add(this.Clone().GetLiteral(value));
            }

            _builder.AppendFormat("({0})",
                string.Join(", ", arguments));
        }

        private void AppendEnumerable(Type targetType, object target, bool useGenericTypesFromTargetForLiterals)
        {
            var enumerableType = GetIEnumerableTypeOfType(targetType);

            var genericType = enumerableType.GetGenericArguments()[0];
            var typeName = GetClassConstructorName(genericType);
            var itemAddingSection = useGenericTypesFromTargetForLiterals
                ? GetArrayItemAddingSection(target, genericType)
                : GetArrayItemAddingSection(target);

            string constructorLiteral = "new List<{0}>()";
            if (targetType.IsArray)
            {
                constructorLiteral = "new {0}[]";
            }

            _builder.AppendFormat(constructorLiteral + "\r\n{{\r\n", typeName);
            _builder.Append(itemAddingSection);
            _builder.Append("}");
        }

        private static Type GetIEnumerableTypeOfType(Type targetType)
        {
            if (targetType.IsExactlyIEnumerableT())
            {
                return targetType;
            }

            return targetType
                    .GetInterfaces()
                    .Single(x => x.IsExactlyIEnumerableT());
        }

        private object GetArrayItemAddingSection(object target, Type itemTypeOverride = null)
        {
            var enumerableTarget = (IEnumerable)target;
            var itemInits = enumerableTarget
                .Cast<object>()
                .Select(item =>
                {
                    string itemLiteral = this.Clone().GetLiteral(item, asType: itemTypeOverride);

                    return string.Format("{0},\r\n", itemLiteral);
                })
                .ToArray();

            return string.Join("", itemInits);
        }

        private void AppendDictionary(Type targetType, object target, bool useGenericTypesFromTargetForLiterals)
        {
            var dictType = GetIDictionaryTypeOfType(targetType);

            var genericTypes = dictType.GetGenericArguments();
            var keyType = genericTypes[0];
            var keyTypeName = GetClassConstructorName(keyType);
            var valueType = genericTypes[1];
            var valueTypeName = GetClassConstructorName(valueType);
            var itemAddingSection = useGenericTypesFromTargetForLiterals
                ? GetDictItemAddingSection(target, keyType, valueType)
                : GetDictItemAddingSection(target);

            _builder.AppendFormat("new Dictionary<{0},{1}>()\r\n{{\r\n", keyTypeName, valueTypeName);
            _builder.Append(itemAddingSection);
            _builder.Append("}");
        }

        private static Type GetIDictionaryTypeOfType(Type targetType)
        {
            if (targetType.IsExactlyIDictionaryT())
            {
                return targetType;
            }

            return targetType
                    .GetInterfaces()
                    .Single(x => x.IsExactlyIDictionaryT());
        }

        private object GetDictItemAddingSection(object target, Type keyTypeOverride = null, Type valueTypeOverride = null)
        {
            var dict = (IDictionary)target;
            var itemInits = dict.Keys
                .Cast<object>()
                .Select(key =>
                {
                    var keyLiteral = this.Clone().GetLiteral(key, asType: keyTypeOverride);
                    var value = dict[key];
                    var valueLiteral = this.Clone().GetLiteral(value, asType: valueTypeOverride);
                    return string.Format("{{\r\n{0},{1}\r\n}},\r\n", keyLiteral, valueLiteral);
                })
                .ToArray();

            return string.Join("", itemInits);
        }

        private void AppendClass(Type targetType, object target)
        {
            string fieldInitSection = GetFieldInitSection(targetType, target);
            string propertyInitSection = GetPropertyInitSection(targetType, target);
            string classConstructorName = GetClassConstructorName(targetType);
            _builder.AppendFormat("new {0}()\r\n{{\r\n", classConstructorName);
            _builder.Append(fieldInitSection);
            _builder.Append(propertyInitSection);
            _builder.Append("}");
        }

        private string GetClassConstructorName(Type targetType)
        {
            if (targetType.IsGenericType == false)
            {
                return targetType.Name;
            }

            if (targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return targetType.GetGenericArguments().Select(GetClassConstructorName).Single() + "?";
            }

            var baseType = targetType.GetGenericTypeDefinition();
            var name = baseType.Name.Split('`')[0];
            var paramNames = targetType.GetGenericArguments().Select(GetClassConstructorName);
            var constructorName = name + "<" + string.Join(",", paramNames) + ">";
            return constructorName;
        }

        private string GetFieldInitSection(Type targetType, object target)
        {
            var fieldInits = 
                this.FieldOrderer(
                    targetType.GetFields(),
                    target)
                .Select(x =>
                {
                    var customWriterResult = SafeGetCustomMemberResult(null, x, target);
                    if (customWriterResult != null)
                    {
                        return customWriterResult;
                    }

                    var fieldName = x.Name;
                    var value = x.GetValue(target);
                    if (this.SkipMembersWithDefaultValue
                        && value as bool? != false
                        && (x.FieldType.GetDefaultInstanceValue() == value 
                            || (x.FieldType.GetDefaultInstanceValue()?.Equals(value) ?? false)))
                    {
                        return "";
                    }
                    var valueLiteral = this.Clone().GetLiteral(value);
                    return string.Format("{0} = {1},\r\n", fieldName, valueLiteral);
                })
                .ToArray();

            return string.Join("", fieldInits);
        }

        private string GetPropertyInitSection(Type targetType, object target)
        {
            var propertyInits = 
                this.PropertyOrderer(
                    targetType.GetProperties(),
                    target)
                .Select(x =>
                {
                    var customWriterResult = SafeGetCustomMemberResult(x, null, target);
                    if (customWriterResult != null)
                    {
                        return customWriterResult;
                    }

                    var fieldName = x.Name;
                    var value = x.GetValue(target, null);
                    if (this.SkipMembersWithDefaultValue
                        && value as bool? != false
                         && (x.PropertyType.GetDefaultInstanceValue() == value 
                            || (x.PropertyType.GetDefaultInstanceValue()?.Equals(value) ?? false)))
                    {
                        return "";
                    }
                    var valueLiteral = this.Clone().GetLiteral(value);
                    return string.Format("{0} = {1},\r\n", fieldName, valueLiteral);
                })
                .ToArray();

            return string.Join("", propertyInits);
        }

        private string SafeGetCustomMemberResult(PropertyInfo propInfo, FieldInfo fieldInfo, object target)
        {
            if (CustomMemberWriter == null)
            {
                return null;
            }
            return CustomMemberWriter(propInfo, fieldInfo, target);
        }
        
        private void AppendDateOnly(object target)
        {
            var targetType = target.GetType();
            _builder.AppendFormat("new DateOnly({0}, {1}, {2})",
                targetType.GetProperty("Year").GetValue(target),
                targetType.GetProperty("Month").GetValue(target),
                targetType.GetProperty("Day").GetValue(target));
        }

        private void AppendTimeOnly(object target)
        {
            var targetType = target.GetType();
           _builder.AppendFormat("new TimeOnly({0}, {1}, {2}, {3})",
                targetType.GetProperty("Hour").GetValue(target),
                targetType.GetProperty("Minute").GetValue(target),
                targetType.GetProperty("Second").GetValue(target),
                targetType.GetProperty("Millisecond").GetValue(target));
        }

        private void AppendDateTimeOffset(DateTimeOffset target)
        {
              _builder.AppendFormat("new DateTimeOffset({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                    target.Year, 
                    target.Month, 
                    target.Day, 
                    target.Hour, 
                    target.Minute, 
                    target.Second,
                    target.Millisecond,
                    this.Clone().GetLiteral(target.Offset));
        }

        private void AppendDateTime(DateTime target)
        {
            if (target.Millisecond != 0)
            {
                    _builder.AppendFormat("new DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                    target.Year, 
                    target.Month, 
                    target.Day, 
                    target.Hour, 
                    target.Minute, 
                    target.Second,
                    target.Millisecond,
                    $"DateTimeKind.{target.Kind}");
                return;
            }

            _builder.AppendFormat("new DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                target.Year, 
                target.Month, 
                target.Day, 
                target.Hour, 
                target.Minute, 
                target.Second, 
                $"DateTimeKind.{target.Kind}");
            return;
            

        }

        private void AppendTimeSpan(TimeSpan target)
        {
            if (target.Days != 0 && target.Milliseconds != 0)
            {
                _builder.AppendFormat("new TimeSpan({0}, {1}, {2}, {3}, {4})",
                    target.Days, target.Hours, target.Minutes, target.Seconds, target.Milliseconds);
                return;
            }

            if (target.Days != 0)
            {
                _builder.AppendFormat("new TimeSpan({0}, {1}, {2}, {3})",
                    target.Days, target.Hours, target.Minutes, target.Seconds);
                return;
            }

            _builder.AppendFormat("new TimeSpan({0}, {1}, {2})",
                target.Hours, target.Minutes, target.Seconds);
        }

        private void AppendEnum(Type targetType, Enum target)
        {
            string literal;

            if (targetType.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
            {
                AppendFlagsEnum(targetType, target);
                return;
            }

            if (Enum.IsDefined(targetType, target))
            {
                literal = targetType.Name + "." + target.ToString();
                _builder.Append(literal);
                return;
            }

            literal = "(" + targetType.Name + ")" + target.ToString();
            _builder.Append(literal);
        }

        private void AppendFlagsEnum(Type targetType, Enum target)
        {
            // todo detect thing like FlagsEnum.Four | 32
            var values = Enum.GetValues(targetType)
                .Cast<Enum>()
                .Where(target.HasFlag)
                .Select(value => targetType.Name + "." + value.ToString());

            var literal = string.Join(" | ", values);
            _builder.Append(literal);
        }

        private void AppendVarNameIfPresent(object target, string propertyName)
        {
            if (propertyName == null)
            {
                return;
            }

            if (target == null)
            {
                _builder.Append("object " + propertyName + " = ");
                return;
            }
            _builder.Append("var " + propertyName + " = ");
        }
    }

    public static class LiteralExtensions
    {
        public static string IndentLiteral(this string literal, string indent = "    ")
        {
            var indentLevel = 0;

            var indentedLines = literal.Split('\n').Select(line =>
            {
                var indentStr = string.Join("", Enumerable.Repeat(indent, indentLevel));
                if (line.StartsWith("{")){
                    indentStr = string.Join("", Enumerable.Repeat(indent, indentLevel));
                    indentLevel += 1;
                }
                if (line.StartsWith("}")){
                    indentLevel -= 1;
                    indentStr = string.Join("", Enumerable.Repeat(indent, indentLevel));
                }
                return indentStr + line;
            });

            return String.Join("\n", indentedLines);
        } 
    }
}