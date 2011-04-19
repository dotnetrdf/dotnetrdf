using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    public interface IPathOperator : ISparqlAlgebra
    {
        PatternItem PathStart
        {
            get;
        }

        PatternItem PathEnd
        {
            get;
        }

        ISparqlPath Path
        {
            get;
        }
    }

    public abstract class BasePathOperator : IPathOperator
    {
        private PatternItem _start, _end;
        private ISparqlPath _path;
        private HashSet<String> _vars = new HashSet<string>();

        public BasePathOperator(PatternItem start, PatternItem end, ISparqlPath path)
        {
            this._start = start;
            this._end = end;
            this._path = path;

            if (this._start.VariableName != null) this._vars.Add(this._start.VariableName);
            if (this._end.VariableName != null) this._vars.Add(this._end.VariableName);
        }

        public PatternItem PathStart
        {
            get 
            { 
                return this._start; 
            }
        }

        public PatternItem PathEnd
        {
            get 
            { 
                return this._end; 
            }
        }

        public ISparqlPath Path
        {
            get 
            { 
                return this._path;
            }
        }

        public abstract BaseMultiset Evaluate(SparqlEvaluationContext context);

        public IEnumerable<string> Variables
        {
            get 
            {
                return this._vars;
            }
        }

        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        public abstract GraphPattern ToGraphPattern();

        public abstract override string ToString();
    }

    public abstract class BaseArbitraryLengthPathOperator : BasePathOperator
    {
        public BaseArbitraryLengthPathOperator(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, end, path) { }

        protected void GetPathStarts(SparqlEvaluationContext context, List<List<INode>> paths, bool reverse)
        {
            HashSet<KeyValuePair<INode, INode>> nodes = new HashSet<KeyValuePair<INode, INode>>();
            if (this.Path is Property)
            {
                INode predicate = ((Property)this.Path).Predicate;
                foreach (Triple t in context.Data.GetTriplesWithPredicate(predicate))
                {
                    if (reverse)
                    {
                        nodes.Add(new KeyValuePair<INode, INode>(t.Object, t.Subject));
                    }
                    else
                    {
                        nodes.Add(new KeyValuePair<INode, INode>(t.Subject, t.Object));
                    }
                }
            }
            else
            {
                BaseMultiset initialInput = context.InputMultiset;
                context.InputMultiset = new IdentityMultiset();
                VariablePattern x = new VariablePattern("?x");
                VariablePattern y = new VariablePattern("?y");
                Bgp bgp = new Bgp(new PropertyPathPattern(x, this.Path, y));

                BaseMultiset results = bgp.Evaluate(context);
                context.InputMultiset = initialInput;

                if (!results.IsEmpty)
                {
                    foreach (Set s in results.Sets)
                    {
                        if (s["x"] != null && s["y"] != null)
                        {
                            if (reverse)
                            {
                                nodes.Add(new KeyValuePair<INode, INode>(s["y"], s["x"]));
                            }
                            else
                            {
                                nodes.Add(new KeyValuePair<INode, INode>(s["x"], s["y"]));
                            }
                        }
                    }
                }
            }

            paths.AddRange(nodes.Select(kvp => new List<INode>(new INode[] { kvp.Key, kvp.Value })));
        }

        protected List<INode> EvaluateStep(SparqlEvaluationContext context, List<INode> path, bool reverse)
        {
            if (this.Path is Property)
            {
                HashSet<INode> nodes = new HashSet<INode>();
                INode predicate = ((Property)this.Path).Predicate;
                IEnumerable<Triple> ts = (reverse ? context.Data.GetTriplesWithPredicateObject(predicate, path[path.Count - 1]) : context.Data.GetTriplesWithSubjectPredicate(path[path.Count - 1], predicate));
                foreach (Triple t in ts)
                {
                    if (reverse)
                    {
                        if (!path.Contains(t.Subject))
                        {
                            nodes.Add(t.Subject);
                        }
                    }
                    else
                    {
                        if (!path.Contains(t.Object))
                        {
                            nodes.Add(t.Object);
                        }
                    }
                }
                return nodes.ToList();
            }
            else
            {
                List<INode> nodes = new List<INode>();

                BaseMultiset initialInput = context.InputMultiset;
                Multiset currInput = new Multiset();
                VariablePattern x = new VariablePattern("?x");
                VariablePattern y = new VariablePattern("?y");
                Set temp = new Set();
                if (reverse)
                {
                    temp.Add("y", path[path.Count - 1]);
                }
                else
                {
                    temp.Add("x", path[path.Count - 1]);
                }
                currInput.Add(temp);
                context.InputMultiset = currInput;

                Bgp bgp = new Bgp(new PropertyPathPattern(x, this.Path, y));
                BaseMultiset results = bgp.Evaluate(context);
                context.InputMultiset = initialInput;

                if (!results.IsEmpty)
                {
                    foreach (Set s in results.Sets)
                    {
                        if (reverse)
                        {
                            if (s["x"] != null)
                            {
                                if (!path.Contains(s["x"]))
                                {
                                    nodes.Add(s["x"]);
                                }
                            }
                        }
                        else
                        {
                            if (s["y"] != null)
                            {
                                if (!path.Contains(s["y"]))
                                {
                                    nodes.Add(s["y"]);
                                }
                            }
                        }
                    }
                }

                return nodes;
            }
        }
    }
}
