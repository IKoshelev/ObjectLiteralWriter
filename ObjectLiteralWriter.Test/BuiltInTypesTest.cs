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
            Etalone.Booleans.AssertEtaloneMatch();
        }

        [Test]
        public void CanHandleNumerics()
        {
            Etalone.Numerics.AssertEtaloneMatch();
        }

        [Test]
        public void CanHandleText()
        {
            Etalone.Text.AssertEtaloneMatch();
        }

        [Test]
        public void CanHandleEnums()
        {
            Etalone.Enums.AssertEtaloneMatch();
        }

        [Test]
        public void CanHandleDateTime()
        {
            Etalone.Time.AssertEtaloneMatch();
        }
    }
}