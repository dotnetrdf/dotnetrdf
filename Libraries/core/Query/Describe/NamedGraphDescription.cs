/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Describe
{
    /// <summary>
    /// Computes a Description for all the results such that the description is the merge of all the Graphs named with a resulting URI
    /// </summary>
    public class NamedGraphDescription : ISparqlDescribe
    {
        /// <summary>
        /// Returns the Graph which is the merge of all the Graphs which are named with URIs contained in the Results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public IGraph Describe(SparqlEvaluationContext context)
        {
            //Get a new empty Graph and import the Base Uri and Namespace Map of the Query
            Graph g = new Graph();
            g.BaseUri = context.Query.BaseUri;
            g.NamespaceMap.Import(context.Query.NamespaceMap);

            //Build a list of INodes to describe
            List<INode> nodes = new List<INode>();
            foreach (IToken t in context.Query.DescribeVariables)
            {
                switch (t.TokenType)
                {
                    case Token.QNAME:
                    case Token.URI:
                        //Resolve Uri/QName
                        nodes.Add(new UriNode(g, new Uri(Tools.ResolveUriOrQName(t, g.NamespaceMap, g.BaseUri))));
                        break;

                    case Token.VARIABLE:
                        //Get Variable Values
                        String var = t.Value.Substring(1);
                        if (context.OutputMultiset.ContainsVariable(var))
                        {
                            foreach (ISet s in context.OutputMultiset.Sets)
                            {
                                INode temp = s[var];
                                if (temp != null) nodes.Add(temp);
                            }
                        }
                        break;

                    default:
                        throw new RdfQueryException("Unexpected Token '" + t.GetType().ToString() + "' in DESCRIBE Variables list");
                }
            }

            foreach (INode n in nodes)
            {
                if (n.NodeType == NodeType.Uri)
                {
                    IUriNode u = (IUriNode)n;
                    if (context.Data.HasGraph(u.Uri))
                    {
                        g.Merge(context.Data[u.Uri]);
                    }
                }
            }

            return g;
        }
    }
}
