namespace CastleCrushers.Tests
{
    using CastleCrushers;
    using System.Collections.Generic;
    using Util.Geometry;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class SweepTest
    {

        public SweepTest()
        {
            //Test1();
            Test3();
        }
        
        public void Test1()
        {
            List<LineObject> lines = new List<LineObject>()
            {
                new LineObject(new LineSegment(new Vector2(1,3), new Vector2(3,1)), null),
                new LineObject(new LineSegment(new Vector2(1.1f,1.1f), new Vector2(2.9f,2.9f)), null)
            };
            DownwardSweepLine sweep = new DownwardSweepLine(lines);
            Assert.AreEqual(sweep.Run().Count, 1);
        }

        public void Test2()
        {
            List<LineObject> lines = new List<LineObject>()
            {
                new LineObject(new LineSegment(new Vector2(1,3), new Vector2(3,2)), null),
                new LineObject(new LineSegment(new Vector2(1,2), new Vector2(3,1)), null),
                new LineObject(new LineSegment(new Vector2(1.1f,1.1f), new Vector2(2.9f,2.9f)), null)
            };
            DownwardSweepLine sweep = new DownwardSweepLine(lines);
            Assert.AreEqual(sweep.Run().Count, 2);
        }

        public void Test3()
        {
            List<LineObject> lines = new List<LineObject>();
            for (int i = 0; i <=10; i++)
            {
                lines.Add(new LineObject(new LineSegment(new Vector2(i, 0), new Vector2(10-i, 1)), null));
            }
            DownwardSweepLine sweep = new DownwardSweepLine(lines);
            Assert.AreEqual(sweep.Run().Count, 11*10/2);
        }
    }
}