using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Algorithms;
using Util.DataStructures.BST;
using Util.Geometry;

namespace CastleCrushers
{
    public struct Intersection
    {
        public LineObject one;
        public LineObject two;

        public Intersection(LineObject one, LineObject two)
        {
            this.one = one;
            this.two = two;
        }
    }

    public enum EventType {
        INSERT,
        DELETE,
        INTERSECT
    }

    public class SweepEvent : ISweepEvent<StatusItem>, IComparable<SweepEvent>, IEquatable<SweepEvent>
    {
        public SweepEvent(EventType type)
        {
            this.EventType = type;
        }

        public Vector2 Pos
        {
            get
            {
                if (this.IsStart)
                {
                    return this.StatusItem.LineObject.Highest();
                }
                else if (this.IsEnd)
                {
                    return this.StatusItem.LineObject.Lowest();
                }
                else //if (this.IsIntersection)
                {
                    return (Vector2)StatusItem.LineObject.line.Intersect(IntersectingStatusItem.LineObject.line);
                }
            }
        }

        public EventType EventType;

        public StatusItem StatusItem { get; set; }

        public StatusItem IntersectingStatusItem { get; set; }

        public bool IsStart
        {
            get
            {
                return this.EventType == EventType.INSERT;
            }
        }

        public bool IsEnd
        {
            get
            {
                return this.EventType == EventType.DELETE;
            }
        }

        public bool IsIntersection
        {
            get
            {
                return this.EventType == EventType.INTERSECT;
            }
        }

        public int CompareTo(SweepEvent other)
        {
            // This method is different to the static CompareTo because we require this equality checks, whereas
            // the other method does not.
            if (this == other)
            {
                return 0;
            }

            return CompareTo(this, other);
        }

        public static int CompareTo(SweepEvent e1, SweepEvent e2)
        {
            if (e1.Pos.y == e2.Pos.y)
            {
                if (e1.Pos.x == e2.Pos.x)
                {
                    return 0;
                }
                return e1.Pos.x > e2.Pos.x ? -1 : 1;
            }
            return e1.Pos.y > e2.Pos.y ? -1 : 1;
        }

        public bool Equals(SweepEvent other)
        {
            return this == other ? true : false;
        }
    }

    public class StatusItem : IComparable<StatusItem>, IEquatable<StatusItem>
    {
        internal LineObject LineObject { get; private set; }

        internal StatusItem(LineObject lineObject)
        {
            LineObject = lineObject;
        }

        public int CompareTo(StatusItem other)
        {
            if (this == other)
            {
                return 0;
            }
            // Missschien zo iets? dat je ze sorteert op X coordinaat op de hoogte van de sweepline?
            // Ik weet alleen niet of de volgorde nu klopt, of dat ze juist precies andersom moeten (dus ? -1 : 1;)

            // Ik denk dat daar iets mis gaat omdat SweepEvent.pos niet klopt
            // How about this? Vgm gaat er alsnog iets fout omdat we de lines moeten swappen, want dan moet je dit doen:
            // - Eerst de oude deleten (dus die moet je kunnen vinden in de BST onder de oude ordering)
            // - Dan de nieuwe inserten (in de nieuwe ordering)
            // Dus misschien moeten we dan if (x_one == x_two) { hier de ordering af laten hangen van of we net boven of onder de sweepline kijken } 
            float x_one = LineObject.line.X(DownwardSweepLine.Line.Point1.y);
            float x_two = other.LineObject.line.X(DownwardSweepLine.Line.Point1.y);
            if (x_one == x_two)
            {
                Debug.LogWarning("Got here");
                if (DownwardSweepLine.ComparePreEvent)
                {
                    return -LineObject.Highest().x > other.LineObject.Highest().x ? 1 : -1;
                } else
                {
                    return -LineObject.Lowest().x > other.LineObject.Lowest().x ? 1 : -1;
                }
            } else
            {
                return -x_one > x_two ? 1 : -1;
            }
        }

        public bool Equals(StatusItem other)
        {
            return this == other;
        }
        
        public override String ToString()
        {
            return "SI cont " + LineObject.ToString();
        }
    }


    public class DownwardSweepLine : SweepLine<SweepEvent, StatusItem>
    {
        public static bool ComparePreEvent { get; private set; }

        private List<LineObject> lines;

