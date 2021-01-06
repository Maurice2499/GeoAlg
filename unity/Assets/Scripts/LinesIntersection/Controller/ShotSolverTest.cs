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
    }
}