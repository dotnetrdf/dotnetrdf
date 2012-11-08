/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    public class TermDetectionHandler
        : BaseRdfHandler
    {
        private INode _rdfType, _rdfsClass, _rdfProperty, _rdfsDatatype, _rdfsLabel, _rdfsComment;
        private HashSet<INode> _terms;
        private Dictionary<INode, String> _termLabels, _termComments;
        private NamespaceMapper _nsmap;

        public TermDetectionHandler()
        {
            this._rdfType = this.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            this._rdfsClass = this.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class"));
            this._rdfProperty = this.CreateUriNode(new Uri(NamespaceMapper.RDF + "Property"));
            this._rdfsDatatype = this.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Datatype"));
            this._rdfsLabel = this.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            this._rdfsComment = this.CreateUriNode(new Uri(NamespaceMapper.RDFS + "comment"));
        }

        public IEnumerable<NamespaceTerm> DetectedTerms
        {
            get
            {
                if (this._terms != null)
                {
                    List<NamespaceTerm> results = new List<NamespaceTerm>();
                    foreach (INode term in this._terms)
                    {
                        //Must be reduceable to a QName
                        String qname;
                        if (this._nsmap.ReduceToQName(term.ToString(), out qname))
                        {
                            String prefix = qname.StartsWith(":") ? String.Empty : qname.Substring(0, qname.IndexOf(':'));
                            String label = this._termLabels.ContainsKey(term) ? this._termLabels[term] : (this._termComments.ContainsKey(term) ? this._termComments[term] : String.Empty);
                            results.Add(new NamespaceTerm(this._nsmap.GetNamespaceUri(prefix).AbsoluteUri, qname, label));
                        }
                    }

                    return results;
                }
                else
                {
                    return Enumerable.Empty<NamespaceTerm>();
                }
            }
        }

        protected override void StartRdfInternal()
        {
            this._terms = new HashSet<INode>();
            this._termLabels = new Dictionary<INode, string>();
            this._termComments = new Dictionary<INode, string>();
            this._nsmap = new NamespaceMapper(true);
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            this._nsmap.AddNamespace(prefix, namespaceUri);
            return true;
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            if (t.Subject.NodeType == NodeType.Uri)
            {
                if (t.Predicate.Equals(this._rdfType))
                {
                    if (t.Object.Equals(this._rdfsClass) || t.Object.Equals(this._rdfsDatatype) || t.Object.Equals(this._rdfProperty))
                    {
                        if (!this._terms.Contains(t.Subject))
                        {
                            this._terms.Add(t.Subject);
                        }
                    }
                }
                else if (t.Predicate.Equals(this._rdfsLabel) && t.Object.NodeType == NodeType.Literal)
                {
                    if (!this._termLabels.ContainsKey(t.Subject))
                    {
                        this._termLabels.Add(t.Subject, ((ILiteralNode)t.Object).Value);
                    }
                }
                else if (t.Predicate.Equals(this._rdfsComment) && t.Object.NodeType == NodeType.Literal)
                {
                    if (!this._termComments.ContainsKey(t.Subject))
                    {
                        this._termLabels.Add(t.Subject, ((ILiteralNode)t.Object).Value);
                    }
                }
            }
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
