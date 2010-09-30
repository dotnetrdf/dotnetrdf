using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using VDS.RDF;
using Alexandria.Documents;
using Alexandria.Documents.Adaptors;

namespace Alexandria.Indexing
{
    public class FileIndexManager : BaseIndexManager
    {
        private FileDocumentManager _manager;
        private SHA256Managed _hash;
        private NQuadsAdaptor _adaptor = new NQuadsAdaptor();

        private bool _subjIndex = true;
        private bool _predIndex = true;
        private bool _objIndex = true;
        private bool _subjPredIndex = true;
        private bool _subjObjIndex = true;
        private bool _predObjIndex = true;
        private bool _tripleIndex = true;

        public FileIndexManager(FileDocumentManager manager)
        {
            this._manager = manager;
        }

        public FileIndexManager(FileDocumentManager manager, IEnumerable<TripleIndexType> indices)
            : this(manager)
        {
            //Turns off indexing and then turns them back on if specified
            this.SetNoIndexing();
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

        private void SetNoIndexing()
        {
            this._subjIndex = false;
            this._predIndex = false;
            this._objIndex = false;
            this._subjPredIndex = false;
            this._subjObjIndex = false;
            this._predObjIndex = false;
            this._tripleIndex = false;
        }

        protected override IEnumerable<Triple> GetTriples(string indexName)
        {
            if (this._manager.HasDocument(indexName))
            {
                return new FileIndexReader(this._manager.GetDocument(indexName));
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        #region Index Names

        protected override IEnumerable<string> GetIndexNames(Triple t)
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

        //TODO: Alter these functions so they provide alternative Index Names if the relevant index is not in-use

        protected override string GetIndexNameForSubject(INode subj)
        {
            return @"index\s\" + this.GetHash(subj.GetHashCode().ToString());
        }

        protected override string GetIndexNameForPredicate(INode pred)
        {
            return @"index\p\" + this.GetHash(pred.GetHashCode().ToString());
        }

        protected override string GetIndexNameForObject(INode obj)
        {
            return @"index\o\" + this.GetHash(obj.GetHashCode().ToString());
        }

        protected override string GetIndexNameForSubjectPredicate(INode subj, INode pred)
        {
            return @"index\sp\" + this.GetHash(Tools.CombineHashCodes(subj, pred).ToString());
        }

        protected override string GetIndexNameForSubjectObject(INode subj, INode obj)
        {
            return @"index\so\" + this.GetHash(Tools.CombineHashCodes(subj, obj).ToString());
        }

        protected override string GetIndexNameForPredicateObject(INode pred, INode obj)
        {
            return @"index\po\" + this.GetHash(Tools.CombineHashCodes(pred, obj).ToString());
        }

        protected override string GetIndexNameForTriple(Triple t)
        {
            return @"index\spo\" + this.GetHash(t.GetHashCode().ToString());
        }

        #endregion

        private String GetHash(String value)
        {
            if (this._hash == null) this._hash = new SHA256Managed();

            Byte[] input = Encoding.UTF8.GetBytes(value);
            Byte[] output = this._hash.ComputeHash(input);

            StringBuilder hash = new StringBuilder();
            foreach (Byte b in output)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }

        protected override void AddToIndexInternal(IEnumerable<Triple> ts, string indexName)
        {
            try
            {
                if (!this._manager.HasDocument(indexName))
                {
                    if (!this._manager.CreateDocument(indexName))
                    {
                        throw new AlexandriaException("Unable to update an Index since the Document Manager was unable to create the Document for this index");
                    }
                }

                IDocument doc = this._manager.GetDocument(indexName);
                this._adaptor.AppendTriples(ts, doc);
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to Update the Index " + indexName, ex);
            }
            finally
            {
                this._manager.ReleaseDocument(indexName);
            }
        }

        protected override void RemoveFromIndexInternal(IEnumerable<Triple> ts, string indexName)
        {
            try
            {
                if (!this._manager.HasDocument(indexName))
                {
                    if (!this._manager.CreateDocument(indexName))
                    {
                        throw new AlexandriaException("Unable to update an Index since the Document Manager was unable to create the Document for this index");
                    }
                }

                IDocument doc = this._manager.GetDocument(indexName);
                this._adaptor.DeleteTriples(ts, doc);
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to Update the Index " + indexName, ex);
            }
            finally
            {
                this._manager.ReleaseDocument(indexName);
            }
        }
    }
}
