using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]
    public class AsTypeTest
    {
        public class Test1
        {
            public int T1;
        }

        public class Test2 : Test1
        {
            public int T2;
        }

        [Test]
        public void CanOutputTypeAsOneOfItsBaseTypes()
        {
            var subj = new Test2
            {
                T1 = 1,
                T2 = 2
            };

            var writer = new ObjectLiteralWriter();
            var output = writer.GetLiteral(subj);

            Assert.AreEqual(
@"new Test2()
{
T1 = 1,
T2 = 2,
}"
, output);

            output = writer.GetLiteral(subj, asType: typeof(Test2));

            Assert.AreEqual(
@"new Test2()
{
T1 = 1,
T2 = 2,
}"
, output);

            output = writer.GetLiteral(subj, asType: typeof(Test1));

            Assert.AreEqual(
@"new Test1()
{
T1 = 1,
}"
, output);
        }

        [Test]
        public void ThrowsIfGivenInvalidType()
        {
            var subj = new Test1
            {
                T1 = 1,
            };

            var writer = new ObjectLiteralWriter();
            Assert.Throws<ArgumentException>(() =>
            {
                writer.GetLiteral(subj, asType: typeof(Test2));
            });
        }

        [Test]
        public void CanBeUsedWithEnumerable()
        {
            var subj = new Dictionary<int, int>
            {
                {1,2}
            };

            var writer = new ObjectLiteralWriter();
            var output = writer.GetLiteral(subj);

            Assert.AreEqual(
@"new Dictionary<Int32,Int32>()
{
{
1,2
},
}"
, output);

            output = writer.GetLiteral(subj, asType: typeof(IEnumerable<KeyValuePair<int, int>>));

            Assert.AreEqual(
@"new List<KeyValuePair<Int32,Int32>>()
{
new KeyValuePair<Int32,Int32>()
{
Key = 1,
Value = 2,
},
}"
, output);
        }

        [Test]
        public void CanOverrideItemTypesInEnumerable()
        {
            var subj = new List<Test1>
            {
                new Test1(),
                new Test2()
            };

            var writer = new ObjectLiteralWriter()
            {
                SkipMembersWithDefaultValue = false
            };
            var output = writer.GetLiteral(subj);

            Assert.AreEqual(
@"new List<Test1>()
{
new Test1()
{
T1 = 0,
},
new Test2()
{
T1 = 0,
T2 = 0,
},
}"
            , output);

            output = writer.GetLiteral(subj, asType: typeof(IEnumerable<Test1>), useCollectionGenericTypeForItemLiterals: true);

            Assert.AreEqual(
@"new List<Test1>()
{
new Test1()
{
T1 = 0,
},
new Test1()
{
T1 = 0,
},
}"
            , output);
        }
    }
}