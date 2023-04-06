using System;
using NUnit.Framework;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]

    // List of built-in types: https://msdn.microsoft.com/en-us/library/ya5y69ds.aspx
    public class BuiltInTypesTest
    {
        [Test]
        public void CanHandleBooleans()
        {
            Etalon.Booleans.AssertEtalonMatch();
        }

        [Test]
        public void CanHandleNumerics()
        {
            Etalon.Numerics.AssertEtalonMatch();
        }

        [Test]
        public void CanHandleText()
        {
            Etalon.Text.AssertEtalonMatch();
        }

        [Test]
        public void CanHandleEnums()
        {
            Etalon.Enums.AssertEtalonMatch();
        }

        [Test]
        public void CanHandleDateTime()
        {
            Etalon.Time.AssertEtalonMatch();
        }

        [Test]
        public void CanHandleGuid()
        {
            Etalon.Guid.AssertEtalonMatch();
        }
    }
}