using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Util.Geometry;

namespace CastleCrushers.Tests
{
    public class ShotSolverTest
    {

        public ShotSolverTest()
        {
            Test1();
            Test2();
            Test3();
            Test4();
            Test5();
            Test6();
            Test7();
            Test8();
        }

        public void Test1()
        {
            List<LineSegment> lst = new List<LineSegment>()
            {
                new LineSegment(new Vector2(0,0), new Vector2(1,1)),
                new LineSegment(new Vector2(0,1), new Vector2(1,2))
            };

            ShotSolver solver = new ShotSolver(lst);
            Assert.AreEqual(solver.GreedyCover().Count, 1);
        }

        public void Test2()
        {
            List<LineSegment> lst = new List<LineSegment>()
            {
                new LineSegment(new Vector2(0,0), new Vector2(1,0)),
                new LineSegment(new Vector2(1,1), new Vector2(2,1)),
                new LineSegment(new Vector2(2,0), new Vector2(3,0))
            };

            ShotSolver solver = new ShotSolver(lst);
            Assert.AreEqual(solver.GreedyCover().Count, 2);
        }

        public void Test3()
        {
            List<LineObject> lines = new List<LineObject>()
            {
                new LineObject(new Vector2(0,0), new Vector2(10,9)),
                new LineObject(new Vector2(2,1), new Vector2(3,8)),
                new LineObject(new Vector2(3,9), new Vector2(6,1)),
                new LineObject(new Vector2(1,2), new Vector2(4,8)),
                new LineObject(new Vector2(5,5), new Vector2(6,8)),
                new LineObject(new Vector2(1,6), new Vector2(9,5))
            };
            ShotSolver solver = new ShotSolver(lines);
            Assert.AreEqual(solver.GreedyCover().Count, 1);
        }

        public void Test4()
        {
            List<LineSegment> lst = new List<LineSegment>()
            {
                new LineSegment(new Vector2(0,0), new Vector2(1,0)),
                new LineSegment(new Vector2(2,1), new Vector2(3,1)),
                new LineSegment(new Vector2(4,0), new Vector2(5,0)),
                new LineSegment(new Vector2(0,2), new Vector2(1,2)),
                new LineSegment(new Vector2(4,2), new Vector2(5,2))
            };

            ShotSolver solver = new ShotSolver(lst);
            Assert.AreEqual(solver.GreedyCover().Count, 2);
        }

        public void Test5()
        {
            List<LineSegment> lst = new List<LineSegment>()
            {
                new LineSegment(new Vector2(0,0), new Vector2(1,0)),
                new LineSegment(new Vector2(2,1), new Vector2(3,1)),
                new LineSegment(new Vector2(4,0), new Vector2(5,0)),
                new LineSegment(new Vector2(0,2), new Vector2(1,2)),
                new LineSegment(new Vector2(4,2), new Vector2(5,2)),
                new LineSegment(new Vector2(2,3), new Vector2(3,3))

            };

            ShotSolver solver = new ShotSolver(lst);
            Assert.AreEqual(solver.GreedyCover().Count, 3);
        }

        public void Test6()
        {
            List<LineSegment> lst = new List<LineSegment>()
            {
                new LineSegment(new Vector2(0,0), new Vector2(1,1)),
                new LineSegment(new Vector2(4,1), new Vector2(6,0)),
                new LineSegment(new Vector2(2,3), new Vector2(3,2)),
                new LineSegment(new Vector2(0,3), new Vector2(1,4)),
                new LineSegment(new Vector2(4,4), new Vector2(5,3)),
                new LineSegment(new Vector2(6,5), new Vector2(7,4))

            };

            ShotSolver solver = new ShotSolver(lst);
            Assert.AreEqual(solver.GreedyCover().Count, 2);
        }

        public void Test7()
        {
            List<LineSegment> lst = new List<LineSegment>()
            {
                new LineSegment(new Vector2(0,0), new Vector2(1,1)),
                new LineSegment(new Vector2(4,1), new Vector2(6,0)),
                new LineSegment(new Vector2(2,3), new Vector2(3,2)),
                new LineSegment(new Vector2(0,3), new Vector2(1,4)),
                new LineSegment(new Vector2(4,4), new Vector2(5,3)),
                new LineSegment(new Vector2(6,5), new Vector2(7,4))

            };

            ShotSolver solver = new ShotSolver(lst);
            for (int i = 0; i < solver.N; i++)
            {
                Vector2 e1 = solver.endpoints[2 * i];
                Vector2 e2 = solver.endpoints[2 * i + 1];

                Vector2 p1 = solver.lines[i].Point1;
                Vector2 p2 = solver.lines[i].Point2;

                Assert.AreApproximatelyEqual((e1 + e2).x, (p1 + p2).x, 1e-6f);
                Assert.AreApproximatelyEqual((e1 + e2).y, (p1 + p2).y, 1e-6f);
            }
            Assert.AreEqual(solver.GreedyCover().Count, 2);
        }

        public void Test8()
        {
            List<LineSegment> lst = new List<LineSegment>()
            {
                new LineSegment(new Vector2(0,0), new Vector2(1,1)),
                new LineSegment(new Vector2(2,2), new Vector2(3,3)),
                new LineSegment(new Vector2(4,4), new Vector2(5,5))
            };
            ShotSolver solver = new ShotSolver(lst);
            Assert.AreEqual(solver.GreedyCover().Count, 3);
        }
    }
}