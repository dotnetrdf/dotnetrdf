/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes
{
    public class DateTimeNode
        : LiteralNode, IValuedNode
    {
        private DateTimeOffset _value;


        protected DateTimeNode(IGraph g, DateTimeOffset value, String lexicalValue, Uri datatype)
            : base(g, lexicalValue, datatype)
        {
            this._value = value;
        }

        protected DateTimeNode(IGraph g, DateTimeOffset value, Uri datatype)
            : this(g, value, GetStringForm(value, datatype), datatype) { }

        public DateTimeNode(IGraph g, DateTimeOffset value)
            : this(g, value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) { }

        public DateTimeNode(IGraph g, DateTimeOffset value, String lexicalValue)
            : this(g, value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) { }

        private static String GetStringForm(DateTimeOffset value, Uri datatype)
        {
            switch (datatype.ToString())
            {
                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                    return value.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat);
                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                    return value.ToString(XmlSpecsHelper.XmlSchemaDateFormat);
                default:
                    throw new RdfQueryException("Unable to create a DateTimeNode because datatype URI is invalid");
            }
        }

        public string AsString()
        {
            return this.Value;
        }

        public long AsInteger()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public float AsFloat()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public double AsDouble()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public bool AsBoolean()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public DateTimeOffset AsDateTime()
        {
            return this._value;
        }

        public SparqlNumericType NumericType
        {
            get 
            {
                return SparqlNumericType.NaN;
            }
        }
    }

    public class DateNode
        : DateTimeNode
    {
        public DateNode(IGraph g, DateTimeOffset value)
            : base(g, value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        public DateNode(IGraph g, DateTimeOffset value, String lexicalValue)
            : base(g, value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }
    }
}
