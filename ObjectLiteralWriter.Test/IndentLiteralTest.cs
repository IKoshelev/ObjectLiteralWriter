using System;
using NUnit.Framework;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]
    internal class IndentLiteralTest
    {
        [Test]
        public void CanIndentExistingLiteral()
        {
            var literal = @"new Object[]
{
new Test1()
{
Foo = 1.1M,
Bar = null,
Bac = (1, 2, 3),
},
null,
1,
2D,
3M,
true,
new object(),
}";

            var indentedLiteral = @"new Object[]
{
    new Test1()
    {
        Foo = 1.1M,
        Bar = null,
        Bac = (1, 2, 3),
    },
    null,
    1,
    2D,
    3M,
    true,
    new object(),
}";

            Assert.AreEqual(literal.IndentLiteral(), indentedLiteral);
        }

        [Test]
        public void CanIndentLiteralDuringCreation()
        {
            var subj = new Object[]
            {
                new Test1()
                {
                    Foo = 1.1M,
                    Bar = null,
                    Bac = (1, 2, 3),
                },
                null,
                1,
                2D,
                3M,
                true,
                new object(),
            };

            var indentedLiteral = @"new Object[]
{
  new Test1()
  {
    Bac = (1, 2, 3),
    Foo = 1.1M,
  },
  null,
  1,
  2D,
  3M,
  true,
  new object(),
}";

            var literal = new ObjectLiteralWriter().GetLiteral(subj, indentation: "  ");

            Assert.AreEqual(literal, indentedLiteral);
        }

        private class Test1
        {
            public Test1()
            {
            }

            public decimal Foo { get; set; }
            public object Bar { get; set; }
            public (int, int, int) Bac { get; set; }
        }
    }
}