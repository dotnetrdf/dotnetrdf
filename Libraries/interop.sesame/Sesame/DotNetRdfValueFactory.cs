using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;

namespace VDS.RDF.Interop.Sesame
{
    public class DotNetRdfValueFactory : dotSesame.ValueFactory
    {
        private SesameMapping _mapping;

        public DotNetRdfValueFactory()
            : this(new Graph()) { }

        public DotNetRdfValueFactory(IGraph g)
            : this(g, new dotSesame.impl.GraphImpl()) { }

        public DotNetRdfValueFactory(IGraph g, dotSesame.Graph target)
            : this(new SesameMapping(g, target)) { }

        public DotNetRdfValueFactory(SesameMapping mapping)
        {
            this._mapping = mapping;
        }

        internal SesameMapping Mapping
        {
            get
            {
                return this._mapping;
            }
        }

        internal IGraph Graph
        {
            get
            {
                return this._mapping.Graph;
            }
        }

        internal dotSesame.Graph Target
        {
            get
            {
                return this._mapping.Target;
            }
        }

        #region ValueFactory Members

        public dotSesame.BNode createBNode()
        {
            dotSesame.BNode bnode = this._mapping.ValueFactory.createBNode();
            if (!this._mapping.InputMapping.ContainsKey(bnode)) this._mapping.InputMapping.Add(bnode, this._mapping.Graph.CreateBlankNode());
            if (!this._mapping.OutputMapping.ContainsKey(this._mapping.InputMapping[bnode])) this._mapping.OutputMapping.Add(this._mapping.InputMapping[bnode], bnode);
            return bnode;
        }

        public dotSesame.BNode createBNode(string str)
        {
            dotSesame.BNode bnode = this._mapping.ValueFactory.createBNode(str);
            if (!this._mapping.InputMapping.ContainsKey(bnode)) this._mapping.InputMapping.Add(bnode, this._mapping.Graph.CreateBlankNode());
            if (!this._mapping.OutputMapping.ContainsKey(this._mapping.InputMapping[bnode])) this._mapping.OutputMapping.Add(this._mapping.InputMapping[bnode], bnode);
            return bnode;
        }

        public dotSesame.Literal createLiteral(javax.xml.datatype.XMLGregorianCalendar xmlgc)
        {
            return this._mapping.ValueFactory.createLiteral(xmlgc);
        }

        public dotSesame.Literal createLiteral(double d)
        {
            return this._mapping.ValueFactory.createLiteral(d);
        }

        public dotSesame.Literal createLiteral(float f)
        {
            return this._mapping.ValueFactory.createLiteral(f);
        }

        public dotSesame.Literal createLiteral(long l)
        {
            return this._mapping.ValueFactory.createLiteral(l);
        }

        public dotSesame.Literal createLiteral(int i)
        {
            return this._mapping.ValueFactory.createLiteral(i);
        }

        public dotSesame.Literal createLiteral(short s)
        {
            return this._mapping.ValueFactory.createLiteral(s);
        }

        public dotSesame.Literal createLiteral(byte b)
        {
            return this._mapping.ValueFactory.createLiteral(b);
        }

        public dotSesame.Literal createLiteral(bool b)
        {
            return this._mapping.ValueFactory.createLiteral(b);
        }

        public dotSesame.Literal createLiteral(string str)
        {
            return this._mapping.ValueFactory.createLiteral(str);
        }

        public dotSesame.Literal createLiteral(string str, dotSesame.URI uri)
        {
            return this._mapping.ValueFactory.createLiteral(str, uri);
        }

        public dotSesame.Literal createLiteral(string str1, string str2)
        {
            return this._mapping.ValueFactory.createLiteral(str1, str2);
        }

        public dotSesame.Statement createStatement(dotSesame.Resource r1, dotSesame.URI uri, dotSesame.Value v, dotSesame.Resource r2)
        {
            return this._mapping.ValueFactory.createStatement(r1, uri, v, r2);
        }

        public dotSesame.Statement createStatement(dotSesame.Resource r, dotSesame.URI uri, dotSesame.Value v)
        {
            return this._mapping.ValueFactory.createStatement(r, uri, v);
        }

        public dotSesame.URI createURI(string str1, string str2)
        {
            return this._mapping.ValueFactory.createURI(str1, str2);
        }

        public dotSesame.URI createURI(string str)
        {
            return this._mapping.ValueFactory.createURI(str);
        }

        #endregion
    }
}
