using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which asserts Triples into a Graph
    /// </summary>
    public class GraphHandler : BaseRdfHandler
    {
        private IGraph _target;
        private IGraph _g;

        public GraphHandler(IGraph g)
            : base(g)
        {
            if (g == null) throw new ArgumentNullException("graph");
            this._g = g;
        }

        public Uri BaseUri
        {
            get
            {
                if (this._target != null)
                {
                    return this._target.BaseUri;
                }
                else
                {
                    return this._g.BaseUri;
                }
            }
        }

        protected override void StartRdfInternal()
        {
            if (this._g.IsEmpty)
            {
                this._target = this._g;
            }
            else
            {
                this._target = new Graph(true);
                this._target.NamespaceMap.Import(this._g.NamespaceMap);
                this._target.BaseUri = this._g.BaseUri;
            }
            this.NodeFactory = this._target;
        }

        protected override void EndRdfInternal(bool ok)
        {
            if (ok)
            {
                //If the Target Graph was different from the Destination Graph then do a Merge
                if (!ReferenceEquals(this._g, this._target))
                {
                    this._g.Merge(this._target);
                    this._g.NamespaceMap.Import(this._target.NamespaceMap);
                    if (this._g.BaseUri == null) this._g.BaseUri = this._target.BaseUri;
                }
                else
                {
                    //The Target was the Graph so we want to set our reference to it to be null so we don't
                    //clear it in the remainder of our clean up step
                    this._target = null;
                }
            }
            else
            {
                //Discard the Parsed Triples if parsing failed
                if (ReferenceEquals(this._g, this._target))
                {
                    this._g.Clear();
                    this._target = null;
                }
            }

            //Always throw away the target afterwards if not already done so
            if (this._target != null)
            {
                this._target.Clear();
                this._target = null;
            }
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            this._target.NamespaceMap.AddNamespace(prefix, namespaceUri);
            return true;
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            this._target.BaseUri = baseUri;
            return true;
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            this._target.Assert(t);
            return true;
        }

        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }
    }
}
