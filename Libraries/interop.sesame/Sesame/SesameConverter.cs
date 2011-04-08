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
using java.util;

namespace VDS.RDF.Interop.Sesame
{
    /// <summary>
    /// Class for converting RDF Graphs between the dotNetRDF and Sesame APIs
    /// </summary>
    public static class SesameConverter
    {
        #region Conversion To Sesame API

        /// <summary>
        /// Converts a dotNetRDF Graph to a Sesame Graph
        /// </summary>
        /// <param name="g">dotNetRDF Graph</param>
        /// <param name="target">Sesame Graph</param>
        public static void ToSesame(IGraph g, dotSesame.Graph target)
        {
            SesameMapping mapping = new SesameMapping(g, target);
            ToSesame(g, mapping, target);
        }

        /// <summary>
        /// Converts a dotNetRDF Graph to a Sesame Graph
        /// </summary>
        /// <param name="g">dotNetRDF Graph</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <param name="target">Sesame Graph</param>
        public static void ToSesame(IGraph g, SesameMapping mapping, dotSesame.Graph target)
        {
            foreach (Triple t in g.Triples)
            {
                target.add(ToSesameResource(t.Subject, mapping), ToSesameUri(t.Predicate, mapping), ToSesameValue(t.Object, mapping));
            }
        }

        /// <summary>
        /// Converts a dotNetRDF Triple to a Sesame Statement
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <returns></returns>
        public static dotSesame.Statement ToSesame(Triple t, SesameMapping mapping)
        {
            return mapping.ValueFactory.createStatement(ToSesameResource(t.Subject, mapping), ToSesameUri(t.Predicate, mapping), ToSesameValue(t.Object, mapping));
        }

