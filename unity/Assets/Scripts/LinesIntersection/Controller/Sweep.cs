using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Algorithms;
using Util.DataStructures.BST;

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
                    return this.LineObject.line.Point1.y > this.LineObject.line.Point2.y ? this.LineObject.line.Point1 : this.LineObject.line.Point2;
                }
                else if (this.IsEnd)
                {
                    return this.LineObject.line.Point1.y < this.LineObject.line.Point2.y ? this.LineObject.line.Point1 : this.LineObject.line.Point2;
                }
                else //if (this.IsIntersection)
                {
                    return (Vector2)LineObject.line.Intersect(IntersectingLineObject.line);
                }
            }
        }

        public EventType EventType;

        public StatusItem StatusItem { get; set; }

        public SweepEvent OtherEvent { get; set; }

        public LineObject LineObject { get; set; }

        public LineObject IntersectingLineObject { get; set; }

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
            return e1.Pos.y > e2.Pos.y ? 1 : -1;
        }

        public bool Equals(SweepEvent other)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusItem : IComparable<StatusItem>, IEquatable<StatusItem>
    {
        internal SweepEvent SweepEvent { get; private set; }

        internal LineObject LineObject { get; private set; }

        internal StatusItem(SweepEvent sweepEvent, LineObject lineObject)
        {
            SweepEvent = sweepEvent;
            LineObject = lineObject;
        }

        public int CompareTo(StatusItem other)
        {
            // TODO somehow compare intersections
            throw new NotImplementedException();
        }

        public bool Equals(StatusItem other)
        {
            throw new NotImplementedException();
        }
    }


    public class DownwardSweepLine : SweepLine<SweepEvent, StatusItem>
    {
        private List<LineObject> lines;

        private List<Intersection> intersections;

        public DownwardSweepLine(List<LineObject> lines)
        {
            this.lines = lines;
        }

        public void Run()
        {
            List<SweepEvent> events = CreateEvents();
            InitializeEvents(events);
            InitializeStatus(new List<StatusItem>());
        }

        private List<SweepEvent> CreateEvents()
        {
            List<SweepEvent> events = new List<SweepEvent>();

            foreach (LineObject line in lines)
            {
                SweepEvent enterEvent = new SweepEvent(EventType.INSERT);
                enterEvent.LineObject = line;
                SweepEvent exitEvent = new SweepEvent(EventType.DELETE);
                exitEvent.LineObject = line;
                exitEvent.OtherEvent = enterEvent;
            }

            return events;
        }

        public new void HandleEvent(IBST<SweepEvent> events, IBST<StatusItem> status, SweepEvent ev)
        {
            if (ev.IsStart)
            {
                ev.StatusItem = new StatusItem(ev, ev.LineObject);
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

                status.Delete(ev.StatusItem);

                if (hasPrev && hasNext)
                {
                    CheckIntersection(prev, next, events);
                }
            }
            else if (ev.IsIntersection)
            {
                this.intersections.Add(new Intersection(ev.LineObject, ev.IntersectingLineObject));

                // TODO: recompute status
            }
        }

        private void CheckIntersection(StatusItem left, StatusItem right, IBST<SweepEvent> events)
        {
            Vector2? intersect = left.LineObject.line.Intersect(right.LineObject.line);
            if (intersect != null)
            {
                SweepEvent ev = new SweepEvent(EventType.INTERSECT);
                ev.LineObject = left.LineObject;
                ev.IntersectingLineObject = right.LineObject;
                events.Insert(ev);
            }
        }
    }
}