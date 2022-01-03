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
        /// <summary>
        /// Creates a new Triple Pattern.
        /// </summary>
        /// <param name="subj">Subject Pattern.</param>
        /// <param name="pred">Predicate Pattern.</param>
        /// <param name="obj">Object Pattern.</param>
        public TriplePattern(PatternItem subj, PatternItem pred, PatternItem obj)
        {
            Subject = subj;
            switch (pred)
            {
                case BlankNodePattern:
                    throw new RdfParseException("Cannot use a Triple Pattern with a Blank Node Predicate in a SPARQL Query");
                case QuotedTriplePattern:
                    throw new RdfParseException(
                        "Cannot use a Triple Pattern with a Triple Node predicate in a SPARQL Query");
            }

            Predicate = pred;
            Object = obj;

            // Decide on the Index Type
            if (Subject is NodeMatchPattern or QuotedTriplePattern)
            {
                if (Predicate is NodeMatchPattern or QuotedTriplePattern)
                {
                    IndexType = TripleIndexType.SubjectPredicate;
                }
                else if (Object is NodeMatchPattern or QuotedTriplePattern)
                {
                    IndexType = TripleIndexType.SubjectObject;
                }
                else
                {
                    IndexType = TripleIndexType.Subject;
                }
            }
            else if (Predicate is NodeMatchPattern or QuotedTriplePattern)
            {
                if (Object is NodeMatchPattern or QuotedTriplePattern)
                {
                    IndexType = TripleIndexType.PredicateObject;
                }
                else
                {
                    IndexType = TripleIndexType.Predicate;
                }
            }
            else if (Object is NodeMatchPattern or QuotedTriplePattern)
            {
                IndexType = TripleIndexType.Object;
            }

            // Determine variables used
            if (Subject.VariableName != null) _vars.Add(Subject.VariableName);
            if (Predicate.VariableName != null)
            {
                if (!_vars.Contains(Predicate.VariableName))
                {
                    _vars.Add(Predicate.VariableName);
                }
                else
                {
                    Predicate.Repeated = true;
                }
            }
            if (Object.VariableName != null)
            {
                if (!_vars.Contains(Object.VariableName))
                {
                    _vars.Add(Object.VariableName);
                }
                else
                {
                    Object.Repeated = true;
                }
            }
            _vars.Sort();
            if (_vars.Count == 0) IndexType = TripleIndexType.NoVariables;
        }

        /// <summary>
        /// Gets whether a given Triple is accepted by this Pattern in the given Context.
        /// </summary>
        /// <param name="context">Evaluation Context.</param>
        /// <param name="obj">Triple to test.</param>
        /// <returns></returns>
        public bool Accepts(IPatternEvaluationContext context, Triple obj)
        {
            if (!Predicate.Repeated && !Object.Repeated)
            {
                return (Subject.Accepts(context, obj.Subject) && Predicate.Accepts(context, obj.Predicate) && Object.Accepts(context, obj.Object));
            }
            else if (Predicate.Repeated && !Object.Repeated)
            {
                return (Subject.Accepts(context, obj.Subject) && obj.Subject.Equals(obj.Predicate) && Object.Accepts(context, obj.Object));
            }
            else if (!Predicate.Repeated && Object.Repeated)
            {
                return (Subject.Accepts(context, obj.Subject) && Predicate.Accepts(context, obj.Predicate) && obj.Subject.Equals(obj.Object));
            }
            else
            {
                return (Subject.Accepts(context, obj.Subject) && obj.Subject.Equals(obj.Predicate) && obj.Subject.Equals(obj.Object));
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
        public TripleIndexType IndexType { get; } = TripleIndexType.None;

        /// <summary>
        /// Subject Pattern.
        /// </summary>
        public PatternItem Subject { get; }

        /// <summary>
        /// Predicate Pattern.
        /// </summary>
        public PatternItem Predicate { get; }

        /// <summary>
        /// Object Pattern.
        /// </summary>
        public PatternItem Object { get; }

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

        public override TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
        {
            return processor.ProcessTriplePattern(this, context);
        }

        public override T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
        {
            return visitor.VisitTriplePattern(this);
        }

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
                return (Subject is VariablePattern && Predicate is VariablePattern && Object is VariablePattern)  &&
                    (Subject.VariableName != Predicate.VariableName && Predicate.VariableName != Object.VariableName && Subject.VariableName != Object.VariableName);
            }

        }

        /// <summary>
        /// Generates a Result Set for a Triple that matches the Pattern.
        /// </summary>
        /// <param name="t">Triple.</param>
        /// <returns></returns>
        public ISet CreateResult(Triple t)
        {
            ISet s = new Set();
            if (Subject.VariableName != null)
            {
                s.Add(Subject.VariableName, t.Subject);
            }
            else if (Subject is QuotedTriplePattern qtp)
            {
                s = qtp.CreateResults(t.Subject)?.Join(s) ?? s;
            }
            if (Predicate.VariableName != null && !Predicate.Repeated)
            {
                s.Add(Predicate.VariableName, t.Predicate);
            }
            if (Object.VariableName != null && !Object.Repeated)
            {
                s.Add(Object.VariableName, t.Object);
            } else if (Object is QuotedTriplePattern qtp)
            {
                s = qtp.CreateResults(t.Object)?.Join(s) ?? s;
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
            return new Triple(Subject.Construct(context), Predicate.Construct(context), Object.Construct(context));
        }

        /// <summary>
        /// Gets whether the Pattern contains no Variables of any kind.
        /// </summary>
        public bool HasNoVariables
        {
            get
            {
                return (IndexType == TripleIndexType.NoVariables);
            }
        }

        /// <summary>
        /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored).
        /// </summary>
        public bool HasNoExplicitVariables
        {
            get
            {
                return Subject is NodeMatchPattern or BlankNodePattern or FixedBlankNodePattern or QuotedTriplePattern { HasNoExplicitVariables: true } &&
                       Predicate is NodeMatchPattern or BlankNodePattern or FixedBlankNodePattern &&
                       Object is NodeMatchPattern or BlankNodePattern or FixedBlankNodePattern or QuotedTriplePattern { HasNoExplicitVariables: true };
            }
        }

        /// <summary>
        /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored).
        /// </summary>
        public override bool HasNoBlankVariables
        {
            get
            {
                return Subject is NodeMatchPattern or VariablePattern or FixedBlankNodePattern or QuotedTriplePattern{HasNoBlankVariables:true} &&
                       Predicate is NodeMatchPattern or VariablePattern or FixedBlankNodePattern &&
                       Object is NodeMatchPattern or VariablePattern or FixedBlankNodePattern or QuotedTriplePattern{HasNoBlankVariables:true};
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
            return Subject.ToString() + " " + Predicate.ToString() + " " + Object.ToString();
        }
    }
}