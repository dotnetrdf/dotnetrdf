using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF;

namespace VDS.Alexandria.Indexing
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
        private bool _stopIndexer = false, _stopped = false, _inactive = true;

        /// <summary>
        /// Variables controlling which indexes are created and used (all default to off by default)
        /// </summary>
        protected readonly bool _subjIndex = false,
                                _predIndex = false,
                                _objIndex = false,
                                _subjPredIndex = false,
                                _subjObjIndex = false,
                                _predObjIndex = false,
                                _tripleIndex = false;

        public BaseIndexManager()
        {
            this._indexer = new Thread(new ThreadStart(this.IndexTriples));
            this._indexer.IsBackground = false;
            this._indexer.Start();
        }

        public BaseIndexManager(IEnumerable<TripleIndexType> indices)
            : this()
        {
            //Enable all the specified indices
            if (indices != null)
            {
                if (indices.Any())
                {
                    foreach (TripleIndexType index in indices)
                    {
                        switch (index)
                        {
                            case TripleIndexType.Object:
                                this._objIndex = true;
                                break;
                            case TripleIndexType.Predicate:
                                this._predIndex = true;
                                break;
                            case TripleIndexType.PredicateObject:
                                this._predObjIndex = true;
                                break;
                            case TripleIndexType.Subject:
                                this._subjIndex = true;
                                break;
                            case TripleIndexType.SubjectObject:
                                this._subjObjIndex = true;
                                break;
                            case TripleIndexType.SubjectPredicate:
                                this._subjPredIndex = true;
                                break;
                            case TripleIndexType.NoVariables:
                                this._tripleIndex = true;
                                break;
                        }
                    }
                }
            }
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
                        this._inactive = false;
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
                            this._inactive = true;
                            this._stopped = true;
                            return;
                        }
                    }
                }
                else
                {
                    //Only Sleep if no waiting Indexing operations
                    bool canSleep = true;
                    lock (this._indexQueue)
                    {
                        canSleep = (this._indexQueue.Count == 0);
                    }
                    if (canSleep)
                    {
                        this._inactive = true;
                        //Sleep to wait for more work to appear
                        Thread.Sleep(100);
                    }
                }
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
        protected virtual IEnumerable<string> GetIndexNames(Triple t)
        {
            if (t == null) return Enumerable.Empty<String>();

            List<String> indices = new List<String>();
            if (this._subjIndex) indices.Add(this.GetIndexNameForSubject(t.Subject));
            if (this._predIndex) indices.Add(this.GetIndexNameForPredicate(t.Predicate));
            if (this._objIndex) indices.Add(this.GetIndexNameForObject(t.Object));
            if (this._subjPredIndex) indices.Add(this.GetIndexNameForSubjectPredicate(t.Subject, t.Predicate));
            if (this._subjObjIndex) indices.Add(this.GetIndexNameForSubjectObject(t.Subject, t.Object));
            if (this._predObjIndex) indices.Add(this.GetIndexNameForPredicateObject(t.Predicate, t.Object));
            if (this._tripleIndex) indices.Add(this.GetIndexNameForTriple(t));

            return indices;
        }

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
            if (this._subjIndex)
            {
                return this.GetTriples(this.GetIndexNameForSubject(subj));
            }
            else
            {
                throw new AlexandriaNoIndexException("Subject");
            }
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            if (this._predIndex)
            {
                return this.GetTriples(this.GetIndexNameForPredicate(pred));
            }
            else
            {
                throw new AlexandriaNoIndexException("Predicate");
            }
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            if (this._objIndex)
            {
                return this.GetTriples(this.GetIndexNameForObject(obj));
            }
            else
            {
                throw new AlexandriaNoIndexException("Object");
            }
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            if (this._subjPredIndex)
            {
                return this.GetTriples(this.GetIndexNameForSubjectPredicate(subj, pred));
            }
            else if (this._subjIndex)
            {
                return (from t in this.GetTriples(this.GetIndexNameForSubject(subj))
                        where t.Predicate.Equals(pred)
                        select t);
            }
            else if (this._predIndex)
            {
                return (from t in this.GetTriples(this.GetIndexNameForPredicate(pred))
                        where t.Subject.Equals(subj)
                        select t);
            }
            else
            {
                throw new AlexandriaNoIndexException("Subject Predicate");
            }
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            if (this._predObjIndex)
            {
                return this.GetTriples(this.GetIndexNameForPredicateObject(pred, obj));
            }
            else if (this._objIndex)
            {
                return (from t in this.GetTriples(this.GetIndexNameForObject(obj))
                        where t.Predicate.Equals(pred)
                        select t);
            }
            else if (this._predIndex)
            {
                return (from t in this.GetTriples(this.GetIndexNameForPredicate(pred))
                        where t.Object.Equals(obj)
                        select t);
            }
            else
            {
                throw new AlexandriaNoIndexException("Predicate Object");
            }
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            if (this._subjObjIndex)
            {
                return this.GetTriples(this.GetIndexNameForSubjectObject(subj, obj));
            }
            else if (this._subjIndex)
            {
                return (from t in this.GetTriples(this.GetIndexNameForSubject(subj))
                        where t.Object.Equals(obj)
                        select t);
            }
            else if (this._objIndex)
            {
                return (from t in this.GetTriples(this.GetIndexNameForObject(obj))
                        where t.Subject.Equals(subj)
                        select t);
            }
            else
            {
                throw new AlexandriaNoIndexException("Subject Object");
            }
        }

        public IEnumerable<Triple> GetTriples(Triple t)
        {
            if (this._tripleIndex)
            {
                return this.GetTriples(this.GetIndexNameForTriple(t));
            }
            else if (this._subjPredIndex)
            {
                return (from x in this.GetTriples(this.GetIndexNameForSubjectPredicate(t.Subject, t.Predicate))
                        where x.Object.Equals(t.Object)
                        select x);
            }
            else if (this._predObjIndex)
            {
                return (from x in this.GetTriples(this.GetIndexNameForPredicateObject(t.Predicate, t.Object))
                        where x.Subject.Equals(t.Subject)
                        select x);
            }
            else if (this._subjObjIndex)
            {
                return (from x in this.GetTriples(this.GetIndexNameForSubjectObject(t.Subject, t.Object))
                        where x.Predicate.Equals(t.Predicate)
                        select x);
            }
            else if (this._subjIndex)
            {
                return (from x in this.GetTriples(this.GetIndexNameForSubject(t.Subject))
                        where x.Predicate.Equals(t.Predicate) && x.Object.Equals(t.Object)
                        select x);
            }
            else if (this._predIndex)
            {
                return (from x in this.GetTriples(this.GetIndexNameForPredicate(t.Predicate))
                        where x.Subject.Equals(t.Subject) && x.Object.Equals(t.Object)
                        select x);
            }
            else if (this._objIndex)
            {
                return (from x in this.GetTriples(this.GetIndexNameForObject(t.Object))
                        where x.Subject.Equals(t.Subject) && x.Predicate.Equals(t.Predicate)
                        select x);
            }
            else
            {
                throw new AlexandriaNoIndexException("Triple");
            }
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

        public virtual void Flush()
        {
            if (this._inactive)
            {
                //If state is inactive check whether there are operations in the queue, if there are then need to wait to see if the indexer becomes active
                bool needToWait = false;
                lock (this._indexQueue)
                {
                    needToWait = (this._indexQueue.Count > 0);
                }
                if (!needToWait) return;
                Thread.Sleep(150);
            }

            //Wait for Indexing to be inactive
            while (!this._inactive)
            {
                Thread.Sleep(50);
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
