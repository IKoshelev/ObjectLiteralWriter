using NUnit.Framework;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]
    internal class IndentLiteralTest
    {
        [Test]
        public void CanIndentLiteral()
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

            System.Console.WriteLine(literal.IndentLiteral());

            Assert.AreEqual(literal.IndentLiteral(), indentedLiteral);
        }
    }
}