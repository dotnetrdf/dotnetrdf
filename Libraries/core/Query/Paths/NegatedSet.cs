using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Paths
{
    public class NegatedSet : ISparqlPath
    {
        private List<Property> _properties = new List<Property>();
        private List<Property> _inverseProperties = new List<Property>();

        public NegatedSet(IEnumerable<Property> properties, IEnumerable<Property> inverseProperties)
        {
            this._properties.AddRange(properties);
            this._inverseProperties.AddRange(inverseProperties);
        }

        public IEnumerable<Property> Properties
        {
            get
            {
                return this._properties;
            }
        }

        public IEnumerable<Property> InverseProperties
        {
            get
            {
                return this._inverseProperties;
            }
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
