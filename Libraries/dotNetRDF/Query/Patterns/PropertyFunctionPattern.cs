/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
        private readonly List<ITriplePattern> _patterns;
        private readonly List<PatternItem> _lhsArgs, _rhsArgs;
        private readonly ISparqlPropertyFunction _function;

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

            foreach (PatternItem item in this._lhsArgs.Concat(this._rhsArgs))
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
        /// Returns the empty enumerable as cannot guarantee any variables are bound
        /// </summary>
        public override IEnumerable<string> FixedVariables
        {
            get { return Enumerable.Empty<String>(); }
        }

        /// <summary>
        /// Returns all variables mentioned in the property function as we can't guarantee they are bound
        /// </summary>
        public override IEnumerable<string> FloatingVariables { get { return this._vars; } }

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
