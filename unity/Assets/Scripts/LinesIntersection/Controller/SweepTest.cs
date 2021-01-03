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
            Test1();
            Test2();
            //Test3();
            Test4();
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
            // ONLY RUN THIS for the degenrate case that there are multiple coinciding event points. So far, we have not solved this :)
            for (int N = 2; N <= 20; N = N + 2)
            {
                Debug.LogWarning("N=" + N);
                List<LineObject> lines = new List<LineObject>();
                for (int i = 0; i <= N; i++)
                {
                    lines.Add(new LineObject(new LineSegment(new Vector2(i, 0), new Vector2(10 - i, 1)), null));
                }
                DownwardSweepLine sweep = new DownwardSweepLine(lines);
                Assert.AreEqual(sweep.Run().Count, (N * (N+1)) / 2);
            }
        }

        public void Test4()
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
            DownwardSweepLine sweep = new DownwardSweepLine(lines);
            Assert.AreEqual(sweep.Run().Count, 9);
        }
    }
}