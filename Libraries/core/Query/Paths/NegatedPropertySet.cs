using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Paths
{
    public class NegatedPropertySet : ISparqlPath
    {
        private List<Property> _properties = new List<Property>();
        private List<Property> _inverseProperties = new List<Property>();

        public NegatedPropertySet(IEnumerable<Property> properties, IEnumerable<Property> inverseProperties)
        {
            this._properties.AddRange(properties);
            this._inverseProperties.AddRange(inverseProperties);
        }

        #region ISparqlPath Members

        public bool IsSimple
        {
            get { throw new NotImplementedException(); }
        }

        public bool AllowsZeroLength
        {
            get { throw new NotImplementedException(); }
        }

        public void Evaluate(PathEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public void ToAlgebra(PathTransformContext context)
        {
            throw new NotImplementedException();
        }

        public ISparqlAlgebra ToAlgebraOperator(PathTransformContext context)
        {
            if (this._properties.Count > 0 && this._inverseProperties.Count == 0)
            {
                return new Algebra.NegatedPropertySet(context.Subject, context.Object, this._properties);
            }
            else if (this._properties.Count == 0 && this._inverseProperties.Count > 0)
            {
                throw new NotSupportedException("Translating Negated Property Sets composed of inverses to SPARQL Algebra is not yet supported");
            }
            else
            {
                PathTransformContext lhsContext = new PathTransformContext(context);
                PathTransformContext rhsContext = new PathTransformContext(context);
                lhsContext.AddTriplePattern(new PropertyPathPattern(lhsContext.Subject, new NegatedPropertySet(this._properties, Enumerable.Empty<Property>()), lhsContext.Object));
                rhsContext.AddTriplePattern(new PropertyPathPattern(rhsContext.Subject, new NegatedPropertySet(Enumerable.Empty<Property>(), this._inverseProperties), rhsContext.Object));
                ISparqlAlgebra lhs = lhsContext.ToAlgebra();
                ISparqlAlgebra rhs = rhsContext.ToAlgebra();
                return new Union(lhs, rhs);
            }
        }

        #endregion

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
