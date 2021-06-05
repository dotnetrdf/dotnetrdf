/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns
{

    /// <summary>
    /// Class for representing Triple Patterns in SPARQL Queries.
    /// </summary>
    public class TriplePattern
        : BaseTriplePattern, IMatchTriplePattern, IConstructTriplePattern, IComparable<TriplePattern>
    {
        private readonly TripleIndexType _indexType = TripleIndexType.None;
        private readonly PatternItem _subj, _pred, _obj;

        /// <summary>
        /// Creates a new Triple Pattern.
        /// </summary>
        /// <param name="subj">Subject Pattern.</param>
        /// <param name="pred">Predicate Pattern.</param>
        /// <param name="obj">Object Pattern.</param>
        public TriplePattern(PatternItem subj, PatternItem pred, PatternItem obj)
        {
            _subj = subj;
            if (pred is BlankNodePattern)
            {
                throw new RdfParseException("Cannot use a Triple Pattern with a Blank Node Predicate in a SPARQL Query");
            }
            _pred = pred;
            _obj = obj;

            // Decide on the Index Type
            if (_subj is NodeMatchPattern)
            {
                if (_pred is NodeMatchPattern)
                {
                    _indexType = TripleIndexType.SubjectPredicate;
                }
                else if (_obj is NodeMatchPattern)
                {
                    _indexType = TripleIndexType.SubjectObject;
                }
                else
                {
                    _indexType = TripleIndexType.Subject;
                }
            }
            else if (_pred is NodeMatchPattern)
            {
                if (_obj is NodeMatchPattern)
                {
                    _indexType = TripleIndexType.PredicateObject;
                }
                else
                {
                    _indexType = TripleIndexType.Predicate;
                }
            }
            else if (_obj is NodeMatchPattern)
            {
                _indexType = TripleIndexType.Object;
            }

            // Determine variables used
            if (_subj.VariableName != null) _vars.Add(_subj.VariableName);
            if (_pred.VariableName != null)
            {
                if (!_vars.Contains(_pred.VariableName))
                {
                    _vars.Add(_pred.VariableName);
                }
                else
                {
                    _pred.Repeated = true;
                }
            }
            if (_obj.VariableName != null)
            {
                if (!_vars.Contains(_obj.VariableName))
                {
                    _vars.Add(_obj.VariableName);
                }
                else
                {
                    Object.Repeated = true;
                }
            }
            _vars.Sort();
            if (_vars.Count == 0) _indexType = TripleIndexType.NoVariables;
        }

        /// <summary>
        /// Gets whether a given Triple is accepted by this Pattern in the given Context.
        /// </summary>
        /// <param name="context">Evaluation Context.</param>
        /// <param name="obj">Triple to test.</param>
        /// <returns></returns>
        public bool Accepts(IPatternEvaluationContext context, Triple obj)
        {
            if (!_pred.Repeated && !_obj.Repeated)
            {
                return (_subj.Accepts(context, obj.Subject) && _pred.Accepts(context, obj.Predicate) && _obj.Accepts(context, obj.Object));
            }
            else if (_pred.Repeated && !_obj.Repeated)
            {
                return (_subj.Accepts(context, obj.Subject) && obj.Subject.Equals(obj.Predicate) && _obj.Accepts(context, obj.Object));
            }
            else if (!_pred.Repeated && _obj.Repeated)
            {
                return (_subj.Accepts(context, obj.Subject) && _pred.Accepts(context, obj.Predicate) && obj.Subject.Equals(obj.Object));
            }
            else
            {
                return (_subj.Accepts(context, obj.Subject) && obj.Subject.Equals(obj.Predicate) && obj.Subject.Equals(obj.Object));
            }
        }

        /// <summary>
        /// Gets the pattern type.
        /// </summary>
        public override TriplePatternType PatternType
        {
            get 
            {
                return TriplePatternType.Match;
            }
        }

        /// <summary>
        /// Gets the Index Type we will use for this Pattern.
        /// </summary>
        public TripleIndexType IndexType
        {
            get
            {
                return _indexType;
            }
        }

        /// <summary>
        /// Subject Pattern.
        /// </summary>
        public PatternItem Subject
        {
            get
            {
                return _subj;
            }
        }

        /// <summary>
        /// Predicate Pattern.
        /// </summary>
        public PatternItem Predicate
        {
            get
            {
                return _pred;
            }
        }

        /// <summary>
        /// Object Pattern.
        /// </summary>
        public PatternItem Object
        {
            get
            {
                return _obj;
            }
        }

        /// <summary>
        /// Returns all variables mentioned as a match guarantees all variables are bound.
        /// </summary>
        public override IEnumerable<string> FixedVariables
        {
            get { return Variables; }
        }

        /// <summary>
        /// Returns an empty enumeration as a match guarantees all variables are bound.
        /// </summary>
        public override IEnumerable<string> FloatingVariables { get { return Enumerable.Empty<string>(); } }

        /// <summary>
        /// Returns whether the Triple Pattern is an accept all.
        /// </summary>
        /// <remarks>
        /// True if all three Pattern Items are <see cref="VariablePattern">VariablePattern</see> and all the Variables names are distinct.
        /// </remarks>
        public override bool IsAcceptAll
        {
            get
            {
                return (_subj is VariablePattern && _pred is VariablePattern && _obj is VariablePattern)  &&
                    (_subj.VariableName != _pred.VariableName && _pred.VariableName != _obj.VariableName && _subj.VariableName != _obj.VariableName);
            }

        }

        /// <summary>
        /// Generates a Result Set for a Triple that matches the Pattern.
        /// </summary>
        /// <param name="t">Triple.</param>
        /// <returns></returns>
        public ISet CreateResult(Triple t)
        {
            var s = new Set();
            if (_subj.VariableName != null)
            {
                s.Add(_subj.VariableName, t.Subject);
            }
            if (_pred.VariableName != null && !_pred.Repeated)
            {
                s.Add(_pred.VariableName, t.Predicate);
            }
            if (_obj.VariableName != null && !_obj.Repeated)
            {
                s.Add(_obj.VariableName, t.Object);
            }
            return s;
        }

        /// <summary>
        /// Constructs a Triple from a Set based on this Triple Pattern.
        /// </summary>
        /// <param name="context">Construct Context.</param>
        /// <returns></returns>
        public Triple Construct(ConstructContext context)
        {
            return new Triple(_subj.Construct(context), _pred.Construct(context), _obj.Construct(context));
        }

        /// <summary>
        /// Gets whether the Pattern contains no Variables of any kind.
        /// </summary>
        public bool HasNoVariables
        {
            get
            {
                return (_indexType == TripleIndexType.NoVariables);
            }
        }

        /// <summary>
        /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored).
        /// </summary>
        public bool HasNoExplicitVariables
        {
            get
            {
                return (_subj is NodeMatchPattern || _subj is BlankNodePattern || _subj is FixedBlankNodePattern) &&
                       (_pred is NodeMatchPattern || _pred is BlankNodePattern || _pred is FixedBlankNodePattern) &&
                       (_obj is NodeMatchPattern || _obj is BlankNodePattern || _obj is FixedBlankNodePattern);
            }
        }

        /// <summary>
        /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored).
        /// </summary>
        public override bool HasNoBlankVariables
        {
            get
            {
                return (_subj is NodeMatchPattern || _subj is VariablePattern || _subj is FixedBlankNodePattern) &&
                       (_pred is NodeMatchPattern || _pred is VariablePattern || _pred is FixedBlankNodePattern) &&
                       (_obj is NodeMatchPattern || _obj is VariablePattern || _obj is FixedBlankNodePattern);
            }
        }

        /// <summary>
        /// Compares a triple pattern to another.
        /// </summary>
        /// <param name="other">Pattern.</param>
        /// <returns></returns>
        public int CompareTo(TriplePattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Compares a triple pattern to another.
        /// </summary>
        /// <param name="other">Pattern.</param>
        /// <returns></returns>
        public int CompareTo(IMatchTriplePattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Gets the String representation of this Pattern.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _subj.ToString() + " " + _pred.ToString() + " " + _obj.ToString();
        }
    }
}