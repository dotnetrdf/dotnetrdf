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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// The Weighted Optimiser is a Query Optimiser that orders Triple Patterns based on weighting computed calculated against
    /// </summary>
    public class WeightedOptimiser 
        : BaseQueryOptimiser
    {
        private Weightings _weights;

        /// <summary>
        /// Default Weight for Subject Terms
        /// </summary>
        public const double DefaultSubjectWeight = 0.8d;
        /// <summary>
        /// Default Weight for Predicate Terms
        /// </summary>
        public const double DefaultPredicateWeight = 0.4d;
        /// <summary>
        /// Default Weight for Object Terms
        /// </summary>
        public const double DefaultObjectWeight = 0.6d;
        /// <summary>
        /// Default Weight for Variables
        /// </summary>
        public const double DefaultVariableWeight = 1d;

        /// <summary>
        /// Creates a new Weighted Optimiser
        /// </summary>
        public WeightedOptimiser()
        { }

        /// <summary>
        /// Creates a new Weighted Optimiser which reads weights from the given RDF Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public WeightedOptimiser(IGraph g)
        {
            _weights = new Weightings(g);
        }

        /// <summary>
        /// Creates a new Weighted Optimiser which reads weights from the given RDF Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="subjWeight">Default Subject Weight</param>
        /// <param name="predWeight">Default Predicate Weight</param>
        /// <param name="objWeight">Default Object Weight</param>
        public WeightedOptimiser(IGraph g, double subjWeight, double predWeight, double objWeight)
            : this(g)
        {
            _weights.DefaultSubjectWeighting = subjWeight;
            _weights.DefaultPredicateWeighting = predWeight;
            _weights.DefaultObjectWeighting = objWeight;
        }

        /// <summary>
        /// Gets the comparer used to order the Triple Patterns based on their computed weightings
        /// </summary>
        /// <returns></returns>
        protected override IComparer<ITriplePattern> GetRankingComparer()
        {
            return new WeightingComparer(_weights);
        }
    }

    /// <summary>
    /// Represents Weightings for the <see cref="WeightedOptimiser">WeightedOptimiser</see>
    /// </summary>
    class Weightings
    {
        private Dictionary<INode, long> _subjectWeightings = new Dictionary<INode, long>();
        private Dictionary<INode, long> _predicateWeightings = new Dictionary<INode, long>();
        private Dictionary<INode, long> _objectWeightings = new Dictionary<INode, long>();

        private double _defSubjWeight = WeightedOptimiser.DefaultSubjectWeight;
        private double _defPredWeight = WeightedOptimiser.DefaultPredicateWeight;
        private double _defObjWeight = WeightedOptimiser.DefaultObjectWeight;
        private double _defVarWeight = WeightedOptimiser.DefaultVariableWeight;

        public Weightings(IGraph g)
        {
            if (g == null) return;

            g.NamespaceMap.AddNamespace("opt", UriFactory.Create(SparqlOptimiser.OptimiserStatsNamespace));

            INode optSubjCount = g.CreateUriNode("opt:subjectCount");
            INode optPredCount = g.CreateUriNode("opt:predicateCount");
            INode optObjCount = g.CreateUriNode("opt:objectCount");
            INode optCount = g.CreateUriNode("opt:count");

            foreach (Triple t in g.GetTriplesWithPredicate(optSubjCount))
            {
                SetSubjectCount(t.Subject, t.Object);
            }
            foreach (Triple t in g.GetTriplesWithPredicate(optPredCount))
            {
                SetPredicateCount(t.Subject, t.Object);
            }
            foreach (Triple t in g.GetTriplesWithPredicate(optObjCount))
            {
                SetObjectCount(t.Subject, t.Object);
            }
            foreach (Triple t in g.GetTriplesWithPredicate(optCount))
            {
                SetSubjectCount(t.Subject, t.Object);
                SetPredicateCount(t.Subject, t.Object);
                SetObjectCount(t.Subject, t.Object);
            }
        }

        public void SetSubjectCount(INode n, INode count)
        {
            if (count.NodeType == NodeType.Literal)
            {
                long i;
                if (Int64.TryParse(((ILiteralNode)count).Value, out i))
                {
                    long current;
                    if (_subjectWeightings.TryGetValue(n, out current))
                    {
                        _subjectWeightings[n] = Math.Max(current, i);
                    }
                    else
                    {
                        _subjectWeightings.Add(n, i);
                    }
                }
            }
        }

        public void SetPredicateCount(INode n, INode count)
        {
            if (count.NodeType == NodeType.Literal)
            {
                long i;
                if (Int64.TryParse(((ILiteralNode)count).Value, out i))
                {
                    long current;
                    if (_predicateWeightings.TryGetValue(n, out current))
                    {
                        _predicateWeightings[n] = Math.Max(current, i);
                    }
                    else
                    {
                        _predicateWeightings.Add(n, i);
                    }
                }
            }
        }

        public void SetObjectCount(INode n, INode count)
        {
            if (count.NodeType == NodeType.Literal)
            {
                long i;
                if (Int64.TryParse(((ILiteralNode)count).Value, out i))
                {
                    long current;
                    if (_objectWeightings.TryGetValue(n, out current))
                    {
                        _objectWeightings[n] = Math.Max(current, i);
                    }
                    else
                    {
                        _objectWeightings.Add(n, i);
                    }
                }
            }
        }

        public double SubjectWeighting(INode n)
        {
            long temp = 1;
            if (_subjectWeightings.TryGetValue(n, out temp))
            {
                temp = Math.Max(1, temp);
                return 1d - (1d / temp);
            }
            else
            {
                return 1d - _defSubjWeight;
            }
        }

        public double PredicateWeighting(INode n)
        {
            long temp = 1;
            if (_predicateWeightings.TryGetValue(n, out temp))
            {
                temp = Math.Max(1, temp);
                return 1d - (1d / temp);
            }
            else
            {
                return 1d - _defPredWeight;
            }
        }

        public double ObjectWeighting(INode n)
        {
            long temp = 1;
            if (_objectWeightings.TryGetValue(n, out temp))
            {
                temp = Math.Max(1, temp);
                return 1d - (1d / temp);
            }
            else
            {
                return 1d - _defObjWeight;
            }
        }

        public double DefaultSubjectWeighting
        {
            get
            {
                return _defSubjWeight;
            }
            set
            {
                _defSubjWeight = Math.Min(Math.Max(Double.Epsilon, value), 1d);
            }
        }

        public double DefaultPredicateWeighting
        {
            get
            {
                return _defPredWeight;
            }
            set
            {
                _defPredWeight = Math.Min(Math.Max(Double.Epsilon, value), 1d);
            }
        }

        public double DefaultObjectWeighting
        {
            get
            {
                return _defObjWeight;
            }
            set
            {
                _defObjWeight = Math.Min(Math.Max(Double.Epsilon, value), 1d);
            }
        }

        public double DefaultVariableWeighting
        {
            get
            {
                return _defVarWeight;
            }
        }
    }

    class WeightingComparer
        : IComparer<ITriplePattern>
    {
        private Weightings _weights;

        public WeightingComparer(Weightings weights)
        {
            if (weights == null) throw new ArgumentNullException("weights");
            _weights = weights;
        }

        public int Compare(ITriplePattern x, ITriplePattern y)
        {
            double xSubj, xPred, xObj;
            double ySubj, yPred, yObj;

            GetSelectivities(x, out xSubj, out xPred, out xObj);
            GetSelectivities(y, out ySubj, out yPred, out yObj);

            double xSel = xSubj * xPred * xObj;
            double ySel = ySubj * yPred * yObj;

            int c = xSel.CompareTo(ySel);
            if (c == 0)
            {
                // Fall back to standard ordering if selectivities are equal
                c = x.CompareTo(y);
            }
            return c;
        }

        private void GetSelectivities(ITriplePattern x, out double subj, out double pred, out double obj)
        {
            switch (x.PatternType)
            {
                case TriplePatternType.Match:
                    IMatchTriplePattern p = (IMatchTriplePattern)x;
                    switch (p.IndexType)
                    {
                        case TripleIndexType.NoVariables:
                            subj = _weights.SubjectWeighting(((NodeMatchPattern)p.Subject).Node);
                            pred = _weights.PredicateWeighting(((NodeMatchPattern)p.Predicate).Node);
                            obj = _weights.ObjectWeighting(((NodeMatchPattern)p.Object).Node);
                            break;
                        case TripleIndexType.Object:
                            subj = _weights.DefaultVariableWeighting;
                            pred = _weights.DefaultVariableWeighting;
                            obj = _weights.ObjectWeighting(((NodeMatchPattern)p.Object).Node);
                            break;
                        case TripleIndexType.Predicate:
                            subj = _weights.DefaultVariableWeighting;
                            pred = _weights.PredicateWeighting(((NodeMatchPattern)p.Predicate).Node);
                            obj = _weights.DefaultVariableWeighting;
                            break;
                        case TripleIndexType.PredicateObject:
                            subj = _weights.DefaultVariableWeighting;
                            pred = _weights.PredicateWeighting(((NodeMatchPattern)p.Predicate).Node);
                            obj = _weights.ObjectWeighting(((NodeMatchPattern)p.Object).Node);
                            break;
                        case TripleIndexType.Subject:
                            subj = _weights.SubjectWeighting(((NodeMatchPattern)p.Subject).Node);
                            pred = _weights.DefaultVariableWeighting;
                            obj = _weights.DefaultVariableWeighting;
                            break;
                        case TripleIndexType.SubjectObject:
                            subj = _weights.SubjectWeighting(((NodeMatchPattern)p.Subject).Node);
                            pred = _weights.DefaultVariableWeighting;
                            obj = _weights.PredicateWeighting(((NodeMatchPattern)p.Object).Node);
                            break;
                        case TripleIndexType.SubjectPredicate:
                            subj = _weights.SubjectWeighting(((NodeMatchPattern)p.Subject).Node);
                            pred = _weights.PredicateWeighting(((NodeMatchPattern)p.Predicate).Node);
                            obj = _weights.DefaultVariableWeighting;
                            break;
                        default:
                            // Shouldn't see an unknown index type but have to keep the compiler happy
                            subj = 1d;
                            pred = 1d;
                            obj = 1d;
                            break;
                    }
                    break;
                default:
                    // Otherwise all are considered to have equivalent selectivity
                    subj = 1d;
                    pred = 1d;
                    obj = 1d;
                    break;
            }
        }
    }
}
