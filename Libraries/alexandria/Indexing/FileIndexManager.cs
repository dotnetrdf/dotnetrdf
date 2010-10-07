using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Documents.Adaptors;

namespace VDS.Alexandria.Indexing
{
    public class FileIndexManager : BaseIndexManager
    {
        private FileDocumentManager _manager;
        private SHA256Managed _hash;
        private NQuadsAdaptor _adaptor = new NQuadsAdaptor();

        public FileIndexManager(FileDocumentManager manager)
            : this(manager, AlexandriaFileManager.OptimalIndices) { }

        public FileIndexManager(FileDocumentManager manager, IEnumerable<TripleIndexType> indices)
            : base(indices)
        {
            this._manager = manager;
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

                IDocument<StreamReader,TextWriter> doc = this._manager.GetDocument(indexName);
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

                IDocument<StreamReader,TextWriter> doc = this._manager.GetDocument(indexName);
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
