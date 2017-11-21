/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

namespace VDS.RDF.Parsing.Events
{
    /// <summary>
    /// Interface for implementing Event Queues which provide Bufferable wrappers to Event Generators
    /// </summary>
    public interface IEventQueue<T> where T : IEvent
    {
        /// <summary>
        /// Removes the first Event from the Queue
        /// </summary>
        /// <returns></returns>
        T Dequeue();

        /// <summary>
        /// Adds an Event to the end of the Queue
        /// </summary>
        /// <param name="e">Event to add</param>
        void Enqueue(T e);

        /// <summary>
        /// Gets the first Event from the Queue without removing it
        /// </summary>
        /// <returns></returns>
        T Peek();

        /// <summary>
        /// Gets the Event Generator that this Queue uses
        /// </summary>
        IEventGenerator<T> EventGenerator
        {
            get;
        }

        /// <summary>
        /// Clears the Event Queue
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the number of Events in the Queue
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Gets/Sets whether Generator Tracing should be used
        /// </summary>
        bool Tracing
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Event Type of the last Event dequeued
        /// </summary>
        int LastEventType
        {
            get;
        }
    }

    /// <summary>
    /// Abstract base implementation of an Event Queue
    /// </summary>
    public abstract class BaseEventQueue<T> : IEventQueue<T> where T : IEvent
    {
        /// <summary>
        /// Generator used to fill the Event Queue
        /// </summary>
        protected IEventGenerator<T> _eventgen;
        /// <summary>
        /// Variable indicating whether Generator Tracing is enabled
        /// </summary>
        protected bool _tracing;
        /// <summary>
        /// Type of Last Event dequeued
        /// </summary>
        protected int _lasteventtype;

        /// <summary>
        /// Dequeues an Event from the Queue
        /// </summary>
        /// <returns></returns>
        public abstract T Dequeue();

        /// <summary>
        /// Adds an Event to the Queue
        /// </summary>
        /// <param name="e">Event</param>
        public abstract void Enqueue(T e);

        /// <summary>
        /// Gets the next Event from the Queue without removing it from the queue
        /// </summary>
        /// <returns></returns>
        public abstract T Peek();

        /// <summary>
        /// Gets the Event Generator used by the Queue
        /// </summary>
        public IEventGenerator<T> EventGenerator
        {
            get
            {
                return _eventgen;
            }
        }

        /// <summary>
        /// Clears the Event Queue
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Gets the number of Events in the Queue
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Gets/Sets Tracing for the Event Queue
        /// </summary>
        public bool Tracing
        {
            get
            {
                return _tracing;
            }
            set
            {
                _tracing = value;
            }
        }

        /// <summary>
        /// Gets the Event Type of the last Event dequeued
        /// </summary>
        public int LastEventType
        {
            get
            {
                return _lasteventtype;
            }
        }
    }
}
