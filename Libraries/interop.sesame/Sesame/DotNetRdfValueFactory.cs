using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;

namespace VDS.RDF.Interop.Sesame
{
    public class DotNetRdfValueFactory : dotSesame.ValueFactory
    {
        private IGraph _g;

        public DotNetRdfValueFactory(IGraph g)
        {
            this._g = g;
        }

        internal IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        #region ValueFactory Members

        public dotSesame.BNode createBNode()
        {
            throw new NotImplementedException();
        }

        public dotSesame.BNode createBNode(string str)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(javax.xml.datatype.XMLGregorianCalendar xmlgc)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(double d)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(float f)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(long l)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(int i)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(short s)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(byte b)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(bool b)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(string str)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(string str, dotSesame.URI uri)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Literal createLiteral(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Statement createStatement(dotSesame.Resource r1, dotSesame.URI uri, dotSesame.Value v, dotSesame.Resource r2)
        {
            throw new NotImplementedException();
        }

        public dotSesame.Statement createStatement(dotSesame.Resource r, dotSesame.URI uri, dotSesame.Value v)
        {
            throw new NotImplementedException();
        }

        public dotSesame.URI createURI(string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public dotSesame.URI createURI(string str)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
