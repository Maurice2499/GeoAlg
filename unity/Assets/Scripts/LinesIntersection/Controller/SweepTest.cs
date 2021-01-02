namespace CastleCrushers.Tests
{
    using CastleCrushers;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using Util.Algorithms.Graph;
    using Util.Geometry;
    using Util.Geometry.Graph;
    using Util.Math;
    using UnityEngine;

    [TestFixture]
    public class SweepTest
    {
        private List<LineObject> lines;
        private DownwardSweepLine sweep;

        public SweepTest()
        {
            lines = new List<LineObject>()
            {
                new LineObject(new LineSegment(new Vector2(1,3), new Vector2()), null),
                new LineObject(new LineSegment(new Vector2(), new Vector2()), null),
                new LineObject(new LineSegment(new Vector2(), new Vector2()), null)
            };
            sweep = new DownwardSweepLine(lines);
        }

        [Test]
        public void Test1()
        {
            return;

            //Assert.AreEqual(cost, mst.TotalEdgeWeight, MathUtil.EPS);
        }
    }
}