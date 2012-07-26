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
using SemWeb;

namespace VDS.RDF.Interop.SemWeb
{
    /// <summary>
    /// Class for converting RDF Graphs between the dotNetRDF and SemWeb API models
    /// </summary>
    public static class SemWebConverter
    {
        /// <summary>
        /// Takes the contents of a dotNetRDF Graph and inputs it into a SemWeb StatementSink
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="sink">Statement Sink</param>
        public static void ToSemWeb(IGraph g, StatementSink sink)
        {
            SemWebMapping mapping = new SemWebMapping(g);
            ToSemWeb(g, mapping, sink);
        }

        /// <summary>
        /// Takes the contents of a dotNetRDF Graph and inputs it into a SemWeb StatementSink
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Blank Node Mapping</param>
        /// <param name="sink">Statement Sink</param>
        public static void ToSemWeb(IGraph g, SemWebMapping mapping, StatementSink sink)
        {
            Statement stmt;

            foreach (Triple t in g.Triples)
            {
                stmt = ToSemWeb(t, mapping);
                //Stop adding statements if the sink tells up to stop
                if (!sink.Add(stmt)) return;
            }
        }

        /// <summary>
        /// Converts a dotNetRDF Triple to a SemWeb Statement
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public static Statement ToSemWeb(Triple t, SemWebMapping mapping)
        {
            Entity s, p;
            Resource o;
            Statement stmt;
            if (t.IsGroundTriple)
            {
                //Easy to map across without need for BNode mapping
                stmt = new Statement(ToSemWebEntity(t.Subject), ToSemWebEntity(t.Predicate), ToSemWeb(t.Object));
            }
            else
            {
                s = ToSemWebEntity(t.Subject, mapping);
                p = ToSemWebEntity(t.Predicate, mapping);
                o = ToSemWeb(t.Object, mapping);

                stmt = new Statement(s, p, o);
            }
            return stmt;
        }

        /// <summary>
        /// Converts a dotNetRDF Node to a SemWeb Resource
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public static Resource ToSemWeb(INode n, SemWebMapping mapping)
        {
            if (n.NodeType == NodeType.Blank)
            {
                if (mapping.OutputMapping.ContainsKey(n))
                {
                    return mapping.OutputMapping[n];
                }
                else
                {
                    Entity temp = ToSemWebEntity(n);
                    mapping.OutputMapping.Add(n, temp);
                    if (!mapping.InputMapping.ContainsKey(temp.ToString())) mapping.InputMapping.Add(temp.ToString(), n);
                    return temp;
                }
            }
            else
            {
                return ToSemWeb(n);
            }
        }

        /// <summary>
        /// Converts a dotNetRDF Node to a SemWeb Entity
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public static Entity ToSemWebEntity(INode n, SemWebMapping mapping)
        {
            Resource r = ToSemWeb(n, mapping);
            return (Entity)r;
        }
        
        /// <summary>
        /// Converts a dotNetRDF Node to a SemWeb Resource
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        static Resource ToSemWeb(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return new BNode(((IBlankNode)n).InternalID);
                case NodeType.GraphLiteral:
                    throw new RdfException("Graph Literal Nodes cannot be converted to SemWeb Resources");
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        return new Literal(lit.Value, null, lit.DataType.ToString());
                    }
                    else if (!lit.Language.Equals(String.Empty))
                    {
                        return new Literal(lit.Value, lit.Language, null);
                    }
                    else
                    {
                        return new Literal(lit.Value);
                    }
                case NodeType.Uri:
                    return new Entity(((IUriNode)n).Uri.ToString());
                default:
                    throw new RdfException("Unknown Node Types cannot be converted to SemWeb Resources");
            }
        }

        /// <summary>
        /// Converts a dotNetRDF Node to a SemWeb Entity
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        static Entity ToSemWebEntity(INode n)
        {
            Object temp = ToSemWeb(n);
            if (temp is Entity)
            {
                return (Entity)temp;
            }
            else
            {
                if (n.NodeType == NodeType.Literal)
                {
                    throw new RdfException("SemWeb does not support Literal Nodes as Subjects");
                }
                else
                {
                    throw new RdfException("Unable to convert the given Node to a SemWeb Entity");
                }
            }
        }

        /// <summary>
        /// Takes the contents of a SemWeb Statement Source and inputs it into a dotNetRDF Graph
        /// </summary>
        /// <param name="source">Statement Source</param>
        /// <param name="g">Graph</param>
        public static void FromSemWeb(StatementSource source, IGraph g)
        {
            SemWebMapping mapping = new SemWebMapping(g);
            Triple t;
            
            MemoryStore mem = new MemoryStore();
            source.Select(mem);

            foreach (Statement stmt in mem)
            {
                t = FromSemWeb(stmt, mapping);
                g.Assert(t);
            }
        }

        /// <summary>
        /// Converts a SemWeb Statement to a dotNetRDF Triple
        /// </summary>
        /// <param name="stmt">Statement</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public static Triple FromSemWeb(Statement stmt, SemWebMapping mapping)
        {
            INode s, p, o;
            s = FromSemWeb(stmt.Subject, mapping);
            p = FromSemWeb(stmt.Predicate, mapping);
            o = FromSemWeb(stmt.Object, mapping);
            return new Triple(s, p, o);
        }

        /// <summary>
        /// Converts a SemWeb Resource into a dotNetRDF Node
        /// </summary>
        /// <param name="r">Resource</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public static INode FromSemWeb(Resource r, SemWebMapping mapping)
        {
            if (r is Entity)
            {
                return FromSemWebEntity((Entity)r, mapping);
            }
            else if (r is Literal)
            {
                Literal lit = (Literal)r;
                if (lit.DataType != null)
                {
                    return mapping.Graph.CreateLiteralNode(lit.Value, new Uri(lit.DataType));
                }
                else if (lit.Language != null)
                {
                    return mapping.Graph.CreateLiteralNode(lit.Value, lit.Language);
                }
                else
                {
                    return mapping.Graph.CreateLiteralNode(lit.Value);
                }
            }
            else
            {
                throw new RdfException("Cannot convert an unknown SemWeb Resource type to a dotNetRDF Node");
            }
        }

        /// <summary>
        /// Converts a SemWeb Entity into a dotNetRDF Node
        /// </summary>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <param name="e">Entity</param>
        /// <returns></returns>
        public static INode FromSemWebEntity(Entity e, SemWebMapping mapping)
        {
            if (e.Uri == null)
            {
                if (mapping.InputMapping.ContainsKey(e.ToString()))
                {
                    return mapping.InputMapping[e.ToString()];
                }
                else
                {
                    INode temp = mapping.Graph.CreateBlankNode();
                    mapping.InputMapping.Add(e.ToString(), temp);
                    if (!mapping.OutputMapping.ContainsKey(temp)) mapping.OutputMapping.Add(temp, e);
                    return temp;
                }
            }
            else
            {
                return mapping.Graph.CreateUriNode(new Uri(e.Uri));
            }
        }
    }
}
