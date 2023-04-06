using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectLiteralWriter
{
    public static class TypeExtensions
    {
        public static Dictionary<Type, string> NumericSuffixes = new Dictionary<Type, string>()
        {
            {typeof (Byte), ""},
            {typeof (SByte), ""},
            {typeof (Int16), ""},
            {typeof (Int32), ""},
            {typeof (Int64), "L"},
            {typeof (UInt16), ""},
            {typeof (UInt32), "U"},
            {typeof (UInt64), "UL"},
            {typeof (Single), "F"},
            {typeof (Double), "D"},
            {typeof (Decimal), "M"}
        };

        public static bool IsNumeric(this Type type)
        {
            return NumericSuffixes.Keys.Contains(type);
        }

        public static string GetNumericSuffix(this Type type)
        {
            return NumericSuffixes[type];
        }

        public static bool IsExactlyIDictionaryT(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }

        public static bool IsSubclassOfIDictionaryT(this Type type)
        {
            return IsExactlyIDictionaryT(type)
                    || type.GetInterfaces().Any(IsExactlyIDictionaryT);
        }

        public static bool IsExactlyIEnumerableT(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        public static bool IsSubclassOfIEnumerableT(this Type type)
        {
            return IsExactlyIEnumerableT(type)
                    || type.GetInterfaces().Any(IsExactlyIEnumerableT);
        }

        public static bool IsDateOnly(this Type type)
        {
            return type.FullName == "System.DateOnly";
        }

        public static bool IsTimeOnly(this Type type)
        {
            return type.FullName == "System.TimeOnly";
        }

        public static bool IsValueTupleT(this Type type)
        {
            if (type.IsGenericType == false)
            {
                return false;
            }

            var tupleTypes = new[]
            {
                typeof (ValueTuple<>),
                typeof (ValueTuple<,>),
                typeof (ValueTuple<,,>),
                typeof (ValueTuple<,,,>),
                typeof (ValueTuple<,,,,>),
                typeof (ValueTuple<,,,,,>),
                typeof (ValueTuple<,,,,,,>),
                typeof (ValueTuple<,,,,,,,>)
            };

            var genericType = type.GetGenericTypeDefinition();
            return tupleTypes.Contains(genericType);
        }

        public static bool IsReferenceTupleT(this Type type)
        {
            if (type.IsGenericType == false)
            {
                return false;
            }

            var tupleTypes = new[]
            {
                typeof (Tuple<>),
                typeof (Tuple<,>),
                typeof (Tuple<,,>),
                typeof (Tuple<,,,>),
                typeof (Tuple<,,,,>),
                typeof (Tuple<,,,,,>),
                typeof (Tuple<,,,,,,>),
                typeof (Tuple<,,,,,,,>)
            };

            var genericType = type.GetGenericTypeDefinition();
            return tupleTypes.Contains(genericType);
        }
    }
}