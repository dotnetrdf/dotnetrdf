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
    public class StringNode
        : LiteralNode, IValuedNode
    {
        public StringNode(IGraph g, String value, Uri datatype)
            : base(g, value, datatype) { }

        public StringNode(IGraph g, String value, String lang)
            : base(g, value, lang) { }

        public StringNode(IGraph g, String value)
            : base(g, value) { }

        #region IValuedNode Members

        public string AsString()
        {
            return this.Value;
        }

        public long AsInteger()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public float AsFloat()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public double AsDouble()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public bool AsBoolean()
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this);
        }

        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public SparqlNumericType NumericType
        {
            get 
            {
                return SparqlNumericType.NaN;
            }
        }

        #endregion
    }
}
