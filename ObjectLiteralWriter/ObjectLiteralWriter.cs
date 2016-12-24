﻿using System;
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
            bool useCollectionGenericTypeForItemLiterals = false);

        Func<Type, object, string> CustomLiteralWriter
        {
            get;
            set;
        }

        Func<PropertyInfo, FieldInfo, object, string> CustomMemberWriter
        {
            get;
            set;
        }

        IObjectLiteralWriter Clone();
    }

    public class ObjectLiteralWriter : IObjectLiteralWriter
    {
        private StringBuilder _builder;

        public IObjectLiteralWriter Clone()
        {
            return new ObjectLiteralWriter()
            {
                CustomLiteralWriter = CustomLiteralWriter,
                CustomMemberWriter = CustomMemberWriter
            };
        }

        public Func<Type, object, string> CustomLiteralWriter
        {
            get;
            set;
        }

        public Func<PropertyInfo, FieldInfo, object, string> CustomMemberWriter
        {
            get;
            set;
        }

        public string GetLiteral(
            object target,
            string propertyName = null,
            bool includeSemicolon = false,
            Type asType = null,
            bool useCollectionGenericTypeForItemLiterals = false)
        {
            _builder = new StringBuilder();

            AppendVarNameIfPresent(target, propertyName);

            AppendLiteral(target, asType, useCollectionGenericTypeForItemLiterals);

            if (includeSemicolon)
            {
                _builder.Append(";");
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
                var currentCultrue = Thread.CurrentThread.CurrentCulture;
                try
                {
                    Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                    var literal = target.ToString() + targetType.GetNumericSuffix();
                    _builder.Append(literal);
                    return;
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = currentCultrue;
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

            if (targetType.IsTupleT())
            {
                AppendTuple(targetType, target);
                return;
            }

            AppendClass(targetType, target);
        }

        private void AppendTuple(Type targetType, object target)
        {
            var genericTypes = targetType.GetGenericArguments();
            var argumentNames = genericTypes.Select(GetClassConsrtuctorName).ToArray();
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

        private void AppendEnumerable(Type targetType, object target, bool useGenericTypesFromTargetForLiterals)
        {
            var enumerableType = GetIEnumerableTypeOfType(targetType);

            var genericType = enumerableType.GetGenericArguments()[0];
            var typeName = GetClassConsrtuctorName(genericType);
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
            var enumerabeTarget = (IEnumerable)target;
            var itemInits = enumerabeTarget
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
            var keyTypeName = GetClassConsrtuctorName(keyType);
            var valueType = genericTypes[1];
            var valueTypeName = GetClassConsrtuctorName(valueType);
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
                    return string.Format("{{{0},{1}}},\r\n", keyLiteral, valueLiteral);
                })
                .ToArray();

            return string.Join("", itemInits);
        }

        private void AppendClass(Type targetType, object target)
        {
            string fieldInitiSection = GetFieldInitSection(targetType, target);
            string propertieInitiSection = GetPropertyInitSection(targetType, target);
            string classConstructorName = GetClassConsrtuctorName(targetType);
            _builder.AppendFormat("new {0}()\r\n{{\r\n", classConstructorName);
            _builder.Append(fieldInitiSection);
            _builder.Append(propertieInitiSection);
            _builder.Append("}");
        }

        private string GetClassConsrtuctorName(Type targetType)
        {
            if (targetType.IsGenericType == false)
            {
                return targetType.Name;
            }

            if (targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return targetType.GetGenericArguments().Select(GetClassConsrtuctorName).Single() + "?";
            }

            var baseType = targetType.GetGenericTypeDefinition();
            var name = baseType.Name.Split('`')[0];
            var paramNames = targetType.GetGenericArguments().Select(GetClassConsrtuctorName);
            var constructorName = name + "<" + string.Join(",", paramNames) + ">";
            return constructorName;
        }

        private string GetFieldInitSection(Type targetType, object target)
        {
            var fieldInits = targetType
                .GetFields()
                .Select(x =>
                {
                    var customWriterResult = SafeGetCustomMemberResult(null, x, target);
                    if (customWriterResult != null)
                    {
                        return customWriterResult;
                    }

                    var fieldName = x.Name;
                    var value = x.GetValue(target);
                    var valueLiteral = this.Clone().GetLiteral(value);
                    return string.Format("{0} = {1},\r\n", fieldName, valueLiteral);
                })
                .ToArray();

            return string.Join("", fieldInits);
        }

        private string GetPropertyInitSection(Type targetType, object target)
        {
            var fieldInits = targetType
                .GetProperties()
                .Select(x =>
                {
                    var customWriterResult = SafeGetCustomMemberResult(x, null, target);
                    if (customWriterResult != null)
                    {
                        return customWriterResult;
                    }

                    var fieldName = x.Name;
                    var value = x.GetValue(target, null);
                    var valueLiteral = this.Clone().GetLiteral(value);
                    return string.Format("{0} = {1},\r\n", fieldName, valueLiteral);
                })
                .ToArray();

            return string.Join("", fieldInits);
        }

        private string SafeGetCustomMemberResult(PropertyInfo propInfo, FieldInfo fieldInfo, object target)
        {
            if (CustomMemberWriter == null)
            {
                return null;
            }
            return CustomMemberWriter(propInfo, fieldInfo, target);
        }

        private void AppendDateTime(DateTime target)
        {
            if (target.Second != 0 || target.Minute != 0 || target.Hour != 0)
            {
                _builder.AppendFormat("new DateTime({0}, {1}, {2}, {3}, {4}, {5})",
                    target.Year, target.Month, target.Day, target.Hour, target.Minute, target.Second);
                return;
            }

            _builder.AppendFormat("new DateTime({0}, {1}, {2})",
                    target.Year, target.Month, target.Day);
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
}