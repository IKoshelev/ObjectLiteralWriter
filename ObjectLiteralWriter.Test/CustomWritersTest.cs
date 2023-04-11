using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]
    public class CustomWritersTest
    {
        [Test]
        public void CustomLiteralWriterReturningNullMeansUseDefaultWriter()
        {
            var subj = new object();
            var writer = new ObjectLiteralWriter { CustomLiteralWriter = (type, target) => null };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual("new object()", output);
        }

        [Test]
        public void CustomLiteralWriterReturningAnyStringMeansLiteralIsHandled()
        {
            var subj = new object();
            var writer = new ObjectLiteralWriter { CustomLiteralWriter = (type, target) => string.Empty };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual("", output);

            writer = new ObjectLiteralWriter { CustomLiteralWriter = (type, target) => "FOOBAR" };
            output = writer.GetLiteral(subj);
            Assert.AreEqual("FOOBAR", output);
        }

        public class Test1
        {
            public int Foo;
            public int Bar;
        }

        [Test]
        public void CustomMemberWriterReturningNullMeansUseDefaultWriter()
        {
            var subj = new Test1();
            var writer = new ObjectLiteralWriter 
            { 
                CustomMemberWriter = (propInfo, fieldInfo, target) => null, 
                SkipMembersWithDefaultValue = false
            };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual(
@"new Test1()
{
Bar = 0,
Foo = 0,
}", output);
        }

        [Test]
        public void CustomMemberWriterReturningAnyStringMeansLiteralIsHandled()
        {
            var subj = new Test1();
            var writer = new ObjectLiteralWriter
            {
                CustomMemberWriter = (propInfo, fieldInfo, target) =>
                {
                    if (fieldInfo.Name == "Foo")
                    {
                        return "Foo = 1,\r\n";
                    }

                    return null;
                },
                SkipMembersWithDefaultValue = false
            };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual(
@"new Test1()
{
Bar = 0,
Foo = 1,
}", output);
        }

        [Test]
        public void CustomMemberWriterReturningEmptyStringMeansSkipMember()
        {
            var subj = new Test1();
            var writer = new ObjectLiteralWriter
            {
                CustomMemberWriter = (propInfo, fieldInfo, target) =>
                {
                    if (fieldInfo.Name == "Foo")
                    {
                        return string.Empty;
                    }

                    return null;
                },
                SkipMembersWithDefaultValue = false
            };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual(
@"new Test1()
{
Bar = 0,
}", output);
        }

        public class Test2
        {
            public int AAA;
            public int BBB;
            public int CCC { get; set; }
            public int DDD{ get; set; }
        }

        [Test]
        public void CustomPropertyOrder()
        {
            var subj = new Test2();
            
             var writer = new ObjectLiteralWriter 
            { 
                SkipMembersWithDefaultValue = false
            };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual(
@"new Test2()
{
AAA = 0,
BBB = 0,
CCC = 0,
DDD = 0,
}", output);

            writer = new ObjectLiteralWriter 
            { 
                FieldOrderer = (fields, target) => fields.OrderByDescending(x => x.Name),
                PropertyOrderer = (props, target) => props.OrderByDescending(x => x.Name),
                SkipMembersWithDefaultValue = false
            };
            output = writer.GetLiteral(subj);
            Assert.AreEqual(
@"new Test2()
{
BBB = 0,
AAA = 0,
DDD = 0,
CCC = 0,
}", output);
        }

    }
}