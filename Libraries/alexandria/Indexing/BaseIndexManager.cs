using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF;

namespace Alexandria.Indexing
{
    /// <summary>
    /// Abstract Base Class for Index Managers
    /// </summary>
    /// <remarks>
    /// <para>
    /// This Manager queues Triples to be indexed and processes them in the Background
    /// </para>
    /// </remarks>
    public abstract class BaseIndexManager : IIndexManager
    {
        private Queue<Triple> _addQueue = new Queue<Triple>();
        private Queue<Triple> _removeQueue = new Queue<Triple>();
        private Thread _indexer;
        private bool _stopIndexer = false, _stopped = false;

        public BaseIndexManager()
        {
            this._indexer = new Thread(new ThreadStart(this.IndexTriples));
            this._indexer.Start();
        }

        private void IndexTriples()
        {
            while (true)
            {
                //First deal with additions to indices
                //We want to empty the add queue and batch it's addition operations by index
                lock (this._addQueue)
                {
                    if (this._addQueue.Count > 0)
                    {
                        Dictionary<String, List<Triple>> batches = new Dictionary<string, List<Triple>>();
                        while (this._addQueue.Count > 0)
                        {
                            this.BatchOperations(this._addQueue.Dequeue(), batches);
                        }

                        foreach (KeyValuePair<String, List<Triple>> batch in batches)
                        {
                            this.AddToIndexInternal(batch.Value, batch.Key);
                        }
                    }
                }

                //Then deal with removals from indices
                lock (this._removeQueue)
                {
                    if (this._removeQueue.Count > 0)
                    {
                        Dictionary<String, List<Triple>> batches = new Dictionary<string, List<Triple>>();
                        while (this._removeQueue.Count > 0)
                        {
                            this.BatchOperations(this._removeQueue.Dequeue(), batches);
                        }

                        foreach (KeyValuePair<String, List<Triple>> batch in batches)
                        {
                            this.RemoveFromIndexInternal(batch.Value, batch.Key);
                        }
                    }
                }

                //Then either stop or sleep as appropriate
                if (this._stopIndexer)
                {
                    //Only stop if queues are empty
                    bool canStop = true;
                    //Checking if the add queue is empty
                    lock (this._addQueue)
                    {
                        if (this._addQueue.Count > 0) canStop = false;
                    }
                    if (canStop)
                    {
                        //Need to also check if remove queue is empty
                        lock (this._removeQueue)
                        {
                            if (this._removeQueue.Count > 0) canStop = false;
                        }

                        if (canStop)
                        {
                            this._stopped = true;
                            return;
                        }
                    }
                }

                //Sleep to wait for more work to appear
                Thread.Sleep(100);
            }
        }

        private void BatchOperations(Triple t, Dictionary<String, List<Triple>> batches)
        {
            String[] indices = this.GetIndexNames(t);
            if (indices.Length > 0)
            {
                foreach (String index in indices)
                {
                    if (!batches.ContainsKey(index)) batches.Add(index, new List<Triple>());
                    batches[index].Add(t);
                }
            }
        }

        /// <summary>
        /// Gets all the Triples from the given Index
        /// </summary>
        /// <param name="indexName">Index Name</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriples(string indexName);

        /// <summary>
        /// Gets all the Indexes to which a Triple should be added
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected abstract String[] GetIndexNames(Triple t);

        /// <summary>
        /// Adds the given Triples to the given Index
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <param name="indexName">Index</param>
        protected abstract void AddToIndexInternal(IEnumerable<Triple> ts, String indexName);

        /// <summary>
        /// Removes the given Triples from the given Index
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <param name="indexName">Index</param>
        protected abstract void RemoveFromIndexInternal(IEnumerable<Triple> ts, String indexName);

        public void AddToIndex(Triple t)
        {
            lock (this._addQueue)
            {
                this._addQueue.Enqueue(t);
            }
        }

        public void AddToIndex(IEnumerable<Triple> ts)
        {
            lock (this._addQueue)
            {
                foreach (Triple t in ts)
                {
                    this._addQueue.Enqueue(t);
                }
            }
        }

        public void RemoveFromIndex(Triple t)
        {
            lock (this._removeQueue)
            {
                this._removeQueue.Enqueue(t);
            }
        }

        public void RemoveFromIndex(IEnumerable<Triple> ts)
        {
            lock (this._removeQueue)
            {
                foreach (Triple t in ts)
                {
                    this._removeQueue.Enqueue(t);
                }
            }
        }

        public virtual void Dispose()
        {
            //Wait for index operations to complete
            this._stopIndexer = true;
            while (!this._stopped)
            {
                Thread.Sleep(50);
            }
        }
    }
}
