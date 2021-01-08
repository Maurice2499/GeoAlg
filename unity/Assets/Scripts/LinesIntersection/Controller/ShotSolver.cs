using MathNet.Numerics;
using System;
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

        private const float EDGE_RATIO = 19;

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
                endpoints[2 * i] =  (lines[i].Point1 *EDGE_RATIO + lines[i].Point2)/(EDGE_RATIO+1);
                endpoints[2 * i + 1] = (lines[i].Point1 + lines[i].Point2 * EDGE_RATIO) / (EDGE_RATIO + 1);
            }
            sets = new bool[2 * N, 2 * N, N];
            CreateSets();
        }

        public void CreateSets()
        {
            for (int i = 0; i < 2 * lines.Length; i++)
            {
                for (int j = i + 2 - (i%2); j < 2 * lines.Length; j++)
                {

                    Line PossibleShot = new Line(endpoints[i], endpoints[j]);

                    for (int k = 0; k < lines.Length; k++)
                    {
                        // Exlude line through endpoints
                        sets[i, j, k] = (lines[k].IntersectProper(PossibleShot) != null);
                    }
                }
            }
        }

        private struct Index
        {
            public int i;
            public int j;
            public int count;

            public Index(int i, int j, int count)
            {
                this.i = i;
                this.j = j;
                this.count = count;
            }
        }
        private Index GetHighestIndex(int[,] counts)
        {
            int max_i = -1;
            int max_j = -1;
            int max_count = -1;
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
            return new Index(max_i, max_j, max_count);
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
            // TODO: check: or if N <= 1?
            if (N <= 2)
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
                        if (k == i/2 || k == j/2 || sets[i, j, k] == true)
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

            // TODO fix
            int x = N;
            while (IsAllTrue(covered) == false && x > 0)
            {
                shots += 1;
                x -= 1;
                
                Index highest = GetHighestIndex(counts);
                int i = highest.i;
                int j = highest.j;
                if (highest.count == 0)
                {
                    // Then, there are no more sets to place
                    for (int k = 0; k < N; k++)
                    {
                        if (covered[k] == false)
                        {
                            covered[k] = true;
                            break;
                        }
                    }
                    continue;
                }

                for (int k = 0; k < N; k++)
                {
                    if (covered[k] == false && sets[i, j, k] == true)
                    {
                        covered[k] = true;
                        for (int a = 0; a < 2 * N; a++)
                        {
                            for (int b = a + 2 - (a % 2); b < 2 * N; b++)
                            {
                                if (sets[a, b, k] == true)
                                {
                                    counts[a, b] -= 1;
                                }
                            }
                        }
                    }
                }
            }

            return shots;
        }
    }
}
