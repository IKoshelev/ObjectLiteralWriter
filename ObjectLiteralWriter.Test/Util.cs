using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace ObjectLiteralWriter.Test
{
    public static class Util
    {
        public static void AssertTypeLiteral<T>(T target, string expectedLiteral)
        {
            var writer = new ObjectLiteralWriter();
            var output = writer.GetLiteral(target);
            Assert.AreEqual(expectedLiteral, output);
        }

        public static void AssertEtaloneMatch(this ValueLiteralPair etalone)
        {
            var writer = new ObjectLiteralWriter();
            var output = writer.GetLiteral(etalone.Value);
            Assert.AreEqual(etalone.Literal, output);
        }

        public static void AssertEtaloneMatch(this IEnumerable<ValueLiteralPair> etalones)
        {
            etalones.ForEach(AssertEtaloneMatch);
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> aciton)
        {
            foreach (T item in items)
            {
                aciton(item);
            }
        }
    }

    public class ValueLiteralPair
    {
        public ValueLiteralPair(object value, string literal)
        {
            Value = value;
            Literal = literal;
        }

        public object Value { get; set; }

        public string Literal { get; set; }
    }

    public enum RgbEnum
    {
        Red, Green, Blue
    }

    [Flags]
    public enum FlagsEnum
    {
        One = 1,
        Two = 2,
        Four = 4,
        Eight = 8
    }

    public static class Etalone
    {
        public static ValueLiteralPair[] All
        {
            get
            {
                return Booleans
                          .Union(Numerics)
                          .Union(Text)
                          .Union(Enums)
                          .Union(Time)
                          .ToArray();
            }
        }

        public static ValueLiteralPair[] Booleans =
        {
            new ValueLiteralPair(true,"true"),
            new ValueLiteralPair(false,"false")
        };

        public static ValueLiteralPair[] Numerics =
        {
            new ValueLiteralPair(Byte.MaxValue, "255"),
            new ValueLiteralPair(Byte.MinValue, "0"),

            new ValueLiteralPair((SByte)0, "0"),
            new ValueLiteralPair(SByte.MaxValue, "127"),
            new ValueLiteralPair(SByte.MinValue, "-128"),

            new ValueLiteralPair((Int16)0, "0"),
            new ValueLiteralPair(Int16.MaxValue, "32767"),
            new ValueLiteralPair(Int16.MinValue, "-32768"),

            new ValueLiteralPair((Int32)0, "0"),
            new ValueLiteralPair(Int32.MaxValue, "2147483647"),
            new ValueLiteralPair(Int32.MinValue, "-2147483648"),

            new ValueLiteralPair((Int64)0, "0L"),
            new ValueLiteralPair(Int64.MaxValue, "9223372036854775807L"),
            new ValueLiteralPair(Int64.MinValue, "-9223372036854775808L"),

            new ValueLiteralPair(UInt16.MaxValue, "65535"),
            new ValueLiteralPair(UInt16.MinValue, "0"),

            new ValueLiteralPair(UInt32.MaxValue, "4294967295U"),
            new ValueLiteralPair(UInt32.MinValue, "0U"),

            new ValueLiteralPair(UInt64.MaxValue, "18446744073709551615UL"),
            new ValueLiteralPair(UInt64.MinValue, "0UL"),

            new ValueLiteralPair((Single)0, "0F"),
            new ValueLiteralPair((Single)123.123F, "123.123F"),
            new ValueLiteralPair((Single)(-123.123F), "-123.123F"),
            new ValueLiteralPair(Single.MaxValue, "3.402823E+38F"),
            new ValueLiteralPair(Single.MinValue, "-3.402823E+38F"),

            new ValueLiteralPair((Double)0, "0D"),
            new ValueLiteralPair((Double)123456.123456D, "123456.123456D"),
            new ValueLiteralPair((Double)(-123456.123456D), "-123456.123456D"),
            new ValueLiteralPair(Double.MaxValue, "1.79769313486232E+308D"),
            new ValueLiteralPair(Double.MinValue, "-1.79769313486232E+308D"),

            new ValueLiteralPair((Decimal)0, "0M"),
            new ValueLiteralPair((Decimal)123456789.123456789m, "123456789.123456789M"),
            new ValueLiteralPair((Decimal)(-123456789.123456789m), "-123456789.123456789M"),
            new ValueLiteralPair(Decimal.MaxValue, "79228162514264337593543950335M"),
            new ValueLiteralPair(Decimal.MinValue, "-79228162514264337593543950335M")
        };

        public static ValueLiteralPair[] Text =
        {
            new ValueLiteralPair('A', "'A'"),
            new ValueLiteralPair('Z', "'Z'"),
            new ValueLiteralPair('0', "'0'"),
            new ValueLiteralPair('Я', "'Я'"),
            new ValueLiteralPair('両', "'両'"),

            new ValueLiteralPair("", "\"\""),
            new ValueLiteralPair("AZАЯ09亰亱亲", "\"AZАЯ09亰亱亲\""),
        };

        public static ValueLiteralPair[] Enums =
        {
            new ValueLiteralPair(RgbEnum.Red, "RgbEnum.Red"),
            new ValueLiteralPair(RgbEnum.Green, "RgbEnum.Green"),
            new ValueLiteralPair(RgbEnum.Blue, "RgbEnum.Blue"),
            new ValueLiteralPair((RgbEnum)10, "(RgbEnum)10"),
            new ValueLiteralPair(FlagsEnum.Four, "FlagsEnum.Four"),
            new ValueLiteralPair(FlagsEnum.Four | FlagsEnum.Two, "FlagsEnum.Two | FlagsEnum.Four"),
        };

        public static ValueLiteralPair[] Time =
        {
            new ValueLiteralPair(new DateTime(1, 1, 1), "new DateTime(1, 1, 1)"),
            new ValueLiteralPair(new DateTime(1, 2, 3), "new DateTime(1, 2, 3)"),
            new ValueLiteralPair(new DateTime(1, 2, 3, 4, 0, 0), "new DateTime(1, 2, 3, 4, 0, 0)"),
            new ValueLiteralPair(new DateTime(1, 2, 3, 4, 5, 0), "new DateTime(1, 2, 3, 4, 5, 0)"),
            new ValueLiteralPair(new DateTime(1, 2, 3, 4, 5, 6), "new DateTime(1, 2, 3, 4, 5, 6)"),

            new ValueLiteralPair(new TimeSpan(1, 2, 3), "new TimeSpan(1, 2, 3)"),
            new ValueLiteralPair(new TimeSpan(1, 2, 3, 4), "new TimeSpan(1, 2, 3, 4)"),
            new ValueLiteralPair(new TimeSpan(1, 2, 3, 4, 5), "new TimeSpan(1, 2, 3, 4, 5)"),
        };
    }
}