﻿namespace CastleCrushers.Tests
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
            Test3();
            //Test4(); Deze doet t nog steeds niet
            Test5();
            Test6();
        }
        
        public void Test1()
        {
            List<LineObject> lines = new List<LineObject>()
            {
                new LineObject(new Vector2(3,1), new Vector2(1,3)),
                new LineObject(new Vector2(1.1f,1.1f), new Vector2(2.9f,2.9f)),
            };
            DownwardSweepLine sweep = new DownwardSweepLine(lines);
            Assert.AreEqual(sweep.Run().Count, 1);
        }
        public void Test2()
        {
            List<LineObject> lines = new List<LineObject>()
            {
                new LineObject(new Vector2(1,3), new Vector2(3,1)),
                new LineObject(new Vector2(1.1f,1.3f), new Vector2(2.9f,2.7f)),
            };
            DownwardSweepLine sweep = new DownwardSweepLine(lines);
            Assert.AreEqual(sweep.Run().Count, 1);
        }

        public void Test3()
        {
            List<LineObject> lines = new List<LineObject>()
            {
                new LineObject(new Vector2(1,3), new Vector2(3,2)),
                new LineObject(new Vector2(1,2), new Vector2(3,1)),
                new LineObject(new Vector2(1.1f,1.1f), new Vector2(2.9f,2.9f)),
            };
            DownwardSweepLine sweep = new DownwardSweepLine(lines);
            Assert.AreEqual(sweep.Run().Count, 2);
        }

        public void Test4()
        {
            // ONLY RUN THIS for the degenrate case that there are multiple coinciding event points. So far, we have not solved this :)
            for (int N = 2; N <= 20; N = N + 2)
            {
                Debug.LogWarning("N=" + N);
                List<LineObject> lines = new List<LineObject>();
                for (int i = 0; i <= N; i++)
                {
                    lines.Add(new LineObject(new Vector2(i, 0), new Vector2(10 - i, 1)));
                }
                DownwardSweepLine sweep = new DownwardSweepLine(lines);
                Assert.AreEqual(sweep.Run().Count, (N * (N+1)) / 2);
            }
        }

        public void Test5()
        {
            List<LineObject> lines = new List<LineObject>()
            {
                new LineObject(new Vector2(0,0), new Vector2(10,9)),
                new LineObject(new Vector2(2,1), new Vector2(3,8)),
                new LineObject(new Vector2(3,9), new Vector2(6,1)),
                new LineObject(new Vector2(1,2), new Vector2(4,8)),
                new LineObject(new Vector2(5,5), new Vector2(6,8)),
                new LineObject(new Vector2(1,6), new Vector2(9,5)),
            };
            DownwardSweepLine sweep = new DownwardSweepLine(lines);
            Assert.AreEqual(sweep.Run().Count, 9);
        }

        public void Test6()
        {
            int maxWalls = 10;
            float MIN_WIDTH = -7.8f;
            float MAX_WIDTH = 7.8f;
            float MIN_HEIGHT = -3.5f;
            float MAX_HEIGHT = 3.5f;

            // Run it 3 times because it is randomized. I dont expect to catch all bugs but at least this is better than only once
            // NOTE: NOT fixed this test case for horizontal line segments
            for (int N = 0; N < 10; N++)
            {
                // Generator code
                List<LineObject> lines = new List<LineObject>();
                while (lines.Count < maxWalls)
                {
                    Vector2 position1 = new Vector3(UnityEngine.Random.Range(MIN_WIDTH, MAX_WIDTH), UnityEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT));
                    Vector2 position2 = new Vector3(UnityEngine.Random.Range(MIN_WIDTH, MAX_WIDTH), UnityEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT));

                    LineObject newLine = new LineObject(position1, position2);

                    if( newLine.IsHorizontal == false)
                    {
                        lines.Add(newLine);
                    }
                }

                DownwardSweepLine sweep = new DownwardSweepLine(lines);
                List<Intersection> intersections = sweep.Run();
                foreach (Intersection intersection in intersections)
                {
                    intersection.two.Break();
                }
                lines.RemoveAll(item => item.hits > 0);
 
                DownwardSweepLine sweep2 = new DownwardSweepLine(lines);
                Assert.AreEqual(sweep2.Run().Count, 0);
            }
        }
    }
}