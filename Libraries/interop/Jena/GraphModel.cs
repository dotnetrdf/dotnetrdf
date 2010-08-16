using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.hp.hpl.jena.rdf.model;
using java.util;

namespace VDS.RDF.Interop.Jena
{
    class GraphModel : Model
    {
        private JenaMapping _mapping;
        private IGraph _g;

        public GraphModel(IGraph g)
        {
            this._g = g;
            this._mapping = new JenaMapping(this._g, this);
        }

        private NotSupportedException NotSupported(String method)
        {
            return new NotSupportedException("The " + method + "() method is not supported by the dotNetRDF GraphModel");
        }

        private IEnumerable<Triple> GetTriples(Resource r, Property p, RDFNode rdfn)
        {
            if (r == null)
            {
                if (p == null)
                {
                    //Object specified
                    return this._g.GetTriplesWithObject(JenaConverter.FromJenaNode(rdfn, this._mapping));
                }
                else if (rdfn == null)
                {
                    //Predicate specified
                    return this._g.GetTriplesWithPredicate(JenaConverter.FromJenaProperty(p, this._mapping));
                }
                else
                {
                    //Object and Predicate specified
                    return this._g.GetTriplesWithPredicateObject(JenaConverter.FromJenaProperty(p, this._mapping), JenaConverter.FromJenaNode(rdfn, this._mapping));
                }
            }
            else if (p == null)
            {
                if (rdfn == null)
                {
                    //Subject specified
                    return this._g.GetTriplesWithSubject(JenaConverter.FromJenaResource(r, this._mapping));
                }
                else
                {
                    //Subject and Object specified
                    return this._g.GetTriplesWithSubjectObject(JenaConverter.FromJenaResource(r, this._mapping), JenaConverter.FromJenaNode(rdfn, this._mapping));
                }
            }
            else if (rdfn == null)
            {
                //Subject and Predicate specified
                return this._g.GetTriplesWithSubjectPredicate(JenaConverter.FromJenaResource(r, this._mapping), JenaConverter.FromJenaProperty(p, this._mapping));
            }
            else
            {
                Triple t = new Triple(JenaConverter.FromJenaResource(r, this._mapping), JenaConverter.FromJenaProperty(p, this._mapping), JenaConverter.FromJenaNode(rdfn, this._mapping));
                if (this._g.ContainsTriple(t))
                {
                    return t.AsEnumerable();
                }
                else
                {
                    return Enumerable.Empty<Triple>();
                }
            }
        }

        public Model abort()
        {
            throw NotSupported("abort");
        }

        public Model add(Model m, bool b)
        {
            Graph h = new Graph();
            JenaConverter.FromJena(m, h);
            if (b)
            {
                //TODO: Suppress reification triples
            }
            this._g.Merge(h);
            return this;
        }

        public Model add(Model m)
        {
            Graph h = new Graph();
            JenaConverter.FromJena(m, h);
            this._g.Merge(h);
            return this;
        }

        public Model add(StmtIterator si)
        {
            while (si.hasNext())
            {
                Statement stmt = si.nextStatement();
                this._g.Assert(JenaConverter.FromJena(stmt, this._mapping));
            }
            return this;
        }

        public Model add(List l)
        {
            ListIterator iter = l.listIterator();
            while (iter.hasNext())
            {
                Object obj = iter.next();
                if (obj is Statement)
                {
                    this._g.Assert(JenaConverter.FromJena((Statement)obj, this._mapping));
                }
            }
            return this;
        }

        public Model add(Statement[] sarr)
        {
            foreach (Statement stmt in sarr)
            {
                this._g.Assert(JenaConverter.FromJena(stmt, this._mapping));
            }
            return this;
        }

        public Model add(Statement s)
        {
            this._g.Assert(JenaConverter.FromJena(s, this._mapping));
            return this;
        }

        public Model begin()
        {
            throw NotSupported("begin");
        }

        public void close()
        {
            this._g.Dispose();
            this._mapping.InputMapping.Clear();
            this._mapping.OutputMapping.Clear();
        }

        public Model commit()
        {
            throw NotSupported("commit");
        }

