/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
