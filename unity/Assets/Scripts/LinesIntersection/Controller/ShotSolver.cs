using MathNet.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;

namespace CastleCrushers
{
    public class ShotSolver
    {
        // OR do we want Line here instead of LineSegment? Idk, but efficiency is nice here. Thats also why array
        public LineSegment[] lines;
        private int N;
        public Vector2[] endpoints;

        public bool[,,] sets; // at sets[i][j][k] is the boolean whether the k-th line is shot by the i to j th line
        // Note: sets[i][j] only maintained iff i<j

        public ShotSolver(List<LineSegment> lines)
        {
            this.lines = lines.ToArray();
            Init();
        }

        public ShotSolver(List<LineObject> lineObjects)
        {
            List<LineSegment> list = new List<LineSegment>();
            foreach (LineObject lineObject in lineObjects)
            {
                list.Add(lineObject.line);
            }
            this.lines = list.ToArray();
            Init();
        }

        private void Init()
        {
            N = lines.Length;
            endpoints = new Vector2[2 * N];
            for (int i = 0; i < N; i++)
            {
                endpoints[2 * i] = lines[i].Point1;
                endpoints[2 * i + 1] = lines[i].Point2;
            }
            sets = new bool[2 * N, 2 * N, N];
        }

        public void CreateSets()
        {
            for (int i = 0; i < 2 * lines.Length; i++)
            {
                for (int j = i + 2 - (i%2); j < 2 * lines.Length; j++)
                {
                    sets[i, j, i] = true;
                    sets[i, j, j] = true;

                    LineSegment PossibleShot = new LineSegment(endpoints[i], endpoints[j]);

                    for (int k = 0; k < lines.Length; k++)
                    {
                        if (k != i && k != j)
                        {
                            sets[i, j, k] = (PossibleShot.Intersect(lines[k]) != null);
                        }
                    }
                }
            }
        }

        private struct Index
        {
            public int i;
            public int j;

            public Index(int i, int j)
            {
                this.i = i;
                this.j = j;
            }
        }
        private Index GetHighestIndex(int[,] counts)
        {
            int max_i = 0;
            int max_j = 2;
            int max_count = counts[max_i, max_j];
            for (int i = 0; i < 2 * N; i++)
            {
                for (int j = i + 2 - (i % 2); j < 2 * N; j++)
                {
                    if (counts[i, j] > max_count)
                    {
                        max_i = i;
                        max_j = j;
                        max_count = counts[i, j]; 
                    }
                }
            }
            return new Index(max_i, max_j);
        }

        private bool IsAllTrue(bool[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == false)
                {
                    return false;
                }
            }
            return true;
        }

        // Returns the number of shots that are required to cover all lines greedily
        public int GreedyCover()
        {
            if (lines.Length <= 2)
            {
                return 1;
            }

            int shots = 0;
            int[,] counts = new int[2 * N, 2 * N];
            bool[] covered = new bool[N];

            for (int i = 0; i < 2 * N; i++)
            {
                for (int j = i + 2 - (i % 2); j < 2 * N; j++)
                {
                    counts[i, j] = 0;
                    for (int k = 0; k < N; k++)
                    {
                        if (k != i && k != j)
                        {
                            counts[i, j] += 1;
                        }
                    }
                }
            }
            for (int i = 0; i < N; i++)
            {
                covered[i] = false;
            }

            return 0;
            // TODO fix
            while (IsAllTrue(covered) == false)
            {
                shots += 1;
                Index highest = GetHighestIndex(counts);
                int i = highest.i;
                int j = highest.j;
                for (int k = 0; k < N; k++)
                {
                    if (!covered[k] && sets[i, j, k])
                    {
                        covered[k] = true;
                        for (int a = 0; i < 2 * N; i++)
                        {
                            for (int b = i + 2 - (i % 2); j < 2 * N; j++)
                            {
                                sets[a, b, k] = false;
                            }
                        }
                    }
                }
            }

            return shots;
        }
    }
}
