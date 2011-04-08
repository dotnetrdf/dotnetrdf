using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.hp.hpl.jena.rdf.model;
using com.hp.hpl.jena.datatypes;

namespace VDS.RDF.Interop.Jena
{
    public static class JenaConverter
    {
        public static void ToJena(IGraph g, Model m)
        {
            JenaMapping mapping = new JenaMapping(g, m);
            ToJena(g, mapping, m);
        }

        public static void FromJena(Model m, IGraph g)
        {
            JenaMapping mapping = new JenaMapping(g, m);
            FromJena(m, mapping, g);
        }

        public static void ToJena(IGraph g, JenaMapping mapping, Model m)
        {
            Statement stmt;

            foreach (Triple t in g.Triples)
            {
                m.add(ToJena(t, mapping));
            }
        }

        public static void FromJena(Model m, JenaMapping mapping, IGraph g)
        {
            StmtIterator iter = m.listStatements();
            while (iter.hasNext())
            {
                Statement stmt = iter.nextStatement();
                g.Assert(FromJena(stmt, mapping));
            }
        }

        public static Statement ToJena(Triple t, JenaMapping mapping)
        {
            Resource s;
            Property p;
            RDFNode o;
            s = ToJenaResource(t.Subject, mapping);
            p = ToJenaProperty(t.Predicate, mapping);
            o = ToJenaNode(t.Object, mapping);

            return mapping.Model.createStatement(s, p, o);
        }

        public static Triple FromJena(Statement stmt, JenaMapping mapping)
        {
            INode s, p, o;
            s = FromJenaResource(stmt.getSubject(), mapping);
            p = FromJenaProperty(stmt.getPredicate(), mapping);
            o = FromJenaNode(stmt.getObject(), mapping);

            return new Triple(s, p, o);
        }

        public static RDFNode ToJenaNode(INode n, JenaMapping mapping)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    return mapping.Model.createResource(n.ToString());
                case NodeType.Blank:
                    if (mapping.OutputMapping.ContainsKey(n))
                    {
                        return mapping.OutputMapping[n];
                    }
                    else
                    {
                        AnonId id = new AnonId(((IBlankNode)n).InternalID);
                        Resource bnode = mapping.Model.createResource(id);
                        mapping.OutputMapping.Add(n, bnode);
                        if (!mapping.InputMapping.ContainsKey(bnode))
                        {
                            mapping.InputMapping.Add(bnode, n);
                        }
                        return bnode;
                    }
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        return mapping.Model.createTypedLiteral(lit.Value, TypeMapper.getInstance().getSafeTypeByName(lit.DataType.ToString()));
                    } 
                    else if (!lit.Language.Equals(String.Empty))
                    {
                        return mapping.Model.createLiteral(lit.Value, lit.Language);
                    }
                    else 
                    {
                        return mapping.Model.createLiteral(lit.Value);
                    }
                default:
                    throw new RdfException("Only URI/Blank/Literal Nodes can be converted to Jena Nodes");
            }
        }

        public static INode FromJenaNode(RDFNode n, JenaMapping mapping)
        {
            if (n.isResource())
            {
                return FromJenaResource((Resource)n, mapping);
            }
            else if (n.isLiteral())
            {
                Literal lit = (Literal)n;
                if (lit.getDatatypeURI() != null)
                {
                    return mapping.Graph.CreateLiteralNode(lit.getLexicalForm(), new Uri(lit.getDatatypeURI()));
                }
                else if (!lit.getLanguage().Equals(String.Empty))
                {
                    return mapping.Graph.CreateLiteralNode(lit.getLexicalForm(), lit.getLanguage());
                }
                else
                {
                    return mapping.Graph.CreateLiteralNode(lit.getLexicalForm());
                }
            }
            else
            {
                throw new RdfException("Unable to convert from an unknown Jena Node type to a dotNetRDF Node");
            }
        }

        public static Resource ToJenaResource(INode n, JenaMapping mapping)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    return mapping.Model.createResource(n.ToString());
                case NodeType.Blank:
                    if (mapping.OutputMapping.ContainsKey(n))
                    {
                        return mapping.OutputMapping[n];
                    } 
                    else 
                    {
                        AnonId id = new AnonId(((IBlankNode)n).InternalID);
                        Resource bnode = mapping.Model.createResource(id);
                        mapping.OutputMapping.Add(n, bnode);
                        if (!mapping.InputMapping.ContainsKey(bnode))
                        {
                            mapping.InputMapping.Add(bnode, n);
                        }
                        return bnode;
                    }
                default:
                    throw new RdfException("Only URI/Blank Nodes can be converted to Jena Resources");
            }
        }

        public static INode FromJenaResource(Resource r, JenaMapping mapping)
        {
            if (r.isAnon())
            {
                if (mapping.InputMapping.ContainsKey(r))
                {
                    return mapping.InputMapping[r];
                }
                else
                {
                    INode bnode = mapping.Graph.CreateBlankNode(r.getId().getLabelString());
                    mapping.InputMapping.Add(r, bnode);
                    if (!mapping.OutputMapping.ContainsKey(bnode))
                    {
                        mapping.OutputMapping.Add(bnode, r);
                    }
                    return bnode;
                }
            }
            else if (r.isURIResource())
            {
                return mapping.Graph.CreateUriNode(new Uri(r.getURI()));
            } 
            else 
            {
                throw new RdfException("Unable to convert from an unknown Jena Resource type to a dotNetRDF Node");
            }
        }

        public static Property ToJenaProperty(INode n, JenaMapping mapping)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    return mapping.Model.createProperty(n.ToString());
                default:
                    throw new RdfException("Only URI Nodes can be converted to Jena Properties");
            }
        }

        public static INode FromJenaProperty(Property p, JenaMapping mapping)
        {
            return mapping.Graph.CreateUriNode(new Uri(p.getURI()));
        }
    }
}
