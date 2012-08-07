using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Patterns
{
    public class PropertyFunctionPattern
        : BaseTriplePattern, IPropertyFunctionPattern, IComparable<PropertyFunctionPattern>
    {
        private List<ITriplePattern> _patterns;
        private List<PatternItem> _lhsArgs, _rhsArgs;
        private ISparqlPropertyFunction _function;

        public PropertyFunctionPattern(IEnumerable<ITriplePattern> origPatterns, IEnumerable<PatternItem> lhsArgs, IEnumerable<PatternItem> rhsArgs, ISparqlPropertyFunction propertyFunction)
        {
            this._patterns = origPatterns.ToList();
            this._lhsArgs = lhsArgs.ToList();
            this._rhsArgs = rhsArgs.ToList();
            this._function = propertyFunction;
        }

        public override TriplePatternType PatternType
        {
            get
            {
                return TriplePatternType.PropertyFunction;
            }
        }

        public IEnumerable<PatternItem> LhsArgs
        {
            get
            {
                return this._lhsArgs;
            }
        }

        public IEnumerable<PatternItem> RhsArgs
        {
            get
            {
                return this._rhsArgs;
            }
        }

        public IEnumerable<ITriplePattern> OriginalPatterns
        {
            get
            {
                return this._patterns;
            }
        }

        public ISparqlPropertyFunction PropertyFunction
        {
            get
            {
                return this._function;
            }
        }

        public override void Evaluate(SparqlEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override bool IsAcceptAll
        {
            get 
            {
                return false;
            }
        }

        public override bool HasNoBlankVariables
        {
            get 
            { 
                throw new NotImplementedException();
            }
        }

        public int CompareTo(PropertyFunctionPattern other)
        {
            return this.CompareTo((IPropertyFunctionPattern)other);
        }

        public int CompareTo(IPropertyFunctionPattern other)
        {
            return base.CompareTo(other);
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
