/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                return this._eventgen;
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
                return this._tracing;
            }
            set
            {
                this._tracing = value;
            }
        }

        /// <summary>
        /// Gets the Event Type of the last Event dequeued
        /// </summary>
        public int LastEventType
        {
            get
            {
                return this._lasteventtype;
            }
        }
    }
}