        public bool contains(Statement s)
        {
            return this._g.ContainsTriple(JenaConverter.FromJena(s, this._mapping));
        }

        public bool contains(Resource r, Property p)
        {
            return this._g.GetTriplesWithSubjectPredicate(JenaConverter.FromJenaResource(r, this._mapping), JenaConverter.FromJenaProperty(p, this._mapping)).Any();
        }

        public bool contains(Resource r, Property p, RDFNode rdfn)
        {
            return this.GetTriples(r, p, rdfn).Any();
        }

        public bool containsAll(Model m)
        {
            throw new NotImplementedException();
        }

        public bool containsAll(StmtIterator si)
        {
            throw new NotImplementedException();
        }

        public bool containsAny(Model m)
        {
            throw new NotImplementedException();
        }

        public bool containsAny(StmtIterator si)
        {
            throw new NotImplementedException();
        }

        public bool containsResource(RDFNode rdfn)
        {
            throw new NotImplementedException();
        }

        public RDFList createList(RDFNode[] rdfnarr)
        {
            throw new NotImplementedException();
        }

        public RDFList createList(java.util.Iterator i)
        {
            throw new NotImplementedException();
        }

        public RDFList createList()
        {
            throw new NotImplementedException();
        }

        public Literal createLiteral(string str, bool b)
        {
            throw new NotImplementedException();
        }

