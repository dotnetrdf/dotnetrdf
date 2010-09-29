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

        public FileIndexManager(FileDocumentManager manager)
        {
            this._manager = manager;
        }

        public override IEnumerable<Triple> GetTriples(string indexName)
        {
            throw new NotImplementedException();
        }

        protected override string[] GetIndexNames(Triple t)
        {
            if (t == null) return new String[0];
            if (this._hash == null) this._hash = new SHA256Managed();

            String[] indices = new String[6];
            indices[0] = @"index\s\" + this.GetHash(t.Subject.GetHashCode().ToString());
            indices[1] = @"index\p\" + this.GetHash(t.Predicate.GetHashCode().ToString());
            indices[2] = @"index\o\" + this.GetHash(t.Object.GetHashCode().ToString());
            indices[3] = @"index\sp\" + this.GetHash(Tools.CombineHashCodes(t.Subject, t.Predicate).ToString());
            indices[4] = @"index\so\" + this.GetHash(Tools.CombineHashCodes(t.Subject, t.Object).ToString());
            indices[5] = @"index\po\" + this.GetHash(Tools.CombineHashCodes(t.Predicate, t.Object).ToString());

            return indices;
        }

        private String GetHash(String value)
        {
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
