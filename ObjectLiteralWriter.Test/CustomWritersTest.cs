﻿using System;
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
            var writer = new ObjectLiteralWriter { CustomLiteralWriter = (type, targer) => null };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual("new object()", output);
        }

        [Test]
        public void CustomLiteralWriterReturningAnyStringMeansLiteralIsHandled()
        {
            var subj = new object();
            var writer = new ObjectLiteralWriter { CustomLiteralWriter = (type, targer) => string.Empty };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual("", output);

            writer = new ObjectLiteralWriter { CustomLiteralWriter = (type, targer) => "FOOBAR" };
            output = writer.GetLiteral(subj);
            Assert.AreEqual("FOOBAR", output);
        }

        public class Test1
        {
            public int Foo;
            public int Bar;
        }

        [Test]
        public void CustoMemberWriterReturningNullMeansUseDefaultWriter()
        {
            var subj = new Test1();
            var writer = new ObjectLiteralWriter { CustomMemberWriter = (propInfo, fieldInfo, target) => null };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual(
@"new Test1()
{
Foo = 0,
Bar = 0,
}", output);
        }

        [Test]
        public void CustoMemberWriterReturningAnyStringMeansLiteralIsHandled()
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
                }
            };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual(
@"new Test1()
{
Foo = 1,
Bar = 0,
}", output);
        }

        [Test]
        public void CustoMemberWriterReturningEmptyStringMeansSkipMember()
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
                }
            };
            var output = writer.GetLiteral(subj);
            Assert.AreEqual(
@"new Test1()
{
Bar = 0,
}", output);
        }
    }
}