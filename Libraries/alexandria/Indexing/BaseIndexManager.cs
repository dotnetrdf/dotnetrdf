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
        private Queue<IndexingAction> _indexQueue = new Queue<IndexingAction>();
        private Thread _indexer;
        private bool _stopIndexer = false, _stopped = false;

        public BaseIndexManager()
        {
            this._indexer = new Thread(new ThreadStart(this.IndexTriples));
            this._indexer.IsBackground = false;
            this._indexer.Start();
        }

        #region Internal Processing

        private void IndexTriples()
        {
            while (true)
            {
                 //We want to empty the add queue and batch it's operations by index and type
                lock (this._indexQueue)
                {
                    if (this._indexQueue.Count > 0)
                    {
                        Dictionary<String, List<Triple>> batches = new Dictionary<string, List<Triple>>();
                        IndexingAction action = this._indexQueue.Dequeue();
                        bool isDelete = action.IsDelete;
                        while (true)
                        {
                            this.BatchOperations(action.Triple, batches);

                            if (this._indexQueue.Count > 0)
                            {
                                action = this._indexQueue.Dequeue();

                                //When the action type changes need to process the batches so far
                                if (action.IsDelete != isDelete)
                                {
                                    foreach (KeyValuePair<String, List<Triple>> batch in batches)
                                    {
                                        this.ProcessBatch(batch.Key, batch.Value, isDelete);
                                    }
                                    //this.ProcessBatches(batches, isDelete);
                                    isDelete = action.IsDelete;

                                    //Remember to clear the batches afterwards!
                                    batches.Clear();
                                }
                            }
                            else
                            {
                                //If we've emptied the queue and the action did not change then we need to process the batches
                                foreach (KeyValuePair<String, List<Triple>> batch in batches)
                                {
                                    this.ProcessBatch(batch.Key, batch.Value, isDelete);
                                }
                                //this.ProcessBatches(batches, isDelete);
                                batches.Clear();

                                //Exit the while loop
                                break;
                            }
                        }
                    }
                }

                //Then either stop or sleep as appropriate
                if (this._stopIndexer)
                {
                    //Only stop if queues are empty
                    bool canStop = true;
                    //Checking if the add queue is empty
                    lock (this._indexQueue)
                    {
                        if (this._indexQueue.Count > 0) canStop = false;
                    }
                    if (canStop)
                    {
                        //Need to also check if remove queue is empty
                        lock (this._indexQueue)
                        {
                            if (this._indexQueue.Count > 0) canStop = false;
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
            List<String> indices = this.GetIndexNames(t).ToList();
            if (indices.Count > 0)
            {
                foreach (String index in indices)
                {
                    if (!batches.ContainsKey(index)) batches.Add(index, new List<Triple>());
                    batches[index].Add(t);
                }
            }
        }

        //private void ProcessBatches(Dictionary<String, List<Triple>> batches, bool isDelete)
        //{
        //    List<String> keys = batches.Keys.ToList();
        //    WaitHandle[] handles = new WaitHandle[8];
        //    IAsyncResult[] results = new IAsyncResult[8];
        //    ProcessBatchDelegate d = new ProcessBatchDelegate(this.ProcessBatch);

        //    int i = 0;
        //    for (int pos = 0; pos < keys.Count; pos++)
        //    {
        //        //Check whether we need to wait for something to complete
        //        if (i >= results.Length)
        //        {
        //            i = 0;
        //            while (i < results.Length && results[i] != null)
        //            {
        //                if (results[i].IsCompleted)
        //                {
        //                    //End the Invoke and reuse this
        //                    d.EndInvoke(results[i]);
        //                    results[i] = null;
        //                    break;
        //                }
        //                else
        //                {
        //                    i++;
        //                }
        //            }
        //            if (i >= results.Length)
        //            {
        //                //Wait for any of the operations to complete and then reset i and continue
        //                WaitHandle.WaitAny(handles);
        //                i = 0;
        //                continue;
        //            }
        //        }

        //        //Start a new async call
        //        results[i] = d.BeginInvoke(keys[pos], batches[keys[pos]], isDelete, null, null);
        //        handles[i] = results[i].AsyncWaitHandle;

        //        i++;
        //    }

        //    //When we exit ensure all the handles are cleaned up
        //    if (!results.All(r => r == null || r.IsCompleted))
        //    {
        //        WaitHandle.WaitAll(handles);
        //    }
        //    foreach (IAsyncResult result in results)
        //    {
        //        if (result != null) d.EndInvoke(result);
        //    }
        //}

        //private delegate void ProcessBatchDelegate(String name, List<Triple> ts, bool isDelete);

        private void ProcessBatch(String name, List<Triple> ts, bool isDelete)
        {
            if (isDelete)
            {
                this.RemoveFromIndexInternal(ts, name);
            }
            else
            {
                this.AddToIndexInternal(ts, name);
            }
        }

        /// <summary>
        /// Gets all the Indexes to which a Triple should be added
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected abstract IEnumerable<String> GetIndexNames(Triple t);

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

        #endregion

        #region Triple Lookups

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return this.GetTriples(this.GetIndexNameForSubject(subj));
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return this.GetTriples(this.GetIndexNameForPredicate(pred));
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return this.GetTriples(this.GetIndexNameForObject(obj));
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return this.GetTriples(this.GetIndexNameForSubjectPredicate(subj, pred));
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return this.GetTriples(this.GetIndexNameForPredicateObject(pred, obj));
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return this.GetTriples(this.GetIndexNameForSubjectObject(subj, obj));
        }

        public IEnumerable<Triple> GetTriples(Triple t)
        {
            return this.GetTriples(this.GetIndexNameForTriple(t));
        }

        protected abstract IEnumerable<Triple> GetTriples(String indexName);

        protected abstract String GetIndexNameForSubject(INode subj);

        protected abstract String GetIndexNameForPredicate(INode pred);

        protected abstract String GetIndexNameForObject(INode obj);

        protected abstract String GetIndexNameForSubjectPredicate(INode subj, INode pred);

        protected abstract String GetIndexNameForSubjectObject(INode subj, INode obj);

        protected abstract String GetIndexNameForPredicateObject(INode pred, INode obj);

        protected abstract String GetIndexNameForTriple(Triple t);

        #endregion

        public void AddToIndex(Triple t)
        {
            lock (this._indexQueue)
            {
                this._indexQueue.Enqueue(new IndexingAction(t));
            }
        }

        public void AddToIndex(IEnumerable<Triple> ts)
        {
            lock (this._indexQueue)
            {
                foreach (Triple t in ts)
                {
                    this._indexQueue.Enqueue(new IndexingAction(t));
                }
            }
        }

        public void RemoveFromIndex(Triple t)
        {
            lock (this._indexQueue)
            {
                this._indexQueue.Enqueue(new IndexingAction(t, true));
            }
        }

        public void RemoveFromIndex(IEnumerable<Triple> ts)
        {
            lock (this._indexQueue)
            {
                foreach (Triple t in ts)
                {
                    this._indexQueue.Enqueue(new IndexingAction(t,true));
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

    class IndexingAction
    {
        private bool _delete = false;
        private Triple _t;

        public IndexingAction(Triple t)
        {
            this._t = t;
        }

        public IndexingAction(Triple t, bool delete)
            : this(t)
        {
            this._delete = delete;
        }

        public Triple Triple
        {
            get
            {
                return this._t;
            }
        }

        public bool IsDelete
        {
            get
            {
                return this._delete;
            }
        }
    }
}
