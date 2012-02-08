using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria.Datasets
{
    class FileEnumerator : IEnumerator<Triple>
    {
        private AlexandriaFileManager _manager;
        private Triple _current = null;
        private IEnumerator<String> _graphCursor = null;
        private IDocument<StreamReader, TextWriter> _currDoc;
        private StreamingNTriplesParser _parser;

        public FileEnumerator(AlexandriaFileManager manager)
        {
            this._manager = manager;
        }

        public Triple Current
        {
            get 
            { 
                if (this._graphCursor == null) throw new InvalidOperationException("The enumerator is positioned before the start of the collection");
                if (this._current == null)
                {
                    throw new InvalidOperationException("The enumerator is positioned after the end of the collection");
                }
                else
                {
                    Triple t = this._current;
                    this._current = null;
                    return t;
                }
            }
        }

        public void Dispose()
        {
            if (this._currDoc != null)
            {
                this._currDoc.EndRead();
                this._manager.DocumentManager.ReleaseDocument(this._currDoc.Name);
                this._currDoc = null;
            }
            if (this._graphCursor != null)
            {
                this._graphCursor.Dispose();
                this._graphCursor = null;
            }
            this._current = null;
        }

        object System.Collections.IEnumerator.Current
        {
            get 
            {
                return this.Current;
            }
        }

        public bool MoveNext()
        {
            if (this._graphCursor == null)
            {
                this._graphCursor = this._manager.DocumentManager.GraphRegistry.GraphUris.ToList().GetEnumerator();

                if (this._graphCursor.MoveNext())
                {
                    this._currDoc = this._manager.DocumentManager.GetDocument(this._manager.DocumentManager.GraphRegistry.GetDocumentName(this._graphCursor.Current));
                    
                    //Ensure we feed in consistent Graph references using the Node Factory
                    String uri = this._manager.DocumentManager.GraphRegistry.GetGraphUri(this._currDoc.Name);
                    if (uri != null && !uri.Equals(String.Empty))
                    {
                        this._parser = new StreamingNTriplesParser(this._manager.DocumentManager.GraphFactory[new Uri(uri)], this._currDoc.BeginRead());
                    }
                    else
                    {
                        this._parser = new StreamingNTriplesParser(this._manager.DocumentManager.GraphFactory[null], this._currDoc.BeginRead());
                    }
                }
            }

            while (this._parser.EOF)
            {
                //Stop reading and release the old document
                this._currDoc.EndRead();
                this._manager.DocumentManager.ReleaseDocument(this._currDoc.Name);
                this._currDoc = null;

                //Try and get the next document
                if (this._graphCursor.MoveNext())
                {
                    this._currDoc = this._manager.DocumentManager.GetDocument(this._manager.DocumentManager.GraphRegistry.GetDocumentName(this._graphCursor.Current));

                    //Ensure we feed in consistent Graph references using the Node Factory
                    String uri = this._manager.DocumentManager.GraphRegistry.GetGraphUri(this._currDoc.Name);
                    if (uri != null && !uri.Equals(String.Empty))
                    {
                        this._parser = new StreamingNTriplesParser(this._manager.DocumentManager.GraphFactory[new Uri(uri)], this._currDoc.BeginRead());
                    }
                    else
                    {
                        this._parser = new StreamingNTriplesParser(this._manager.DocumentManager.GraphFactory[null], this._currDoc.BeginRead());
                    }
                }
                else
                {
                    this._graphCursor = null;
                    break;
                }
            }

            if (!this._parser.EOF)
            {
                this._current = this._parser.GetNextTriple();
                return true;
            }
            else
            {
                //If no further documents contain Triples then nothing further to iterate
                return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException("The FileIterator does not support the Reset() operation");
        }
    }

    class FileEnumerable : IEnumerable<Triple>
    {
        private AlexandriaFileManager _manager;

        public FileEnumerable(AlexandriaFileManager manager)
        {
            this._manager = manager;
        }

        public IEnumerator<Triple> GetEnumerator()
        {
            return new FileEnumerator(this._manager);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
