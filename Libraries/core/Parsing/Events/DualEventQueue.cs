using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Events
{
    public class DualEventQueue<T> : BaseEventQueue<T> where T : IEvent
    {
        private IEventQueue<T> _queue;
        private Queue<T> _temp = new Queue<T>();

        public DualEventQueue(IEventQueue<T> queue)
        {
            this._queue = queue;
        }

        public override T Dequeue()
        {
            if (this._temp.Count > 0)
            {
                return this._temp.Dequeue();
            }
            else
            {
                return this._queue.Dequeue();
            }
        }

        public override void Enqueue(T e)
        {
            this._queue.Enqueue(e);
        }

        public void Requeue(T e)
        {
            this._temp.Enqueue(e);
        }

        public override T Peek()
        {
            if (this._temp.Count > 0)
            {
                return this._temp.Peek();
            }
            else
            {
                return this._queue.Peek();
            }
        }

        public override void Clear()
        {
            this._temp.Clear();
            this._queue.Clear();
        }

        public override int Count
        {
            get 
            {
                return this._temp.Count + this._queue.Count; 
            }
        }
    }
}
