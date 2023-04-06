using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ObjectLiteralWriter;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]
    internal class BaseTest
    {
        [Test]
        public void CanHandleNull()
        {
            var writer = new ObjectLiteralWriter();
            var output = writer.GetLiteral(null);
            Assert.AreEqual("null", output);
        }

        [Test]
        public void CanHandleObject()
        {
            var writer = new ObjectLiteralWriter();
            var output = writer.GetLiteral(new Object());
            Assert.AreEqual("new object()", output);
        }

        [Test]
        public void EmitsGivenPropertyName()
        {
            var writer = new ObjectLiteralWriter();
            var output = writer.GetLiteral(new Object(), "foo");
            Assert.AreEqual("var foo = new object()", output);
        }

        [Test]
        public void CanHandlePropertyNameWithNullTarget()
        {
            var writer = new ObjectLiteralWriter();
            var output = writer.GetLiteral(null, "foo");
            Assert.AreEqual("object foo = null", output);
        }

        [Test]
        public void CanHandleMultipleInARow()
        {
            var writer = new ObjectLiteralWriter();
            var output = writer.GetLiteral(new Object());
            Assert.AreEqual(output, "new object()");
            output = writer.GetLiteral(null);
            Assert.AreEqual("null", output);
        }
    }
}