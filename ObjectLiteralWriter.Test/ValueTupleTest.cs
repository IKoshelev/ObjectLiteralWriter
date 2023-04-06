using System;
using System.Linq;
using NUnit.Framework;

namespace ObjectLiteralWriter.Test
{
    [TestFixture]
    public class ValueTupleTest
    {
        [Test]
        public void CanHandleTuple2()
        {
            var subj = (1, 2);
            Util.AssertTypeLiteral(subj,
@"(1, 2)");
        }

        [Test]
        public void CanHandleTuple3()
        {
            var subj = (1, 2, 3);
            Util.AssertTypeLiteral(subj,
@"(1, 2, 3)");
        }

        [Test]
        public void CanHandleTuple4()
        {
            var subj = (1, 2, 3, 4);
            Util.AssertTypeLiteral(subj,
@"(1, 2, 3, 4)");
        }

        [Test]
        public void CanHandleTuple5()
        {
            var subj = (1, 2, 3, 4, 5);
            Util.AssertTypeLiteral(subj,
@"(1, 2, 3, 4, 5)");
        }

        [Test]
        public void CanHandleTuple6()
        {
            var subj = (1, 2, 3, 4, 5, 6);
            Util.AssertTypeLiteral(subj,
@"(1, 2, 3, 4, 5, 6)");
        }

        [Test]
        public void CanHandleTuple7()
        {
            var subj = (1, 2, 3, 4, 5, 6, 7);
            Util.AssertTypeLiteral(subj,
@"(1, 2, 3, 4, 5, 6, 7)");
        }

        [Test]
        public void CanHandleTuple8()
        {
            var subj = (1, 2, 3, 4, 5, 6, 7, 8);
            Util.AssertTypeLiteral(subj,
@"(1, 2, 3, 4, 5, 6, 7, 8)");
        }

        [Test]
        public void CanHandleMixedTuple8()
        {
            var subj = (1, 2D, 3M, true, new object(), (object)null, (int?)null, 0);
            Util.AssertTypeLiteral(subj,
@"(1, 2D, 3M, true, new object(), null, null, 0)");
        }

        [Test]
        public void CanHandleTuple25()
        {
            var subj = (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25);
            Util.AssertTypeLiteral(subj,
@"(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25)");
        }
    }
}