using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Optimisation
{
    public class WeightedOptimiser : BaseQueryOptimiser
    {
        private Weightings _weights;

        public const double DefaultSubjectWeight = 0.8d;
        public const double DefaultPredicateWeight = 0.4d;
        public const double DefaultObjectWeight = 0.6d;
        public const double DefaultVariableWeight = 1d;

        public WeightedOptimiser(IGraph g)
        {
            this._weights = new Weightings(g);
        }

        public WeightedOptimiser(IGraph g, double subjWeight, double predWeight, double objWeight)
            : this(g)
        {
            this._weights.DefaultSubjectWeighting = subjWeight;
            this._weights.DefaultPredicateWeighting = predWeight;
            this._weights.DefaultObjectWeighting = objWeight;
        }

        protected override IComparer<ITriplePattern> GetRankingComparer()
        {
            return new WeightingComparer(this._weights);
        }
    }

    class Weightings
    {
        private Dictionary<INode, int> _subjectWeightings = new Dictionary<INode, int>();
        private Dictionary<INode, int> _predicateWeightings = new Dictionary<INode, int>();
        private Dictionary<INode, int> _objectWeightings = new Dictionary<INode, int>();

        private double _defSubjWeight = WeightedOptimiser.DefaultSubjectWeight;
        private double _defPredWeight = WeightedOptimiser.DefaultPredicateWeight;
        private double _defObjWeight = WeightedOptimiser.DefaultObjectWeight;
        private double _defVarWeight = WeightedOptimiser.DefaultVariableWeight;

        public Weightings(IGraph g)
        {
            if (g == null) return;

            g.NamespaceMap.AddNamespace("opt", new Uri(SparqlOptimiser.OptimiserStatsNamespace));

            INode optSubjCount = g.CreateUriNode("opt:subjectCount");
            INode optPredCount = g.CreateUriNode("opt:predicateCount");
            INode optObjCount = g.CreateUriNode("opt:objectCount");
            INode optCount = g.CreateUriNode("opt:count");

            foreach (Triple t in g.GetTriplesWithPredicate(optSubjCount))
            {
                this.SetSubjectCount(t.Subject, t.Object);
            }
            foreach (Triple t in g.GetTriplesWithPredicate(optPredCount))
            {
                this.SetPredicateCount(t.Subject, t.Object);
            }
            foreach (Triple t in g.GetTriplesWithPredicate(optObjCount))
            {
                this.SetObjectCount(t.Subject, t.Object);
            }
            foreach (Triple t in g.GetTriplesWithPredicate(optCount))
            {
                this.SetSubjectCount(t.Subject, t.Object);
                this.SetPredicateCount(t.Subject, t.Object);
                this.SetObjectCount(t.Subject, t.Object);
            }
        }

        public void SetSubjectCount(INode n, INode count)
        {
            if (count.NodeType == NodeType.Literal)
            {
                int i;
                if (Int32.TryParse(((ILiteralNode)count).Value, out i))
                {
                    int current;
                    if (this._subjectWeightings.TryGetValue(n, out current))
                    {
                        this._subjectWeightings[n] = Math.Max(current, i);
                    }
                    else
                    {
                        this._subjectWeightings.Add(n, i);
                    }
                }
            }
        }

        public void SetPredicateCount(INode n, INode count)
        {
            if (count.NodeType == NodeType.Literal)
            {
                int i;
                if (Int32.TryParse(((ILiteralNode)count).Value, out i))
                {
                    int current;
                    if (this._predicateWeightings.TryGetValue(n, out current))
                    {
                        this._predicateWeightings[n] = Math.Max(current, i);
                    }
                    else
                    {
                        this._predicateWeightings.Add(n, i);
                    }
                }
            }
        }

        public void SetObjectCount(INode n, INode count)
        {
            if (count.NodeType == NodeType.Literal)
            {
                int i;
                if (Int32.TryParse(((ILiteralNode)count).Value, out i))
                {
                    int current;
                    if (this._objectWeightings.TryGetValue(n, out current))
                    {
                        this._objectWeightings[n] = Math.Max(current, i);
                    }
                    else
                    {
                        this._objectWeightings.Add(n, i);
                    }
                }
            }
        }

        public double SubjectWeighting(INode n)
        {
            int temp = 1;
            if (this._subjectWeightings.TryGetValue(n, out temp))
            {
                temp = Math.Max(1, temp);
                return 1d - (1d / temp);
            }
            else
            {
                return 1d - this._defSubjWeight;
            }
        }

        public double PredicateWeighting(INode n)
        {
            int temp = 1;
            if (this._predicateWeightings.TryGetValue(n, out temp))
            {
                temp = Math.Max(1, temp);
                return 1d - (1d / temp);
            }
            else
            {
                return 1d - this._defPredWeight;
            }
        }

        public double ObjectWeighting(INode n)
        {
            int temp = 1;
            if (this._objectWeightings.TryGetValue(n, out temp))
            {
                temp = Math.Max(1, temp);
                return 1d - (1d / temp);
            }
            else
            {
                return 1d - this._defObjWeight;
            }
        }

        public double DefaultSubjectWeighting
        {
            get
            {
                return this._defSubjWeight;
            }
            set
            {
                this._defSubjWeight = Math.Min(Math.Max(Double.Epsilon, value), 1d);
            }
        }

        public double DefaultPredicateWeighting
        {
            get
            {
                return this._defPredWeight;
            }
            set
            {
                this._defPredWeight = Math.Min(Math.Max(Double.Epsilon, value), 1d);
            }
        }

        public double DefaultObjectWeighting
        {
            get
            {
                return this._defObjWeight;
            }
            set
            {
                this._defObjWeight = Math.Min(Math.Max(Double.Epsilon, value), 1d);
            }
        }

        public double DefaultVariableWeighting
        {
            get
            {
                return this._defVarWeight;
            }
        }
    }

    class WeightingComparer : IComparer<ITriplePattern>
    {
        private Weightings _weights;

        public WeightingComparer(Weightings weights)
        {
            if (weights == null) throw new ArgumentNullException("weights");
            this._weights = weights;
        }

        public int Compare(ITriplePattern x, ITriplePattern y)
        {
            double xSubj, xPred, xObj;
            double ySubj, yPred, yObj;

            this.GetSelectivities(x, out xSubj, out xPred, out xObj);
            this.GetSelectivities(y, out ySubj, out yPred, out yObj);

            double xSel = xSubj * xPred * xObj;
            double ySel = ySubj * yPred * yObj;

            int c = xSel.CompareTo(ySel);
            if (c == 0)
            {
                //Fall back to standard ordering if selectivities are equal
                c = x.CompareTo(y);
            }
            return c;
        }

        private void GetSelectivities(ITriplePattern x, out double subj, out double pred, out double obj)
        {
            switch (x.IndexType)
            {
                case TripleIndexType.NoVariables:
                    subj = this._weights.SubjectWeighting(((NodeMatchPattern)((TriplePattern)x).Subject).Node);
                    pred = this._weights.PredicateWeighting(((NodeMatchPattern)((TriplePattern)x).Predicate).Node);
                    obj = this._weights.ObjectWeighting(((NodeMatchPattern)((TriplePattern)x).Object).Node);
                    break;
                case TripleIndexType.Object:
                    subj = this._weights.DefaultVariableWeighting;
                    pred = this._weights.DefaultVariableWeighting;
                    obj = this._weights.ObjectWeighting(((NodeMatchPattern)((TriplePattern)x).Object).Node);
                    break;
                case TripleIndexType.Predicate:
                    subj = this._weights.DefaultVariableWeighting;
                    pred = this._weights.PredicateWeighting(((NodeMatchPattern)((TriplePattern)x).Predicate).Node);
                    obj = this._weights.DefaultVariableWeighting;
                    break;
                case TripleIndexType.PredicateObject:
                    subj = this._weights.DefaultVariableWeighting;
                    pred = this._weights.PredicateWeighting(((NodeMatchPattern)((TriplePattern)x).Predicate).Node);
                    obj = this._weights.ObjectWeighting(((NodeMatchPattern)((TriplePattern)x).Object).Node);
                    break;
                case TripleIndexType.Subject:
                    subj = this._weights.SubjectWeighting(((NodeMatchPattern)((TriplePattern)x).Subject).Node);
                    pred = this._weights.DefaultVariableWeighting;
                    obj = this._weights.DefaultVariableWeighting;
                    break;
                case TripleIndexType.SubjectObject:
                    subj = this._weights.SubjectWeighting(((NodeMatchPattern)((TriplePattern)x).Subject).Node);
                    pred = this._weights.DefaultVariableWeighting;
                    obj = this._weights.PredicateWeighting(((NodeMatchPattern)((TriplePattern)x).Object).Node);
                    break;
                case TripleIndexType.SubjectPredicate:
                    subj = this._weights.SubjectWeighting(((NodeMatchPattern)((TriplePattern)x).Subject).Node);
                    pred = this._weights.PredicateWeighting(((NodeMatchPattern)((TriplePattern)x).Predicate).Node);
                    obj = this._weights.DefaultVariableWeighting;
                    break;

                case TripleIndexType.None:
                case TripleIndexType.SpecialAssignment:
                case TripleIndexType.SpecialFilter:
                case TripleIndexType.SpecialPropertyPath:
                case TripleIndexType.SpecialSubQuery:
                default:
                    //Otherwise all are considered to have equivalent selectivity
                    subj = 1d;
                    pred = 1d;
                    obj = 1d;
                    break;
            }
        }
    }
}