        /// <summary>
        /// Converts a dotNetRDF Node to a Sesame Resource
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <returns></returns>
        internal static dotSesame.Resource ToSesameResource(INode n, SesameMapping mapping)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    return mapping.ValueFactory.createURI(((IUriNode)n).Uri.ToString());

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
                    throw new RdfException("Only URI and Blank Node subjects are supported in Sesame");
            }
        }

        /// <summary>
        /// Converts a dotNetRDF Node to a Sesame URI
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <returns></returns>
        internal static dotSesame.URI ToSesameUri(INode n, SesameMapping mapping)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    return mapping.ValueFactory.createURI(((IUriNode)n).Uri.ToString());
                default:
                    throw new RdfException("Only URI Predicates are supported in Sesame");
            }
        }

        /// <summary>
        /// Converts a URI to a Sesame URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <returns></returns>
        internal static dotSesame.URI ToSesameUri(Uri u, SesameMapping mapping)
        {
            return mapping.ValueFactory.createURI(u.ToString());
        }

        /// <summary>
        /// Converts a dotNetRDF Node to a Sesame Value
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <returns></returns>
        internal static dotSesame.Value ToSesameValue(INode n, SesameMapping mapping)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    return mapping.ValueFactory.createURI(((IUriNode)n).Uri.ToString());
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)n;
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

        #endregion

        #region Conversion From Sesame API

        /// <summary>
        /// Converts a Sesame Graph to a dotNetRDF Graph
        /// </summary>
        /// <param name="source">Sesame Graph</param>
        /// <param name="target">dotNetRDF Graph</param>
        public static void FromSesame(dotSesame.Graph source, IGraph target)
        {
            SesameMapping mapping = new SesameMapping(target, source);
            FromSesame(source, mapping, target);
        }

        /// <summary>
        /// Converts a Sesame Graph to a dotNetRDF Graph
        /// </summary>
        /// <param name="source">Sesame Graph</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <param name="target">dotNetRDF Graph</param>
        public static void FromSesame(dotSesame.Graph source, SesameMapping mapping, IGraph target)
        {
            Iterator iter = source.iterator();
            while (iter.hasNext())
            {
                dotSesame.Statement stmt = (dotSesame.Statement)iter.next();
                target.Assert(FromSesame(stmt, mapping));
            }
        }

        /// <summary>
        /// Converts a Sesame Statement to a dotNetRDF Triple
        /// </summary>
        /// <param name="statement">Sesame Statement</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <returns></returns>
        public static Triple FromSesame(dotSesame.Statement statement, SesameMapping mapping)
        {
            return new Triple(FromSesameResource(statement.getSubject(), mapping), FromSesameUri(statement.getPredicate(), mapping), FromSesameValue(statement.getObject(), mapping));
        }

        /// <summary>
        /// Converts a Sesame Resource to a dotNetRDF Node
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <returns></returns>
        internal static INode FromSesameResource(dotSesame.Resource resource, SesameMapping mapping)
        {
            if (resource is dotSesame.URI)
            {
                return mapping.Graph.CreateUriNode(new Uri(((dotSesame.URI)resource).stringValue()));
            }
            else if (resource is dotSesame.BNode)
            {
                dotSesame.BNode bnode = (dotSesame.BNode)resource;
                if (mapping.InputMapping.ContainsKey(bnode))
                {
                    return mapping.InputMapping[bnode];
                }
                else
                {
                    INode n = mapping.Graph.CreateBlankNode();
                    lock (mapping)
                    {
                        if (!mapping.InputMapping.ContainsKey(bnode))
                        {
                            mapping.InputMapping.Add(bnode, n);
                        }
                        else
                        {
                            n = mapping.InputMapping[bnode];
                        }
                        if (!mapping.OutputMapping.ContainsKey(n)) mapping.OutputMapping.Add(n, bnode);
                    }
                    return n;
                }
            }
            else
            {
                throw new RdfException("Unable to convert unexpected Sesame Resource Type to a dotNetRDF INode");
            }
        }

        /// <summary>
        /// Converts a Sesame URI to a dotNetRDF Node
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <returns></returns>
        internal static INode FromSesameUri(dotSesame.URI uri, SesameMapping mapping)
        {
            return mapping.Graph.CreateUriNode(new Uri(uri.stringValue()));
        }

        /// <summary>
        /// Converts a Sesame URI to a URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        internal static Uri FromSesameUri(dotSesame.URI uri)
        {
            return new Uri(uri.stringValue());
        }

        /// <summary>
        /// Converts a Sesame Value to a dotNetRDF Node
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <returns></returns>
        internal static INode FromSesameValue(dotSesame.Value value, SesameMapping mapping)
        {
            if (value is dotSesame.URI)
            {
                return mapping.Graph.CreateUriNode(new Uri(((dotSesame.URI)value).stringValue()));
            }
            else if (value is dotSesame.Literal)
            {
                dotSesame.Literal lit = (dotSesame.Literal)value;
                if (lit.getDatatype() != null)
                {
                    return mapping.Graph.CreateLiteralNode(lit.stringValue(), FromSesameUri(lit.getDatatype()));
                }
                else if (lit.getLanguage() != null)
                {
                    return mapping.Graph.CreateLiteralNode(lit.stringValue(), lit.getLanguage());
                }
                else
                {
                    return mapping.Graph.CreateLiteralNode(lit.stringValue());
                }
            }
            else if (value is dotSesame.BNode)
            {
                dotSesame.BNode bnode = (dotSesame.BNode)value;
                if (mapping.InputMapping.ContainsKey(bnode))
                {
                    return mapping.InputMapping[bnode];
                }
                else
                {
                    INode n = mapping.Graph.CreateBlankNode();
                    lock (mapping)
                    {
                        if (!mapping.InputMapping.ContainsKey(bnode))
                        {
                            mapping.InputMapping.Add(bnode, n);
                        }
                        else
                        {
                            n = mapping.InputMapping[bnode];
                        }
                        if (!mapping.OutputMapping.ContainsKey(n)) mapping.OutputMapping.Add(n, bnode);
                    }
                    return n;
                }
            }
            else
            {
                throw new RdfException("Unable to convert unexpected Sesame Value Type to a dotNetRDF INode");
            }
        }

        #endregion


    }
}
