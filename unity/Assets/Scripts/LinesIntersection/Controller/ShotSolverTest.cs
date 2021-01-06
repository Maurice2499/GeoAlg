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
        }

        public void Test1()
        {
            List<LineSegment> lst = new List<LineSegment>()
            {
                new LineSegment(new Vector2(0,0), new Vector2(1,1)),
                new LineSegment(new Vector2(0,1), new Vector2(1,2))
            };

            ShotSolver solver = new ShotSolver(lst);
            Assert.AreEqual(solver.GreedyCover(), 1);
        }

        public void Test2()
        {
            List<LineObject> lines = new List<LineObject>()
            {
                new LineObject(new LineSegment(new Vector2(0,0), new Vector2(10,9)), null),
                new LineObject(new LineSegment(new Vector2(2,1), new Vector2(3,8)), null),
                new LineObject(new LineSegment(new Vector2(3,9), new Vector2(6,1)), null),
                new LineObject(new LineSegment(new Vector2(1,2), new Vector2(4,8)), null),
                new LineObject(new LineSegment(new Vector2(5,5), new Vector2(6,8)), null),
                new LineObject(new LineSegment(new Vector2(1,6), new Vector2(9,5)), null)
            };
            ShotSolver solver = new ShotSolver(lines);
            Debug.LogWarning("Initialized that solver thingy:)");
            Assert.AreEqual(solver.GreedyCover(), 1);
        }
    }
}