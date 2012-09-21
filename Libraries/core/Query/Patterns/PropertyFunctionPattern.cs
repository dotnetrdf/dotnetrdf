/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing property function patterns in SPARQL Query
    /// </summary>
    public class PropertyFunctionPattern
        : BaseTriplePattern, IPropertyFunctionPattern, IComparable<PropertyFunctionPattern>
    {
        private List<ITriplePattern> _patterns;
        private List<PatternItem> _lhsArgs, _rhsArgs;
        private ISparqlPropertyFunction _function;

        /// <summary>
        /// Creates a new Property Function pattern
        /// </summary>
        /// <param name="info">Function information</param>
        /// <param name="propertyFunction">Property Function</param>
        public PropertyFunctionPattern(PropertyFunctionInfo info, ISparqlPropertyFunction propertyFunction)
            : this(info.Patterns.OfType<ITriplePattern>(), info.SubjectArgs, info.ObjectArgs, propertyFunction) { }

        /// <summary>
        /// Creates a new Property Function pattern
        /// </summary>
        /// <param name="origPatterns">Original Triple Patterns</param>
        /// <param name="lhsArgs">Subject Arguments</param>
        /// <param name="rhsArgs">Object Arguments</param>
        /// <param name="propertyFunction">Property Function</param>
        public PropertyFunctionPattern(IEnumerable<ITriplePattern> origPatterns, IEnumerable<PatternItem> lhsArgs, IEnumerable<PatternItem> rhsArgs, ISparqlPropertyFunction propertyFunction)
        {
            this._patterns = origPatterns.ToList();
            this._lhsArgs = lhsArgs.ToList();
            this._rhsArgs = rhsArgs.ToList();
            this._function = propertyFunction;

            foreach (PatternItem item in lhsArgs.Concat(rhsArgs))
            {
                if (item.VariableName != null && !this._vars.Contains(item.VariableName)) this._vars.Add(item.VariableName);
            }
        }

        /// <summary>
        /// Gets the Pattern Type
        /// </summary>
        public override TriplePatternType PatternType
        {
            get
            {
                return TriplePatternType.PropertyFunction;
            }
        }

        /// <summary>
        /// Gets the Subject arguments
        /// </summary>
        public IEnumerable<PatternItem> SubjectArgs
        {
            get
            {
                return this._lhsArgs;
            }
        }

        /// <summary>
        /// Gets the Object arguments
        /// </summary>
        public IEnumerable<PatternItem> ObjectArgs
        {
            get
            {
                return this._rhsArgs;
            }
        }

        /// <summary>
        /// Gets the original triple patterns
        /// </summary>
        public IEnumerable<ITriplePattern> OriginalPatterns
        {
            get
            {
                return this._patterns;
            }
        }

        /// <summary>
        /// Gets the property function
        /// </summary>
        public ISparqlPropertyFunction PropertyFunction
        {
            get
            {
                return this._function;
            }
        }

        /// <summary>
        /// Evaluates the property function
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            context.OutputMultiset = this._function.Evaluate(context);
        }

        /// <summary>
        /// Returns false because property functions are not accept-alls
        /// </summary>
        public override bool IsAcceptAll
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if none of the 
        /// </summary>
        public override bool HasNoBlankVariables
        {
            get 
            {
                return !this._vars.Any(v => v.StartsWith("_:"));
            }
        }

        /// <summary>
        /// Compares a property function pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(PropertyFunctionPattern other)
        {
            return this.CompareTo((IPropertyFunctionPattern)other);
        }

        /// <summary>
        /// Compares a property function pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(IPropertyFunctionPattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Gets the string representation of the pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._lhsArgs.Count > 1)
            {
                output.Append("( ");
                foreach (PatternItem arg in this._lhsArgs)
                {
                    output.Append(arg.ToString());
                    output.Append(' ');
                }
                output.Append(')');
            }
            else
            {
                output.Append(this._lhsArgs.First().ToString());
            }
            output.Append(" <");
            output.Append(this._function.FunctionUri);
            output.Append("> ");
            if (this._rhsArgs.Count > 1)
            {
                output.Append("( ");
                foreach (PatternItem arg in this._rhsArgs)
                {
                    output.Append(arg.ToString());
                    output.Append(' ');
                }
                output.Append(')');
            }
            else
            {
                output.Append(this._rhsArgs.First().ToString());
            }
            output.Append(" .");
            return output.ToString();
        }
    }
}