        public Literal createLiteral(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public Property createProperty(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public ReifiedStatement createReifiedStatement(string str, Statement s)
        {
            throw new NotImplementedException();
        }

        public ReifiedStatement createReifiedStatement(Statement s)
        {
            throw new NotImplementedException();
        }

        public Resource createResource()
        {
            throw new NotImplementedException();
        }

        public Resource createResource(AnonId ai)
        {
            throw new NotImplementedException();
        }

        public Resource createResource(string str)
        {
            throw new NotImplementedException();
        }

        public Statement createStatement(Resource r, Property p, RDFNode rdfn)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(object obj)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(object obj, com.hp.hpl.jena.datatypes.RDFDatatype rdfd)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(string str, com.hp.hpl.jena.datatypes.RDFDatatype rdfd)
        {
            throw new NotImplementedException();
        }

        public Model difference(Model m)
        {
            throw new NotImplementedException();
        }

        public bool equals(object obj)
        {
            throw new NotImplementedException();
        }

        public object executeInTransaction(com.hp.hpl.jena.shared.Command c)
        {
            throw new NotImplementedException();
        }

        public Resource getAnyReifiedStatement(Statement s)
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.shared.Lock getLock()
        {
            throw new NotImplementedException();
        }

        public Statement getProperty(Resource r, Property p)
        {
            throw new NotImplementedException();
        }

        public Property getProperty(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.shared.ReificationStyle getReificationStyle()
        {
            throw new NotImplementedException();
        }

        public Statement getRequiredProperty(Resource r, Property p)
        {
            throw new NotImplementedException();
        }

        public Resource getResource(string str)
        {
            throw new NotImplementedException();
        }

        public bool independent()
        {
            throw new NotImplementedException();
        }

        public Model intersection(Model m)
        {
            throw new NotImplementedException();
        }

        public bool isClosed()
        {
            throw new NotImplementedException();
        }

        public bool isEmpty()
        {
            throw new NotImplementedException();
        }

        public bool isIsomorphicWith(Model m)
        {
            throw new NotImplementedException();
        }

        public bool isReified(Statement s)
        {
            throw new NotImplementedException();
        }

        public NsIterator listNameSpaces()
        {
            throw new NotImplementedException();
        }

        public NodeIterator listObjects()
        {
            throw new NotImplementedException();
        }

        public NodeIterator listObjectsOfProperty(Resource r, Property p)
        {
            throw new NotImplementedException();
        }

        public NodeIterator listObjectsOfProperty(Property p)
        {
            throw new NotImplementedException();
        }

        public RSIterator listReifiedStatements(Statement s)
        {
            throw new NotImplementedException();
        }

        public RSIterator listReifiedStatements()
        {
            throw new NotImplementedException();
        }

        public ResIterator listResourcesWithProperty(Property p, RDFNode rdfn)
        {
            throw new NotImplementedException();
        }

        public ResIterator listResourcesWithProperty(Property p)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listStatements(Selector s)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listStatements(Resource r, Property p, RDFNode rdfn)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listStatements()
        {
            throw new NotImplementedException();
        }

        public ResIterator listSubjects()
        {
            throw new NotImplementedException();
        }

        public ResIterator listSubjectsWithProperty(Property p, RDFNode rdfn)
        {
            throw new NotImplementedException();
        }

        public ResIterator listSubjectsWithProperty(Property p)
        {
            throw new NotImplementedException();
        }

        public Model notifyEvent(object obj)
        {
            throw new NotImplementedException();
        }

        public Model query(Selector s)
        {
            throw new NotImplementedException();
        }

        public Model read(string str1, string str2, string str3)
        {
            throw new NotImplementedException();
        }

        public Model read(java.io.Reader r, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public Model read(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public Model read(java.io.Reader r, string str)
        {
            throw new NotImplementedException();
        }

        public Model read(string str)
        {
            throw new NotImplementedException();
        }

        public Model read(java.io.InputStream @is, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public Model read(java.io.InputStream @is, string str)
        {
            throw new NotImplementedException();
        }

        public Model register(ModelChangedListener mcl)
        {
            throw new NotImplementedException();
        }

        public Model remove(Statement s)
        {
            throw new NotImplementedException();
        }

        public Model remove(java.util.List l)
        {
            throw new NotImplementedException();
        }

        public Model remove(Statement[] sarr)
        {
            throw new NotImplementedException();
        }

        public Model removeAll(Resource r, Property p, RDFNode rdfn)
        {
            throw new NotImplementedException();
        }

        public Model removeAll()
        {
            throw new NotImplementedException();
        }

        public void removeAllReifications(Statement s)
        {
            throw new NotImplementedException();
        }

        public void removeReification(ReifiedStatement rs)
        {
            throw new NotImplementedException();
        }

        public long size()
        {
            throw new NotImplementedException();
        }

        public bool supportsSetOperations()
        {
            throw new NotImplementedException();
        }

        public bool supportsTransactions()
        {
            throw new NotImplementedException();
        }

        public Model union(Model m)
        {
            throw new NotImplementedException();
        }

        public Model unregister(ModelChangedListener mcl)
        {
            throw new NotImplementedException();
        }

        public Model write(java.io.OutputStream os)
        {
            throw new NotImplementedException();
        }

        public Model write(java.io.Writer w, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public Model write(java.io.Writer w)
        {
            throw new NotImplementedException();
        }

        public Model write(java.io.OutputStream os, string str)
        {
            throw new NotImplementedException();
        }

        public Model write(java.io.OutputStream os, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public Model write(java.io.Writer w, string str)
        {
            throw new NotImplementedException();
        }

        public Model add(Resource r, Property p, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public Model add(Resource r, Property p, string str, bool b)
        {
            throw new NotImplementedException();
        }

        public Model add(Resource r, Property p, string str, com.hp.hpl.jena.datatypes.RDFDatatype rdfd)
        {
            throw new NotImplementedException();
        }

        public Model add(Resource r, Property p, string str)
        {
            throw new NotImplementedException();
        }

        public Model add(Resource r, Property p, RDFNode rdfn)
        {
            throw new NotImplementedException();
        }

        public Model addLiteral(Resource r, Property p, Literal l)
        {
            throw new NotImplementedException();
        }

        public Model addLiteral(Resource r, Property p, object obj)
        {
            throw new NotImplementedException();
        }

        public Model addLiteral(Resource r, Property p, double d)
        {
            throw new NotImplementedException();
        }

        public Model addLiteral(Resource r, Property p, float f)
        {
            throw new NotImplementedException();
        }

        public Model addLiteral(Resource r, Property p, char ch)
        {
            throw new NotImplementedException();
        }

        public Model addLiteral(Resource r, Property p, int i)
        {
            throw new NotImplementedException();
        }

        public Model addLiteral(Resource r, Property p, long l)
        {
            throw new NotImplementedException();
        }

        public Model addLiteral(Resource r, Property p, bool b)
        {
            throw new NotImplementedException();
        }

        public bool contains(Resource r, Property p, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public bool contains(Resource r, Property p, string str)
        {
            throw new NotImplementedException();
        }

        public bool containsLiteral(Resource r, Property p, object obj)
        {
            throw new NotImplementedException();
        }

        public bool containsLiteral(Resource r, Property p, double d)
        {
            throw new NotImplementedException();
        }

        public bool containsLiteral(Resource r, Property p, float f)
        {
            throw new NotImplementedException();
        }

        public bool containsLiteral(Resource r, Property p, char ch)
        {
            throw new NotImplementedException();
        }

        public bool containsLiteral(Resource r, Property p, int i)
        {
            throw new NotImplementedException();
        }

        public bool containsLiteral(Resource r, Property p, long l)
        {
            throw new NotImplementedException();
        }

        public bool containsLiteral(Resource r, Property p, bool b)
        {
            throw new NotImplementedException();
        }

        public Alt createAlt(string str)
        {
            throw new NotImplementedException();
        }

        public Alt createAlt()
        {
            throw new NotImplementedException();
        }

        public Bag createBag(string str)
        {
            throw new NotImplementedException();
        }

        public Bag createBag()
        {
            throw new NotImplementedException();
        }

        public Literal createLiteral(string str)
        {
            throw new NotImplementedException();
        }

        public Statement createLiteralStatement(Resource r, Property p, object obj)
        {
            throw new NotImplementedException();
        }

        public Statement createLiteralStatement(Resource r, Property p, char ch)
        {
            throw new NotImplementedException();
        }

        public Statement createLiteralStatement(Resource r, Property p, int i)
        {
            throw new NotImplementedException();
        }

        public Statement createLiteralStatement(Resource r, Property p, long l)
        {
            throw new NotImplementedException();
        }

        public Statement createLiteralStatement(Resource r, Property p, double d)
        {
            throw new NotImplementedException();
        }

        public Statement createLiteralStatement(Resource r, Property p, float f)
        {
            throw new NotImplementedException();
        }

        public Statement createLiteralStatement(Resource r, Property p, bool b)
        {
            throw new NotImplementedException();
        }

        public Property createProperty(string str)
        {
            throw new NotImplementedException();
        }

        public Resource createResource(string str, ResourceF rf)
        {
            throw new NotImplementedException();
        }

        public Resource createResource(ResourceF rf)
        {
            throw new NotImplementedException();
        }

        public Resource createResource(string str, Resource r)
        {
            throw new NotImplementedException();
        }

        public Resource createResource(Resource r)
        {
            throw new NotImplementedException();
        }

        public Seq createSeq(string str)
        {
            throw new NotImplementedException();
        }

        public Seq createSeq()
        {
            throw new NotImplementedException();
        }

        public Statement createStatement(Resource r, Property p, string str1, string str2, bool b)
        {
            throw new NotImplementedException();
        }

        public Statement createStatement(Resource r, Property p, string str, bool b)
        {
            throw new NotImplementedException();
        }

        public Statement createStatement(Resource r, Property p, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public Statement createStatement(Resource r, Property p, string str)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(object obj, string str)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(string str)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(double d)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(float f)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(char ch)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(java.util.Calendar c)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(long l)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(bool b)
        {
            throw new NotImplementedException();
        }

        public Literal createTypedLiteral(int i)
        {
            throw new NotImplementedException();
        }

        public Alt getAlt(Resource r)
        {
            throw new NotImplementedException();
        }

        public Alt getAlt(string str)
        {
            throw new NotImplementedException();
        }

        public Bag getBag(Resource r)
        {
            throw new NotImplementedException();
        }

        public Bag getBag(string str)
        {
            throw new NotImplementedException();
        }

        public Property getProperty(string str)
        {
            throw new NotImplementedException();
        }

        public RDFNode getRDFNode(com.hp.hpl.jena.graph.Node n)
        {
            throw new NotImplementedException();
        }

        public Resource getResource(string str, ResourceF rf)
        {
            throw new NotImplementedException();
        }

        public Seq getSeq(Resource r)
        {
            throw new NotImplementedException();
        }

        public Seq getSeq(string str)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listLiteralStatements(Resource r, Property p, double d)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listLiteralStatements(Resource r, Property p, float f)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listLiteralStatements(Resource r, Property p, long l)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listLiteralStatements(Resource r, Property p, char ch)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listLiteralStatements(Resource r, Property p, bool b)
        {
            throw new NotImplementedException();
        }

        public ResIterator listResourcesWithProperty(Property p, object obj)
        {
            throw new NotImplementedException();
        }

        public ResIterator listResourcesWithProperty(Property p, double d)
        {
            throw new NotImplementedException();
        }

        public ResIterator listResourcesWithProperty(Property p, float f)
        {
            throw new NotImplementedException();
        }

        public ResIterator listResourcesWithProperty(Property p, char ch)
        {
            throw new NotImplementedException();
        }

        public ResIterator listResourcesWithProperty(Property p, long l)
        {
            throw new NotImplementedException();
        }

        public ResIterator listResourcesWithProperty(Property p, bool b)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listStatements(Resource r, Property p, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public StmtIterator listStatements(Resource r, Property p, string str)
        {
            throw new NotImplementedException();
        }

        public ResIterator listSubjectsWithProperty(Property p, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public ResIterator listSubjectsWithProperty(Property p, string str)
        {
            throw new NotImplementedException();
        }

        public Model remove(Model m, bool b)
        {
            throw new NotImplementedException();
        }

        public Model remove(Model m)
        {
            throw new NotImplementedException();
        }

        public Model remove(StmtIterator si)
        {
            throw new NotImplementedException();
        }

        public Model remove(Resource r, Property p, RDFNode rdfn)
        {
            throw new NotImplementedException();
        }

        public RDFNode asRDFNode(com.hp.hpl.jena.graph.Node n)
        {
            throw new NotImplementedException();
        }

        public Statement asStatement(com.hp.hpl.jena.graph.Triple t)
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.graph.Graph getGraph()
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.graph.query.QueryHandler queryHandler()
        {
            throw new NotImplementedException();
        }

        public RDFReader getReader(string str)
        {
            throw new NotImplementedException();
        }

        public RDFReader getReader()
        {
            throw new NotImplementedException();
        }

        public string setReaderClassName(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public RDFWriter getWriter()
        {
            throw new NotImplementedException();
        }

        public RDFWriter getWriter(string str)
        {
            throw new NotImplementedException();
        }

        public string setWriterClassName(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public string expandPrefix(string str)
        {
            throw new NotImplementedException();
        }

        public java.util.Map getNsPrefixMap()
        {
            throw new NotImplementedException();
        }

        public string getNsPrefixURI(string str)
        {
            throw new NotImplementedException();
        }

        public string getNsURIPrefix(string str)
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.shared.PrefixMapping @lock()
        {
            throw new NotImplementedException();
        }

        public string qnameFor(string str)
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.shared.PrefixMapping removeNsPrefix(string str)
        {
            throw new NotImplementedException();
        }

        public bool samePrefixMappingAs(com.hp.hpl.jena.shared.PrefixMapping pm)
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.shared.PrefixMapping setNsPrefix(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.shared.PrefixMapping setNsPrefixes(java.util.Map m)
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.shared.PrefixMapping setNsPrefixes(com.hp.hpl.jena.shared.PrefixMapping pm)
        {
            throw new NotImplementedException();
        }

        public string shortForm(string str)
        {
            throw new NotImplementedException();
        }

        public com.hp.hpl.jena.shared.PrefixMapping withDefaultMappings(com.hp.hpl.jena.shared.PrefixMapping pm)
        {
            throw new NotImplementedException();
        }

        public void enterCriticalSection(bool b)
        {
            throw new NotImplementedException();
        }

        public void leaveCriticalSection()
        {
            throw new NotImplementedException();
        }
    }
}
