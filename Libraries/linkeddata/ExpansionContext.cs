using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.LinkedData.Profiles;

namespace VDS.RDF.LinkedData
{
    class ExpansionContext
    {
        private TripleStore _store = new TripleStore();
        private Queue<UriToExpand> _uris = new Queue<UriToExpand>();
        private ExpansionProfile _profile;
        private IUriNode _sameAs, _seeAlso;
        private bool _multithreading = false;
        private ThreadSafeGraph _linkTree = new ThreadSafeGraph();

        public ExpansionContext(ExpansionProfile profile)
        {
            this._profile = profile;

            this._linkTree.BaseUri = new Uri(VDS.RDF.Writing.WriterHelper.StoreDefaultGraphURIs.First());
            this._linkTree.NamespaceMap.AddNamespace("owl", new Uri(NamespaceMapper.OWL));
            this._sameAs = this._linkTree.CreateUriNode("owl:sameAs");
            this._seeAlso = this._linkTree.CreateUriNode("rdfs:seeAlso");
            this._store.Add(this._linkTree);
        }

        public IInMemoryQueryableStore Store
        {
            get
            {
                return this._store;
            }
        }

        public Queue<UriToExpand> Uris
        {
            get
            {
                return this._uris;
            }
        }

        public UriToExpand GetNextUri()
        {
            UriToExpand temp = null;
            try
            {
                Monitor.Enter(this._uris);
                if (this._uris.Count > 0)
                {
                    temp = this._uris.Dequeue();
                }
            }
            finally
            {
                Monitor.Exit(this._uris);
            }
            return temp;
        }

        public ExpansionProfile Profile
        {
            get
            {
                return this._profile;
            }
        }

        public IUriNode SameAs
        {
            get
            {
                return this._sameAs;
            }
        }

        public IUriNode SeeAlso
        {
            get
            {
                return this._seeAlso;
            }
        }

        public bool MultiThreading
        {
            get
            {
                return this._multithreading;
            }
            set
            {
                this._multithreading = value;
            }
        }

        public IGraph LinkGraph
        {
            get
            {
                return this._linkTree;
            }
        }
    }
}
