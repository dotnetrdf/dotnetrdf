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
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// A Valued Node with a numeric value
    /// </summary>
    public abstract class NumericNode
        : LiteralNode, IValuedNode
    {
        private SparqlNumericType _numType = SparqlNumericType.NaN;

        public NumericNode(IGraph g, String value, Uri datatype, SparqlNumericType numType)
            : base(g, value, datatype) 
        {
            this._numType = numType;
        }

        public string AsString()
        {
            return this.Value;
        }

        public abstract long AsInteger();

        public abstract decimal AsDecimal();

        public abstract float AsFloat();

        public abstract double AsDouble();

        public bool AsBoolean()
        {
            switch (this._numType)
            {
                case SparqlNumericType.Integer:
                    return this.AsInteger() != 0;
                case SparqlNumericType.Decimal:
                    return this.AsDecimal() != Decimal.Zero;
                case SparqlNumericType.Float:
                    return this.AsFloat() != 0.0f && this.AsFloat() != Single.NaN;
                case SparqlNumericType.Double:
                    return this.AsDouble() != 0.0d && this.AsDouble() != Double.NaN;
                default:
                    return SparqlSpecsHelper.EffectiveBooleanValue(this);
            }
        }

        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Numeric Types cannot be converted into Date Times");
        }

        public SparqlNumericType NumericType
        {
            get
            {
                return this._numType;
            }
        }
    }
}
