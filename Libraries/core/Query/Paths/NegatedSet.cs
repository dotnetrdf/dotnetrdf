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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents a Negated Property Set
    /// </summary>
    public class NegatedSet : ISparqlPath
    {
        private List<Property> _properties = new List<Property>();
        private List<Property> _inverseProperties = new List<Property>();

        /// <summary>
        /// Creates a new Negated Property Set
        /// </summary>
        /// <param name="properties">Negated Properties</param>
        /// <param name="inverseProperties">Inverse Negated Properties</param>
        public NegatedSet(IEnumerable<Property> properties, IEnumerable<Property> inverseProperties)
        {
            this._properties.AddRange(properties);
            this._inverseProperties.AddRange(inverseProperties);
        }

        /// <summary>
        /// Gets the Negated Properties
        /// </summary>
        public IEnumerable<Property> Properties
        {
            get
            {
                return this._properties;
            }
        }

        /// <summary>
        /// Gets the Inverse Negated Properties
        /// </summary>
        public IEnumerable<Property> InverseProperties
        {
            get
            {
                return this._inverseProperties;
            }
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            if (this._properties.Count > 0 && this._inverseProperties.Count == 0)
            {
                return new NegatedPropertySet(context.Subject, context.Object, this._properties);
            }
            else if (this._properties.Count == 0 && this._inverseProperties.Count > 0)
            {
                return new NegatedPropertySet(context.Object, context.Subject, this._inverseProperties, true);
            }
            else
            {
                PathTransformContext lhsContext = new PathTransformContext(context);
                PathTransformContext rhsContext = new PathTransformContext(context);
                lhsContext.AddTriplePattern(new PropertyPathPattern(lhsContext.Subject, new NegatedSet(this._properties, Enumerable.Empty<Property>()), lhsContext.Object));
                rhsContext.AddTriplePattern(new PropertyPathPattern(rhsContext.Subject, new NegatedSet(Enumerable.Empty<Property>(), this._inverseProperties), rhsContext.Object));
                ISparqlAlgebra lhs = lhsContext.ToAlgebra();
                ISparqlAlgebra rhs = rhsContext.ToAlgebra();
                return new Union(lhs, rhs);
            }
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('!');
            if (this._properties.Count + this._inverseProperties.Count > 1) output.Append('(');

            for (int i = 0; i < this._properties.Count; i++)
            {
                output.Append(this._properties[i].ToString());
                if (i < this._properties.Count - 1 || this._inverseProperties.Count > 0)
                {
                    output.Append(" | ");
                }
            }
            for (int i = 0; i < this._inverseProperties.Count; i++)
            {
                output.Append(this._inverseProperties[i].ToString());
                if (i < this._inverseProperties.Count - 1)
                {
                    output.Append(" | ");
                }
            }

            if (this._properties.Count + this._inverseProperties.Count > 1) output.Append(')');

            return output.ToString();
        }
    }
}
