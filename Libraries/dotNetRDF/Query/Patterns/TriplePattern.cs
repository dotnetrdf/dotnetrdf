/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
    /// Class for representing Triple Patterns in SPARQL Queries
    /// </summary>
    public class TriplePattern
        : BaseTriplePattern, IMatchTriplePattern, IConstructTriplePattern, IComparable<TriplePattern>
    {
        private readonly TripleIndexType _indexType = TripleIndexType.None;
        private readonly PatternItem _subj, _pred, _obj;

        /// <summary>
        /// Creates a new Triple Pattern
        /// </summary>
        /// <param name="subj">Subject Pattern</param>
        /// <param name="pred">Predicate Pattern</param>
        /// <param name="obj">Object Pattern</param>
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
                if (Options.FullTripleIndexing)
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
                else
                {
                    _indexType = TripleIndexType.Subject;
                }
            }
            else if (_pred is NodeMatchPattern)
            {
                if (Options.FullTripleIndexing)
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
        /// Gets whether a given Triple is accepted by this Pattern in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(SparqlEvaluationContext context, Triple obj)
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
        /// Gets the pattern type
        /// </summary>
        public override TriplePatternType PatternType
        {
            get 
            {
                return TriplePatternType.Match;
            }
        }

        /// <summary>
        /// Gets the Index Type we will use for this Pattern
        /// </summary>
        public TripleIndexType IndexType
        {
            get
            {
                return _indexType;
            }
        }

        /// <summary>
        /// Subject Pattern
        /// </summary>
        public PatternItem Subject
        {
            get
            {
                return _subj;
            }
        }

        /// <summary>
        /// Predicate Pattern
        /// </summary>
        public PatternItem Predicate
        {
            get
            {
                return _pred;
            }
        }

        /// <summary>
        /// Object Pattern
        /// </summary>
        public PatternItem Object
        {
            get
            {
                return _obj;
            }
        }

        /// <summary>
        /// Returns all variables mentioned as a match guarantees all variables are bound
        /// </summary>
        public override IEnumerable<string> FixedVariables
        {
            get { return Variables; }
        }

        /// <summary>
        /// Returns an empty enumeration as a match guarantees all variables are bound
        /// </summary>
        public override IEnumerable<string> FloatingVariables { get { return Enumerable.Empty<String>(); } }

        /// <summary>
        /// Returns whether the Triple Pattern is an accept all
        /// </summary>
        /// <remarks>
        /// True if all three Pattern Items are <see cref="VariablePattern">VariablePattern</see> and all the Variables names are distinct
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
        /// Evaluates a Triple Pattern in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            if (_indexType == TripleIndexType.NoVariables)
            {
                // If there are no variables then at least one Triple must match or we abort
                INode s = ((NodeMatchPattern)_subj).Node;
                INode p = ((NodeMatchPattern)_pred).Node;
                INode o = ((NodeMatchPattern)_obj).Node;
                if (context.Data.ContainsTriple(new Triple(s, p, o)))
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
                else
                {
                    context.OutputMultiset = new NullMultiset();
                }
            }
            else
            {
                FindResults(context, GetTriples(context));
            }
        }

        /// <summary>
        /// Gets the Enumeration of Triples that should be assessed for matching the pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(SparqlEvaluationContext context)
        {
            INode subj, pred, obj;

            // Stuff for more precise indexing
            IEnumerable<INode> values = null;
            IEnumerable<ISet> valuePairs = null;
            String subjVar = _subj.VariableName;
            String predVar = _pred.VariableName;
            String objVar = _obj.VariableName;
            bool boundSubj = (subjVar != null && context.InputMultiset.ContainsVariable(subjVar));
            bool boundPred = (predVar != null && context.InputMultiset.ContainsVariable(predVar));
            bool boundObj = (objVar != null && context.InputMultiset.ContainsVariable(objVar));

            switch (_indexType)
            {
                case TripleIndexType.Subject:
                    subj = ((NodeMatchPattern)_subj).Node;
                    if (boundPred)
                    {
                        if (boundObj)
                        {
                            valuePairs = (from set in context.InputMultiset.Sets
                                          where set.ContainsVariable(predVar) && set.ContainsVariable(objVar)
                                          select set).Distinct(new SetDistinctnessComparer(new String[] { predVar, objVar }));
                            return (from set in valuePairs
                                    where set[predVar] != null && set[objVar] != null
                                    select CreateTriple(subj, set[predVar], set[objVar])).Where(t => context.Data.ContainsTriple(t));
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(predVar)
                                      select set[predVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithSubjectPredicate(subj, value)
                                    select t);
                        }
                    }
                    else if (boundObj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(objVar)
                                  select set[objVar]).Distinct();
                        return (from value in values
                                where value != null
                                from t in context.Data.GetTriplesWithSubjectObject(subj, value)
                                select t);
                    }
                    else
                    {
                        return context.Data.GetTriplesWithSubject(subj);
                    }

                case TripleIndexType.SubjectPredicate:
                    subj = ((NodeMatchPattern)_subj).Node;
                    pred = ((NodeMatchPattern)_pred).Node;

                    if (boundObj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(objVar)
                                  select set[objVar]).Distinct();
                        return (from value in values
                                where value != null
                                select CreateTriple(subj, pred, value)).Where(t => context.Data.ContainsTriple(t));
                    }
                    else
                    {
                        return context.Data.GetTriplesWithSubjectPredicate(subj, pred);
                    }

                case TripleIndexType.SubjectObject:
                    subj = ((NodeMatchPattern)_subj).Node;
                    obj = ((NodeMatchPattern)_obj).Node;

                    if (boundPred)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(predVar)
                                  select set[predVar]).Distinct();
                        return (from value in values
                                where value != null
                                select CreateTriple(subj, value, obj)).Where(t => context.Data.ContainsTriple(t));
                    }
                    else
                    {
                        return context.Data.GetTriplesWithSubjectObject(subj, obj);
                    }

                case TripleIndexType.Predicate:
                    pred = ((NodeMatchPattern)_pred).Node;
                    if (boundSubj)
                    {
                        if (boundObj)
                        {
                            valuePairs = (from set in context.InputMultiset.Sets
                                          where set.ContainsVariable(subjVar) && set.ContainsVariable(objVar)
                                          select set).Distinct(new SetDistinctnessComparer(new String[] { subjVar, objVar }));
                            return (from set in valuePairs
                                    where set[subjVar] != null && set[objVar] != null
                                    select CreateTriple(set[subjVar], pred, set[objVar])).Where(t => context.Data.ContainsTriple(t));
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(subjVar)
                                      select set[subjVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithSubjectPredicate(value, pred)
                                    select t);
                        }
                    }
                    else if (boundObj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(objVar)
                                  select set[objVar]).Distinct();
                        return (from value in values
                                where value != null
                                from t in context.Data.GetTriplesWithPredicateObject(pred, value)
                                select t);
                    }
                    else
                    {
                        return context.Data.GetTriplesWithPredicate(pred);
                    }

                case TripleIndexType.PredicateObject:
                    pred = ((NodeMatchPattern)_pred).Node;
                    obj = ((NodeMatchPattern)_obj).Node;

                    if (boundSubj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(subjVar)
                                  select set[subjVar]).Distinct();
                        return (from value in values
                                where value != null
                                select CreateTriple(value, pred, obj)).Where(t => context.Data.ContainsTriple(t));
                    }
                    else
                    {
                        return context.Data.GetTriplesWithPredicateObject(pred, obj);
                    }

                case TripleIndexType.Object:
                    obj = ((NodeMatchPattern)_obj).Node;
                    if (boundSubj)
                    {
                        if (boundPred)
                        {
                            valuePairs = (from set in context.InputMultiset.Sets
                                          where set.ContainsVariable(subjVar) && set.ContainsVariable(predVar)
                                          select set).Distinct(new SetDistinctnessComparer(new String[] { subjVar, predVar }));
                            return (from set in valuePairs
                                    where set[subjVar] != null && set[predVar] != null
                                    select CreateTriple(set[subjVar], set[predVar], obj)).Where(t => context.Data.ContainsTriple(t));
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(subjVar)
                                      select set[subjVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithSubjectObject(value, obj)
                                    select t);
                        }
                    }
                    else if (boundPred)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(predVar)
                                  select set[predVar]).Distinct();
                        return (from value in values
                                where value != null
                                from t in context.Data.GetTriplesWithPredicateObject(value, obj)
                                select t);
                    }
                    else
                    {
                        return context.Data.GetTriplesWithObject(obj);
                    }

                case TripleIndexType.NoVariables:
                    // If there are no variables then at least one Triple must match or we abort
                    INode s, p, o;
                    s = ((NodeMatchPattern)_subj).Node;
                    p = ((NodeMatchPattern)_pred).Node;
                    o = ((NodeMatchPattern)_obj).Node;
                    if (context.Data.ContainsTriple(new Triple(s, p, o)))
                    {
                        return new Triple(s, p, o).AsEnumerable();
                    }
                    else
                    {
                        return Enumerable.Empty<Triple>();
                    }

                case TripleIndexType.None:
                    // This means we got a pattern like ?s ?p ?o so we want to use whatever bound variables 
                    // we have to reduce the triples we return as far as possible
                    // TODO: Add handling for all the cases here
                    if (boundSubj)
                    {
                        if (boundPred)
                        {
                            return context.Data.Triples;
                        }
                        else if (boundObj)
                        {
                            return context.Data.Triples;
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(subjVar)
                                      select set[subjVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithSubject(value)
                                    select t);
                        }
                    }
                    else if (boundPred)
                    {
                        if (boundObj)
                        {
                            return context.Data.Triples;
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(predVar)
                                      select set[predVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithPredicate(value)
                                    select t);
                        }
                    }
                    else if (boundObj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(objVar)
                                  select set[objVar]).Distinct();
                        return (from value in values
                                where value != null
                                from t in context.Data.GetTriplesWithObject(value)
                                select t);
                    }
                    else
                    {
                        return context.Data.Triples;
                    }

                default:
                    return context.Data.Triples;
            }
        }

        private Triple CreateTriple(INode s, INode p, INode o)
        {
            IGraph target = s.Graph;
            if (target == null)
            {
                target = p.Graph;
                if (target == null) target = o.Graph;
            }
            return new Triple(s.CopyNode(target), p.CopyNode(target), o.CopyNode(target));
        }

        /// <summary>
        /// Takes an enumerable and extracts Triples which match this pattern as results
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="ts">Enumerable of Triples</param>
        private void FindResults(SparqlEvaluationContext context, IEnumerable<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                if (Accepts(context, t))
                {
                    context.OutputMultiset.Add(CreateResult(t));
                }
            }
        }

        /// <summary>
        /// Generates a Result Set for a Triple that matches the Pattern
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public ISet CreateResult(Triple t)
        {
            Set s = new Set();
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
        /// Constructs a Triple from a Set based on this Triple Pattern
        /// </summary>
        /// <param name="context">Construct Context</param>
        /// <returns></returns>
        public Triple Construct(ConstructContext context)
        {
            return new Triple(Tools.CopyNode(_subj.Construct(context), context.Graph), Tools.CopyNode(_pred.Construct(context), context.Graph), Tools.CopyNode(_obj.Construct(context), context.Graph));
        }

        /// <summary>
        /// Gets whether the Pattern contains no Variables of any kind
        /// </summary>
        public bool HasNoVariables
        {
            get
            {
                return (_indexType == TripleIndexType.NoVariables);
            }
        }

        /// <summary>
        /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored)
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
        /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored)
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
        /// Compares a triple pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(TriplePattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Compares a triple pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(IMatchTriplePattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Gets the String representation of this Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _subj.ToString() + " " + _pred.ToString() + " " + _obj.ToString();
        }
    }
}