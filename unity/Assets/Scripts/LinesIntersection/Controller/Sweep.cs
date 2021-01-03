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

        public bool Equals(SweepEvent other) // Need this for events.Contains( ev ) :) 
        {
            if (this == other)
            {
                return true;
            } else if (this.EventType == other.EventType && this.EventType == EventType.INTERSECT && (
                (this.StatusItem == other.StatusItem && this.IntersectingStatusItem == other.IntersectingStatusItem) ||
                (this.StatusItem == other.IntersectingStatusItem && this.IntersectingStatusItem == other.StatusItem)))
            {
                // TODO: Do we also want intersection events to be equal if they are about the same location?
                // Then, I think we may be able to handle cases where >2 lines intersect, but we will only return 1 intersection point between 2 of the >2 lines.
                return true;
            } else
            {
                return false;
            }
         
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
            // Less than 0 	    This instance precedes other in the sort order. 
            // Zero             This instance occurs in the same position in the sort order as other.
            // Greater than 0   This instance follows other in the sort order.
            float x_one = LineObject.line.X(DownwardSweepLine.Line.Point1.y);
            float x_two = other.LineObject.line.X(DownwardSweepLine.Line.Point1.y);
            if (Math.Abs(x_one - x_two) < 1e-4f)
            {
                if (DownwardSweepLine.ComparePreEvent)
                {
                    return LineObject.Highest().x > other.LineObject.Highest().x ? 1 : -1;
                } else
                {
                    return LineObject.Lowest().x > other.LineObject.Lowest().x ? 1 : -1;
                }
            } else
            {
                if (Math.Abs(x_one - x_two) < 1e-3f)
                {
                    Debug.LogWarning("Very close..? Is StatusItem comparison lenient enough? one: " + x_one + " other: " + x_two);
                }
                return x_one > x_two ? 1 : -1;
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

                // Remove
                ComparePreEvent = true;
                if (!status.Delete(left))
                {
                    Debug.LogWarning(left + " not deleted");
                }
                if (!status.Delete(right))
                {
                    Debug.LogWarning(right + " not deleted");
                }

                // Swap
                ComparePreEvent = false;

                // Add
                status.Insert(left);
                status.Insert(right);
                // NOTE: At this point, ComparePreEvent = false implies that right PRECEDES left now

                StatusItem prev;
                if(status.FindNextSmallest(right, out prev))
                {
                    CheckIntersection(prev, right, events);
                }

                StatusItem next;
                if(status.FindNextBiggest(left, out next))
                {
                    CheckIntersection(left, next, events);
                }
            }
        }

        private void CheckIntersection(StatusItem left, StatusItem right, IBST<SweepEvent> events)
        {
            Vector2? intersect = left.LineObject.line.Intersect(right.LineObject.line);
            if (intersect != null)
            {
                if (((Vector2)intersect).y < Line.Point1.y)
                {
                    SweepEvent ev = new SweepEvent(EventType.INTERSECT);
                    ev.StatusItem = left;
                    ev.IntersectingStatusItem = right;
                    if (!events.Contains(ev))
                    {
                        events.Insert(ev);
                    }
                }
            }
        }
    }
}