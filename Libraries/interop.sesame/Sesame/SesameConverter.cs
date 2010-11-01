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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;

namespace VDS.RDF.Interop.Sesame
{
    public static class SesameConverter
    {

        public static void ToSesame(IGraph g, dotSesame.Graph target)
        {
            SesameMapping mapping = new SesameMapping(g, target);
            ToSesame(g, mapping, target);
        }

        public static void ToSesame(IGraph g, SesameMapping mapping, dotSesame.Graph target)
        {
            foreach (Triple t in g.Triples)
            {
                target.add(ToSesameResource(t.Subject, mapping), ToSesameUri(t.Predicate, mapping), ToSesameValue(t.Object, mapping));
            }
        }

        static dotSesame.Statement ToSesame(Triple t, SesameMapping mapping)
        {
            return mapping.ValueFactory.createStatement(ToSesameResource(t.Subject, mapping), ToSesameUri(t.Predicate, mapping), ToSesameValue(t.Object, mapping));
        }

        static dotSesame.Resource ToSesameResource(INode n, SesameMapping mapping)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    return mapping.ValueFactory.createURI(((UriNode)n).Uri.ToString());

                case NodeType.Blank:
                    if (mapping.OutputMapping.ContainsKey(n))
                    {
                        return mapping.OutputMapping[n];
                    }
                    else
                    {
                        dotSesame.BNode bnode = mapping.ValueFactory.createBNode();

                        lock (mapping)
                        {
                            if (!mapping.OutputMapping.ContainsKey(n))
                            {
                                mapping.OutputMapping.Add(n, bnode);
                            } 
                            else 
                            {
                                bnode = mapping.OutputMapping[n];
                            }
                            if (!mapping.InputMapping.ContainsKey(bnode)) mapping.InputMapping.Add(bnode, n);
                        }
                        return bnode;
                    }

                default:
                    throw new RdfException("Only URI and Blank Node subjects are not supported in Sesame");
            }
        }

        static dotSesame.URI ToSesameUri(INode n, SesameMapping mapping)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    return mapping.ValueFactory.createURI(((UriNode)n).Uri.ToString());
                default:
                    throw new RdfException("Only URI Predicates are supported in Sesame");
            }
        }

        static dotSesame.URI ToSesameUri(Uri u, SesameMapping mapping)
        {
            return mapping.ValueFactory.createURI(u.ToString());
        }

        static dotSesame.Value ToSesameValue(INode n, SesameMapping mapping)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    return mapping.ValueFactory.createURI(((UriNode)n).Uri.ToString());
                case NodeType.Literal:
                    LiteralNode lit = (LiteralNode)n;
                    if (lit.DataType != null)
                    {
                        return mapping.ValueFactory.createLiteral(lit.Value, ToSesameUri(lit.DataType, mapping));
                    }
                    else if (!lit.Language.Equals(String.Empty))
                    {
                        return mapping.ValueFactory.createLiteral(lit.Value, lit.Language);
                    }
                    else
                    {
                        return mapping.ValueFactory.createLiteral(lit.Value);
                    }
                case NodeType.Blank:
                    if (mapping.OutputMapping.ContainsKey(n))
                    {
                        return mapping.OutputMapping[n];
                    }
                    else
                    {
                        dotSesame.BNode bnode = mapping.ValueFactory.createBNode();

                        lock (mapping)
                        {
                            if (!mapping.OutputMapping.ContainsKey(n))
                            {
                                mapping.OutputMapping.Add(n, bnode);
                            }
                            else
                            {
                                bnode = mapping.OutputMapping[n];
                            }
                            if (!mapping.InputMapping.ContainsKey(bnode)) mapping.InputMapping.Add(bnode, n);
                        }
                        return bnode;
                    }

                default:
                    throw new RdfException("Only URI, Blank Node and Literal Objects are supported by Sesame");
            }
        }

                    
    }
}
