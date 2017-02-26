/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using VDS.RDF.Parsing.Events.RdfXml;

namespace VDS.RDF.Parsing.Events
{
    /// <summary>
    /// Represents a Queue of events for use by event based parsers
    /// </summary>
    public class EventQueue<T> : BaseEventQueue<T> where T : IEvent
    {
        /// <summary>
        /// Queue of Events
        /// </summary>
        protected Queue<T> _events = new Queue<T>();

        /// <summary>
        /// Creates a new Event Queue
        /// </summary>
        public EventQueue()
        {

        }

        /// <summary>
        /// Creates a new Event Queue with the given Event Generator
        /// </summary>
        /// <param name="generator">Event Generator</param>
        public EventQueue(IEventGenerator<T> generator)
        {
            this._eventgen = generator;
        }

        /// <summary>
        /// Dequeues and returns the next event in the Queue
        /// </summary>
        /// <returns></returns>
        public override T Dequeue()
        {
            this._lasteventtype = this._events.Peek().EventType;
            //REQ: Add proper tracing support to this
            //if (this._tracing) this.PrintTrace(this._events.Peek());
            return this._events.Dequeue();
        }

        /// <summary>
        /// Adds an event to the end of the Queue
        /// </summary>
        /// <param name="e">Event</param>
        public override void Enqueue(T e)
        {
            this._events.Enqueue(e);
        }

        /// <summary>
        /// Peeks and returns the next event in the Queue
        /// </summary>
        /// <returns></returns>
        public override T Peek()
        {
            return this._events.Peek();
        }

        /// <summary>
        /// Clears the Queue
        /// </summary>
        public override void Clear()
        {
            this._events.Clear();
        }

        /// <summary>
        /// Gets the number of events currently in the Queue
        /// </summary>
        public override int Count
        {
            get 
            {
                return this._events.Count; 
            }
        }
    }

    /// <summary>
    /// Represents a Queue of events which are streamed from an instance of a <see cref="IJitEventGenerator{T}">IJitEventGenerator</see> for use by an event based parser
    /// </summary>
    public class StreamingEventQueue<T> : EventQueue<T> where T : IEvent
    {
        private IJitEventGenerator<T> _jitgen;
        private int _buffer = 10;

        /// <summary>
        /// Creates a new Streaming Event Queue
        /// </summary>
        /// <param name="generator">Event Generator</param>
        public StreamingEventQueue(IJitEventGenerator<T> generator)
            : base(generator)
        {
            this._jitgen = generator;
        }

        /// <summary>
        /// Gets the Count of events in the queue
        /// </summary>
        public override int Count
        {
            get
            {
                if (this._jitgen.Finished) return base.Count;

                while (!this._jitgen.Finished && this._events.Count < this._buffer)
                {
                    this.Enqueue(this._jitgen.GetNextEvent());
                }
                return base.Count;
            }
        }

        /// <summary>
        /// Adds an event to the Queue
        /// </summary>
        /// <param name="e">Event</param>
        public override void Enqueue(T e)
        {
            if (!ReferenceEquals(e, null))
            {
                if (e is ClearQueueEvent)
                {
                    this.Clear();
                }
                else
                {
                    base.Enqueue(e);
                }
            }
        }

        /// <summary>
        /// Gets the next event from the Queue and removes it from the Queue
        /// </summary>
        /// <returns></returns>
        public override T Dequeue()
        {
            while (!this._jitgen.Finished && this._events.Count < this._buffer)
            {
                this.Enqueue(this._jitgen.GetNextEvent());
            }
            return base.Dequeue();
        }

        /// <summary>
        /// Gets the next event from the Queue while leaving the Queue unchanged
        /// </summary>
        /// <returns></returns>
        public override T Peek()
        {
            while (!this._jitgen.Finished && this._events.Count < this._buffer)
            {
                this.Enqueue(this._jitgen.GetNextEvent());
            }
            return base.Peek();
        }
    }


    public class SublistEventQueue<T> : BaseEventQueue<T> where T : IEvent
    {
        private IEventQueue<T> _events;
        private int _threshold;

        public SublistEventQueue(IEventQueue<T> events, int threshold)
        {
            this._events = events;
            this._threshold = threshold;
        }

        public override T Dequeue()
        {
            if (this._events.Count > this._threshold)
            {
                return this._events.Dequeue();
            }
            else
            {
                throw new InvalidOperationException("Event queue is empty");
            }
        }

        public override void Enqueue(T e)
        {
            if (!ReferenceEquals(e, null))
            {
                if (e is ClearQueueEvent)
                {
                    this.Clear();
                }
                else
                {
                    this._events.Enqueue(e);
                }
            }
        }

        public override T Peek()
        {
            if (this._events.Count > this._threshold)
            {
                return this._events.Peek();
            }
            else
            {
                throw new InvalidOperationException("Event queue is empty");
            }
        }

        public override void Clear()
        {
            this._events.Clear();
        }

        public override int Count
        {
            get { return this._events.Count - this._threshold; }
        }
    }
}
