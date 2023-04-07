using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]
    public class CollectionTest
    {
        [Test]
        public void CanHandleDictionaryEmpty()
        {
            var subj = new Dictionary<string, int>()
            {
            };

            Util.AssertTypeLiteral(subj,
@"new Dictionary<String,Int32>()
{
}");
        }

        [Test]
        public void CanHandleDictionary()
        {
            var subj = new Dictionary<string, int>()
            {
                {"1", 2},
                {"3", 4}
            };

            Util.AssertTypeLiteral(subj,
@"new Dictionary<String,Int32>()
{
{
""1"",2
},
{
""3"",4
},
}");
        }

        public class Test1
        {
            public decimal Foo;

            public Test1 Bar;
        }

        [Test]
        public void CanHandleDictionaryOfComplexClasses()
        {
            var subj = new Dictionary<decimal, Test1>()
            {
                {1.1m, new Test1()
                {
                    Foo = 1.1m,
                    Bar = new Test1()
                    {
                        Foo = -3.3m
                    }
                }},
                {-2.2m, null}
            };

            Util.AssertTypeLiteral(subj,
@"new Dictionary<Decimal,Test1>()
{
{
1.1M,new Test1()
{
Foo = 1.1M,
Bar = new Test1()
{
Foo = -3.3M,
Bar = null,
},
}
},
{
-2.2M,null
},
}");

            Util.AssertTypeLiteral(subj,
@"new Dictionary<Decimal,Test1>()
{
{
1.1M,new Test1()
{
Foo = 1.1M,
Bar = new Test1()
{
Foo = -3.3M,
},
}
},
{
-2.2M,null
},
}", true);
        }

        [Test]
        public void CanHandleListEmpty()
        {
            var subj = new List<decimal>()
            {
            };

            Util.AssertTypeLiteral(subj,
@"new List<Decimal>()
{
}");
        }

        [Test]
        public void CanHandleList()
        {
            var subj = new List<decimal>()
            {
                -4.123456789m,
                0
            };

            Util.AssertTypeLiteral(subj,
@"new List<Decimal>()
{
-4.123456789M,
0M,
}");
        }

        [Test]
        public void CanHandleListOfComplexClasses()
        {
            var subj = new List<Test1>()
            {
                new Test1()
                    {
                        Foo = 1.1m,
                        Bar = new Test1()
                        {
                            Foo = -3.3m
                        }
                    }
                ,
                null
            };

            Util.AssertTypeLiteral(subj,
@"new List<Test1>()
{
new Test1()
{
Foo = 1.1M,
Bar = new Test1()
{
Foo = -3.3M,
Bar = null,
},
},
null,
}");

            Util.AssertTypeLiteral(subj,
@"new List<Test1>()
{
new Test1()
{
Foo = 1.1M,
Bar = new Test1()
{
Foo = -3.3M,
},
},
null,
}", true);
        }

        [Test]
        public void CanHandleArrayEmpty()
        {
            var subj = new decimal[]
            {
            };

            Util.AssertTypeLiteral(subj,
@"new Decimal[]
{
}");
        }

        [Test]
        public void CanHandleArray()
        {
            var subj = new decimal[]
            {
                -4.123456789m,
                0
            };

            Util.AssertTypeLiteral(subj,
@"new Decimal[]
{
-4.123456789M,
0M,
}");
        }

        [Test]
        public void CanHandleArrayOfComplexClasses()
        {
            var subj = new Test1[]
            {
                new Test1()
                {
                    Foo = 1.1m,
                    Bar = new Test1()
                    {
                        Foo = -3.3m
                    }
                }
                ,
                null
            };

            Util.AssertTypeLiteral(subj,
@"new Test1[]
{
new Test1()
{
Foo = 1.1M,
Bar = new Test1()
{
Foo = -3.3M,
Bar = null,
},
},
null,
}");

            Util.AssertTypeLiteral(subj,
@"new Test1[]
{
new Test1()
{
Foo = 1.1M,
Bar = new Test1()
{
Foo = -3.3M,
},
},
null,
}", true);
        }

        [Test]
        public void CanHandleArrayOfMixedTypes()
        {
            var subj = new object[]
            {
                new Test1()
                {
                    Foo = 1.1m,
                    Bar = null
                },
                null,
                1,
                2D,
                3M,
                true,
                new object(),
            };

            Util.AssertTypeLiteral(subj,
@"new Object[]
{
new Test1()
{
Foo = 1.1M,
Bar = null,
},
null,
1,
2D,
3M,
true,
new object(),
}");
        }
    }
}