        private List<Intersection> intersections;

        public DownwardSweepLine(List<LineObject> lines)
        {
            this.lines = lines;
            ComparePreEvent = false;
        }

        public List<Intersection> Run()
        {
            this.intersections = new List<Intersection>();
            List<SweepEvent> events = CreateEvents();
            InitializeEvents(events);
            InitializeStatus(new List<StatusItem>());
            VerticalSweep(HandleEvent);
            return this.intersections;
        }

        private List<SweepEvent> CreateEvents()
        {
            List<SweepEvent> events = new List<SweepEvent>();

            foreach (LineObject line in lines)
            {
                StatusItem statusItem = new StatusItem(line);
                SweepEvent enterEvent = new SweepEvent(EventType.INSERT);
                enterEvent.StatusItem = statusItem;
                SweepEvent exitEvent = new SweepEvent(EventType.DELETE);
                exitEvent.StatusItem = statusItem;

                events.Add(enterEvent);
                events.Add(exitEvent);
            }

            return events;
        }

        public new void HandleEvent(IBST<SweepEvent> events, IBST<StatusItem> status, SweepEvent ev)
        {
            if (ev.IsStart)
            {
                Debug.LogWarning("Added " + ev.StatusItem);
                if (!status.Insert(ev.StatusItem)) // TODO: the comparer of StatusItem (which depends on the sweepline like what the fuck man 
                {
                    throw new ArgumentException("Failed to insert into state");
                }

                StatusItem prev;
                if (status.FindNextSmallest(ev.StatusItem, out prev))
                {
                    CheckIntersection(prev, ev.StatusItem, events);
                }

                StatusItem next;
                if (status.FindNextBiggest(ev.StatusItem, out next))
                {
                    CheckIntersection(ev.StatusItem, next, events);
                }
            }
            else if (ev.IsEnd)
            {
                Debug.LogWarning("Removed " + ev.StatusItem);
                StatusItem prev;
                bool hasPrev = status.FindNextSmallest(ev.StatusItem, out prev);

                StatusItem next;
                bool hasNext = status.FindNextBiggest(ev.StatusItem, out next);

                if (!status.Delete(ev.StatusItem))
                {
                    throw new Exception("Could not delete from status : (");
                }

                if (hasPrev && hasNext)
                {
                    CheckIntersection(prev, next, events);
                }
            }
            else if (ev.IsIntersection)
            {
                StatusItem left = ev.StatusItem;
                StatusItem right = ev.IntersectingStatusItem;
                this.intersections.Add(new Intersection(left.LineObject, right.LineObject));

                Debug.LogWarning("Left: " + left + " Right: " + right);

                // Remove
                ComparePreEvent = true;
                status.Delete(left);
                status.Delete(right);

                // Swap
                Debug.LogWarning("Swapping");
                Debug.LogWarning(ComparePreEvent + " " + left.CompareTo(right));
                ComparePreEvent = false;
                Debug.LogWarning(ComparePreEvent + " " + left.CompareTo(right));
                Debug.LogWarning("Done Swapping");

                // Add
                status.Insert(left);
                status.Insert(right);

                StatusItem a, b;
                if (left.CompareTo(right) > 0)
                {
                    a = left;
                    b = right;
                } else
                {
                    b = left;
                    a = right;
                }

                StatusItem prev;
                if(status.FindNextSmallest(a, out prev))
                {
                    CheckIntersection(prev, a, events);
                }

                StatusItem next;
                if(status.FindNextBiggest(b, out next))
                {
                    CheckIntersection(b, next, events);
                }
            }

            Debug.LogWarning("Status: " + status.ToString()); // Requires adding status.ToString, which I should not commit to git
        }

        private void CheckIntersection(StatusItem left, StatusItem right, IBST<SweepEvent> events)
        {
            // TODO make sure we dont add duplicate intersection events :) 
            //Debug.LogWarning("Checking intersection between: " + left + " and " + right);
            Vector2? intersect = left.LineObject.line.Intersect(right.LineObject.line);
            if (intersect != null)
            {
                if (((Vector2)intersect).y < Line.Point1.y)
                {
                    SweepEvent ev = new SweepEvent(EventType.INTERSECT);
                    ev.StatusItem = left;
                    ev.IntersectingStatusItem = right;
                    events.Insert(ev);
                }
            }
        }
    }
}