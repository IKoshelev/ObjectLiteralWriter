using System;
using System.Linq;
using NUnit.Framework;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]
    public class ClassTest
    {
        public class Test1
        {
        }

        [Test]
        public void CanHandleEmptyClass()
        {
            var subj = new Test1()
            {
            };

            Util.AssertTypeLiteral(subj,
@"new Test1()
{
}");
        }

        public class Test2
        {
            public object Foo;
        }

        [Test]
        public void CanHandleField()
        {
            var subj = new Test2()
            {
                Foo = new object(),
            };

            Util.AssertTypeLiteral(subj,
@"new Test2()
{
Foo = new object(),
}");
        }

        public class Test3
        {
            public object Foo { get; set; }
        }

        [Test]
        public void CanHandleProperty()
        {
            var subj = new Test2()
            {
                Foo = new object(),
            };

            Util.AssertTypeLiteral(subj,
@"new Test2()
{
Foo = new object(),
}");
        }

        public class Test4
        {
            public object Foo;

            public object Bar { get; set; }
        }

        [Test]
        public void CanHandleNulls()
        {
            var subj = new Test4()
            {
            };

            Util.AssertTypeLiteral(subj,
                @"new Test4()
{
Foo = null,
Bar = null,
}");
        }

                [Test]
        public void CanSkipNullsForClasses()
        {
            var subj = new Test4()
            {
            };

            Util.AssertTypeLiteral(subj,
                @"new Test4()
{
}",

skipMembersWithDefaultValue: true);
        }

        public class Test5<T>
        {
            public T Foo;

            public T Bar { get; set; }
        }

        [Test]
        public void CanHandleBuiltinPropertiesAndFields()
        {
            Etalon.All.ForEach(vlp =>
            {
                Type targetType = vlp.Value.GetType();
                Type closedType = typeof(Test5<>).MakeGenericType(targetType);
                object target = Activator.CreateInstance(closedType);
                closedType.GetField("Foo").SetValue(target, vlp.Value);
                closedType.GetProperty("Bar").SetValue(target, vlp.Value, null);

                var writer = new ObjectLiteralWriter()
                {
                    SkipMembersWithDefaultValue = false
                };
                var output = writer.GetLiteral(target);

                var expectedOutput = @"new Test5<" + targetType.Name + @">()
{
Foo = " + vlp.Literal + @",
Bar = " + vlp.Literal + @",
}";

                Assert.AreEqual(expectedOutput, output);
            });
        }

        [Test]
        public void CanHandleNullables()
        {
            Etalon.All.ForEach(vlp =>
            {
                Type targetType = vlp.Value.GetType();
                if (targetType.IsValueType == false)
                {
                    return;
                }
                targetType = typeof(Nullable<>).MakeGenericType(targetType);
                Type closedType = typeof(Test5<>).MakeGenericType(targetType);
                object target = Activator.CreateInstance(closedType);

                var writer = new ObjectLiteralWriter()
                {
                    SkipMembersWithDefaultValue = false
                };
                var output = writer.GetLiteral(target);

                closedType.GetField("Foo").SetValue(target, vlp.Value);
                closedType.GetProperty("Bar").SetValue(target, vlp.Value, null);

                var expectedOutput = @"new Test5<" + vlp.Value.GetType().Name + @"?>()
{
Foo = " + vlp.Literal + @",
Bar = " + vlp.Literal + @",
}";

                closedType.GetField("Foo").SetValue(target, null);
                closedType.GetProperty("Bar").SetValue(target, null, null);

                expectedOutput = @"new Test5<" + vlp.Value.GetType().Name + @"?>()
{
Foo = null,
Bar = null,
}";

                Assert.AreEqual(expectedOutput, output);
            });
        }

        public class Test6
        {
            public Test4 Foo;
        }

        [Test]
        public void CanHandleNestedTypes()
        {
            var subj = new Test6()
            {
                Foo = new Test4()
                {
                    Foo = new object(),
                    Bar = new object()
                }
            };

            Util.AssertTypeLiteral(subj,
@"new Test6()
{
Foo = new Test4()
{
Foo = new object(),
Bar = new object(),
},
}");
        }

        public class TestGeneric<T1,T2>
        {
            public T1 T_1 { get; set; }
            public T2 T_2 { get; set; }
        }

        [Test]
        public void CanSkipDefaultValuesExceptBooleans()
        {
            var subj = new TestGeneric<object, decimal>
            {
                T_1 = null,
                T_2 = default,
            };

            Util.AssertTypeLiteral(subj,
@"new TestGeneric<Object,Decimal>()
{
}",
skipMembersWithDefaultValue: true);

            var subj2 = new TestGeneric<bool, Guid>
            {
                T_1 = default,
                T_2 = default,
            };

            Util.AssertTypeLiteral(subj2,
@"new TestGeneric<Boolean,Guid>()
{
T_1 = false,
}",
skipMembersWithDefaultValue: true);
        }
    }
}