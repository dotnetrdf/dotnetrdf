using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Nodes
{
    public class ByteNode
        : NumericNode
    {
        private byte _value;

        public ByteNode(IGraph g, byte value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte), SparqlNumericType.Integer)
        {
            this._value = value;
        }

        public ByteNode(IGraph g, byte value)
            : this(g, value, value.ToString()) { }


        public override long AsInteger()
        {
            try
            {
                return Convert.ToInt64(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to integer");
            }
        }

        public override decimal AsDecimal()
        {
            try
            {
                return Convert.ToDecimal(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to decimal");
            }
        }

        public override float AsFloat()
        {
            try
            {
                return Convert.ToSingle(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to float");
            }
        }

        public override double AsDouble()
        {
            try
            {
                return Convert.ToDouble(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to double");
            }
        }
    }

    public class SignedByteNode
        : NumericNode
    {
        private sbyte _value;

        public SignedByteNode(IGraph g, sbyte value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeByte), SparqlNumericType.Integer)
        {
            this._value = value;
        }

        public SignedByteNode(IGraph g, sbyte value)
            : this(g, value, value.ToString()) { }

        public override long AsInteger()
        {
            try
            {
                return Convert.ToInt64(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to integer");
            }
        }

        public override decimal AsDecimal()
        {
            try
            {
                return Convert.ToDecimal(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to decimal");
            }
        }

        public override float AsFloat()
        {
            try
            {
                return Convert.ToSingle(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to float");
            }
        }

        public override double AsDouble()
        {
            try
            {
                return Convert.ToDouble(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to double");
            }
        }
    }
}
