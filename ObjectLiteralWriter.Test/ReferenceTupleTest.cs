using System;
using System.Linq;
using NUnit.Framework;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]
    public class ReferenceTupleTest
    {
        [Test]
        public void CanHandleTuple1()
        {
            var subj = Tuple.Create(1);
            Util.AssertTypeLiteral(subj,
@"new Tuple<Int32>(1)");
        }

        [Test]
        public void CanHandleTuple2()
        {
            var subj = Tuple.Create(1, 2);
            Util.AssertTypeLiteral(subj,
@"new Tuple<Int32, Int32>(1, 2)");
        }

        [Test]
        public void CanHandleTuple3()
        {
            var subj = Tuple.Create(1, 2, 3);
            Util.AssertTypeLiteral(subj,
@"new Tuple<Int32, Int32, Int32>(1, 2, 3)");
        }

        [Test]
        public void CanHandleTuple4()
        {
            var subj = Tuple.Create(1, 2, 3, 4);
            Util.AssertTypeLiteral(subj,
@"new Tuple<Int32, Int32, Int32, Int32>(1, 2, 3, 4)");
        }

        [Test]
        public void CanHandleTuple5()
        {
            var subj = Tuple.Create(1, 2, 3, 4, 5);
            Util.AssertTypeLiteral(subj,
@"new Tuple<Int32, Int32, Int32, Int32, Int32>(1, 2, 3, 4, 5)");
        }

        [Test]
        public void CanHandleTuple6()
        {
            var subj = Tuple.Create(1, 2, 3, 4, 5, 6);
            Util.AssertTypeLiteral(subj,
@"new Tuple<Int32, Int32, Int32, Int32, Int32, Int32>(1, 2, 3, 4, 5, 6)");
        }

        [Test]
        public void CanHandleTuple7()
        {
            var subj = Tuple.Create(1, 2, 3, 4, 5, 6, 7);
            Util.AssertTypeLiteral(subj,
@"new Tuple<Int32, Int32, Int32, Int32, Int32, Int32, Int32>(1, 2, 3, 4, 5, 6, 7)");
        }

        [Test]
        public void CanHandleTuple8()
        {
            var subj = Tuple.Create(1, 2, 3, 4, 5, 6, 7, 8);
            Util.AssertTypeLiteral(subj,
@"new Tuple<Int32, Int32, Int32, Int32, Int32, Int32, Int32, Tuple<Int32>>(1, 2, 3, 4, 5, 6, 7, new Tuple<Int32>(8))");
        }

        [Test]
        public void CanHandleMixedTuple8()
        {
            var subj = Tuple.Create(1, 2D, 3M, true, new object(), (object)null, (int?)null, 0);
            Util.AssertTypeLiteral(subj,
@"new Tuple<Int32, Double, Decimal, Boolean, Object, Object, Int32?, Tuple<Int32>>(1, 2D, 3M, true, new object(), null, null, new Tuple<Int32>(0))");
        }
    }
}