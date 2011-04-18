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

    public class ZeroLengthPath : BasePathOperator
    {
        public ZeroLengthPath(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, end, path) { }
    
        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            if (this.AreBothTerms())
            {
                if (this.AreSameTerms())
                {
                    return new IdentityMultiset();
                }
                else
                {
                    return new NullMultiset();
                }
            }

            String subjVar = this.PathStart.VariableName;
            String objVar = this.PathEnd.VariableName;

            //Determine the Triples to which this applies
            IEnumerable<Triple> ts = null;
            if (subjVar != null)
            {
                //Subject is a Variable
                if (context.InputMultiset.ContainsVariable(subjVar))
                {
                    //Subject is Bound
                    if (objVar != null)
                    {
                        //Object is a Variable
                        if (context.InputMultiset.ContainsVariable(objVar))
                        {
                            //Object is Bound
                            ts = (from s in context.InputMultiset.Sets
                                  where s[subjVar] != null && s[objVar] != null
                                  from t in context.Data.GetTriplesWithSubjectObject(s[subjVar], s[objVar])
                                  select t);
                        }
                        else
                        {
                            //Object is Unbound
                            ts = (from s in context.InputMultiset.Sets
                                  where s[subjVar] != null
                                  from t in context.Data.GetTriplesWithSubject(s[subjVar])
                                  select t);
                        }
                    }
                    else
                    {
                        //Object is a Term
                        //Preseve sets where the Object Term is equal to the currently bound Subject
                        INode objTerm = ((NodeMatchPattern)this.PathEnd).Node;
                        foreach (Set s in context.InputMultiset.Sets)
                        {
                            INode temp = s[subjVar];
                            if (temp != null && temp.Equals(objTerm))
                            {
                                context.OutputMultiset.Add(new Set(s));
                            }
                        }
                    }
                }
                else
                {
                    //Subject is Unbound
                    if (objVar != null)
                    {
                        //Object is a Variable
                        if (context.InputMultiset.ContainsVariable(objVar))
                        {
                            //Object is Bound
                            ts = (from s in context.InputMultiset.Sets
                                  where s[objVar] != null
                                  from t in context.Data.GetTriplesWithObject(s[objVar])
                                  select t);
                        }
                        else
                        {
                            //Object is Unbound
                            HashSet<INode> nodes = new HashSet<INode>();
                            foreach (Triple t in context.Data.Triples)
                            {
                                nodes.Add(t.Subject);
                                nodes.Add(t.Object);
                            }
                            foreach (INode n in nodes)
                            {
                                Set s = new Set();
                                s.Add(subjVar, n);
                                s.Add(objVar, n);
                                context.OutputMultiset.Add(s);
                            }
                        }
                    }
                    else
                    {
                        //Object is a Term
                        //Create a single set with the Variable bound to the Object Term
                        Set s = new Set();
                        s.Add(subjVar, ((NodeMatchPattern)this.PathEnd).Node);
                        context.OutputMultiset.Add(s);
                    }
                }
            }
            else if (objVar != null)
            {
                //Subject is a Term but Object is a Variable
                if (context.InputMultiset.ContainsVariable(objVar))
                {
                    //Object is Bound
                    //Preseve sets where the Subject Term is equal to the currently bound Object
                    INode subjTerm = ((NodeMatchPattern)this.PathStart).Node;
                    foreach (Set s in context.InputMultiset.Sets)
                    {
                        INode temp = s[objVar];
                        if (temp != null && temp.Equals(subjTerm))
                        {
                            context.OutputMultiset.Add(new Set(s));
                        }
                    }
                }
                else
                {
                    //Object is Unbound
                    //Create a single set with the Variable bound to the Suject Term
                    Set s = new Set();
                    s.Add(objVar, ((NodeMatchPattern)this.PathStart).Node);
                    context.OutputMultiset.Add(s);
                }
            }
            else
            {
                //Should already have dealt with this earlier (the AreBothTerms() and AreSameTerms() branch)
                throw new RdfQueryException("Reached unexpected point of ZeroLengthPath evaluation");
            }

            //Get the Matches only if we haven't already generated the output
            if (ts != null)
            {
                HashSet<KeyValuePair<INode, INode>> matches = new HashSet<KeyValuePair<INode, INode>>();
                foreach (Triple t in ts)
                {
                    if (this.PathStart.Accepts(context, t.Subject) && this.PathEnd.Accepts(context, t.Object))
                    {
                        matches.Add(new KeyValuePair<INode, INode>(t.Subject, t.Object));
                    }
                }

                //Generate the Output based on the mathces
                if (matches.Count == 0)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    if (this.PathStart.VariableName == null && this.PathEnd.VariableName == null)
                    {
                        context.OutputMultiset = new IdentityMultiset();
                    }
                    else
                    {
                        context.OutputMultiset = new Multiset();
                        foreach (KeyValuePair<INode, INode> m in matches)
                        {
                            Set s = new Set();
                            if (subjVar != null) s.Add(subjVar, m.Key);
                            if (objVar != null) s.Add(objVar, m.Value);
                            context.OutputMultiset.Add(s);
                        }
                    }
                }
            }
            return context.OutputMultiset;
        }

        private bool AreBothTerms()
        {
            return (this.PathStart.VariableName == null && this.PathEnd.VariableName == null);
        }

        private bool AreSameTerms()
        {
            if (this.PathStart is NodeMatchPattern && this.PathEnd is NodeMatchPattern)
            {
                return ((NodeMatchPattern)this.PathStart).Node.Equals(((NodeMatchPattern)this.PathEnd).Node);
            }
            else if (this.PathStart is FixedBlankNodePattern && this.PathEnd is FixedBlankNodePattern)
            {
                return ((FixedBlankNodePattern)this.PathStart).InternalID.Equals(((FixedBlankNodePattern)this.PathEnd).InternalID);
            }
            else
            {
                return false;
            }
        }

        public override string  ToString()
        {
            return "ZeroLengthPath(" + this.PathStart.ToString() + ", " + this.Path.ToString() + ", " + this.PathEnd.ToString() + ")";
        }

        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(this.PathStart, new FixedCardinality(this.Path, 0), this.PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }

    public class ZeroOrMorePath : BasePathOperator
    {
        public ZeroOrMorePath(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, end, path) { }

        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "ZeroOrMorePath(" + this.PathStart.ToString() + ", " + this.Path.ToString() + ", " + this.PathEnd.ToString() + ")";
        }

        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(this.PathStart, new ZeroOrMore(this.Path), this.PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }

    public class OneOrMorePath : BasePathOperator
    {
        public OneOrMorePath(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, end, path) { }

        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            List<List<INode>> paths = new List<List<INode>>();
            BaseMultiset initialInput = context.InputMultiset;
            int step = 0, prevCount = 0, skipCount = 0;

            String subjVar = this.PathStart.VariableName;
            String objVar = this.PathEnd.VariableName;
            bool bothTerms = (subjVar == null && objVar == null);

            if (subjVar == null || (subjVar != null && context.InputMultiset.ContainsVariable(subjVar)) || (objVar != null && !context.InputMultiset.ContainsVariable(objVar)))
            {
                //Work Forwards from the Starting Term or Bound Variable
                //OR if there is no Ending Term or Bound Variable work forwards regardless
                if (subjVar == null)
                {
                    paths.Add(((NodeMatchPattern)this.PathStart).Node.AsEnumerable().ToList());
                } 
                else if (context.InputMultiset.ContainsVariable(subjVar))
                {
                    paths.AddRange((from s in context.InputMultiset.Sets
                                    where s[subjVar] != null
                                    select s[subjVar]).Distinct().Select(n => n.AsEnumerable().ToList()));
                }

                //if (this.Path is Property)
                //{
                    //INode predicate = ((Property)this.Path).Predicate;
                    if (paths.Count == 0)
                    {
                        //If nothing yet bound need to do a step to generate the path starts
                        //foreach (INode subj in context.Data.GetTriplesWithPredicate(predicate).Select(t => t.Subject).Distinct())
                        //{
                        //    paths.Add(subj.AsEnumerable().ToList());
                        //}
                        this.GetPathStarts(context, paths);
                    }

                    //Traverse the Paths
                    do
                    {
                        prevCount = paths.Count;
                        foreach (List<INode> path in paths.Skip(skipCount).ToList())
                        {
                            //foreach (Triple t in context.Data.GetTriplesWithSubjectPredicate(path[path.Count - 1], predicate))
                            //{
                            //    if (!path.Contains(t.Object))
                            //    {
                            //        List<INode> newPath = new List<INode>(path);
                            //        newPath.Add(t.Object);
                            //        paths.Add(newPath);
                            //    }
                            //}
                            foreach (INode nextStep in this.EvaluateStep(context, path))
                            {
                                List<INode> newPath = new List<INode>(path);
                                newPath.Add(nextStep);
                                paths.Add(newPath);
                            }
                        }

                        if (step == 0)
                        {
                            //Remove any 1 length paths as these denote path starts that couldn't be traversed
                            paths.RemoveAll(p => p.Count == 1);
                            prevCount = paths.Count;
                        }

                        //Update Counts
                        //skipCount is used to indicate the paths which we will ignore for the purposes of
                        //trying to further extend since we've already done them once
                        step++;
                        if (paths.Count == 0) break;
                        if (step > 1)
                        {
                            skipCount = prevCount;
                        }

                        //Can short circuit evaluation here if both are terms and any path is acceptable
                        if (bothTerms)
                        {
                            bool exit = false;
                            foreach (List<INode> path in paths)
                            {
                                if (this.PathStart.Accepts(context, path[0]) && this.PathEnd.Accepts(context, path[path.Count - 1]))
                                {
                                    exit = true;
                                    break;
                                }
                            }
                            if (exit) break;
                        }
                    } while (paths.Count > prevCount || (step == 1 && paths.Count == prevCount));

                    if (paths.Count == 0)
                    {
                        //If all path starts lead nowhere then we get the Null Multiset as a result
                        context.OutputMultiset = new NullMultiset();
                    }
                    else
                    {
                        context.OutputMultiset = new Multiset();

                        //Evaluate the Paths to check that are acceptable
                        foreach (List<INode> path in paths)
                        {
                            if (this.PathStart.Accepts(context, path[0]) && this.PathEnd.Accepts(context, path[path.Count - 1]))
                            {
                                Set s = new Set();
                                if (!bothTerms)
                                {
                                    if (subjVar != null) s.Add(subjVar, path[0]);
                                    if (objVar != null) s.Add(objVar, path[path.Count - 1]);
                                }
                                context.OutputMultiset.Add(s);

                                //If both are terms can short circuit evaluate here
                                //It is sufficient just to determine that there is one path possible
                                if (bothTerms) break;
                            }
                        }

                        if (bothTerms)
                        {
                            //If both were terms transform to an Identity/Null Multiset as appropriate
                            if (context.OutputMultiset.IsEmpty)
                            {
                                context.OutputMultiset = new NullMultiset();
                            }
                            else
                            {
                                context.OutputMultiset = new IdentityMultiset();
                            }
                        }
                    }
                //}
                //else
                //{
                //    throw new NotImplementedException("Complex Paths with a + modifier are not yet supported");
                //}

            } 
            else if (objVar == null || (objVar != null && context.InputMultiset.ContainsVariable(objVar)))
            {
                //Work Backwards from Ending Term or Bound Variable
                if (objVar == null)
                {
                    paths.Add(((NodeMatchPattern)this.PathEnd).Node.AsEnumerable().ToList());
                }
                else
                {
                    paths.AddRange((from s in context.InputMultiset.Sets
                                    where s[objVar] != null
                                    select s[objVar]).Distinct().Select(n => n.AsEnumerable().ToList()));
                }

                throw new NotImplementedException("Paths with a + modifier which will be evaluated in reverse as the Object is a Term/Bound Variable are not yet implemented");
            } 

            context.InputMultiset = initialInput;
            return context.OutputMultiset;
        }

        private void GetPathStarts(SparqlEvaluationContext context, List<List<INode>> paths)
        {
            HashSet<KeyValuePair<INode,INode>> nodes = new HashSet<KeyValuePair<INode,INode>>();
            if (this.Path is Property)
            {
                INode predicate = ((Property)this.Path).Predicate;
                foreach (Triple t in context.Data.GetTriplesWithPredicate(predicate))
                {
                    nodes.Add(new KeyValuePair<INode, INode>(t.Subject, t.Object));
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
                            nodes.Add(new KeyValuePair<INode, INode>(s["x"], s["y"]));
                        }
                    }
                }
            }

            paths.AddRange(nodes.Select(kvp => new List<INode>(new INode[] { kvp.Key, kvp.Value })));
        }

        private List<INode> EvaluateStep(SparqlEvaluationContext context, List<INode> path)
        {
            if (this.Path is Property)
            {
                HashSet<INode> nodes = new HashSet<INode>();
                INode predicate = ((Property)this.Path).Predicate;
                foreach (Triple t in context.Data.GetTriplesWithSubjectPredicate(path[path.Count - 1], predicate))
                {
                    if (!path.Contains(t.Object))
                    {
                        nodes.Add(t.Object);
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
                temp.Add("x", path[path.Count - 1]);
                currInput.Add(temp);
                context.InputMultiset = currInput;

                Bgp bgp = new Bgp(new PropertyPathPattern(x, this.Path, y));
                BaseMultiset results = bgp.Evaluate(context);
                context.InputMultiset = initialInput;

                if (!results.IsEmpty)
                {
                    foreach (Set s in results.Sets)
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

                return nodes;
            }
        }

        public override string ToString()
        {
            return "OneOrMorePath(" + this.PathStart.ToString() + ", " + this.Path.ToString() + ", " + this.PathEnd.ToString() + ")";
        }

        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(this.PathStart, new OneOrMore(this.Path), this.PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }

    public class NegatedPropertySet : ISparqlAlgebra
    {
        private List<INode> _properties = new List<INode>();
        private PatternItem _start, _end;
        private bool _inverse;

        public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties, bool inverse)
        {
            this._start = start;
            this._end = end;
            this._properties.AddRange(properties.Select(p => p.Predicate));
            this._inverse = inverse;
        }

        public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties)
            : this(start, end, properties, false) { }

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

        public IEnumerable<INode> Properties
        {
            get
            {
                return this._properties;
            }
        }

        #region ISparqlAlgebra Members

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            IEnumerable<Triple> ts;
            String subjVar = this._start.VariableName;
            String objVar = this._end.VariableName;
            if (subjVar != null && context.InputMultiset.ContainsVariable(subjVar))
            {
                if (objVar != null && context.InputMultiset.ContainsVariable(objVar))
                {
                    ts = (from s in context.InputMultiset.Sets
                          where s[subjVar] != null && s[objVar] != null
                          from t in context.Data.GetTriplesWithSubjectObject(s[subjVar], s[objVar])
                          select t);
                }
                else
                {
                    ts = (from s in context.InputMultiset.Sets
                          where s[subjVar] != null
                          from t in context.Data.GetTriplesWithSubject(s[subjVar])
                          select t);
                }
            }
            else if (objVar != null && context.InputMultiset.ContainsVariable(objVar))
            {
                ts = (from s in context.InputMultiset.Sets
                      where s[objVar] != null
                      from t in context.Data.GetTriplesWithObject(s[objVar])
                      select t);
            }
            else
            {
                ts = context.Data.Triples;
            }

            context.OutputMultiset = new Multiset();
            if (this._inverse)
            {
                String temp = objVar;
                objVar = subjVar;
                subjVar = temp;
            }
            foreach (Triple t in ts)
            {
                if (!this._properties.Contains(t.Predicate))
                {
                    Set s = new Set();
                    if (subjVar != null) s.Add(subjVar, t.Subject);
                    if (objVar != null) s.Add(objVar, t.Object);
                    context.OutputMultiset.Add(s);
                }
            }

            if (subjVar == null && objVar == null)
            {
                if (context.OutputMultiset.Count == 0)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
            }

            return context.OutputMultiset;
        }

        public IEnumerable<string> Variables
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public SparqlQuery ToQuery()
        {
            throw new NotImplementedException();
        }

        public GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp;
            if (this._inverse)
            {
                pp = new PropertyPathPattern(this.PathStart, new NegatedSet(Enumerable.Empty<Property>(), this._properties.Select(p => new Property(p))), this.PathEnd);
            }
            else
            {
                pp = new PropertyPathPattern(this.PathStart, new NegatedSet(this._properties.Select(p => new Property(p)), Enumerable.Empty<Property>()), this.PathEnd);
            }
            gp.AddTriplePattern(pp);
            return gp;
        }

        #endregion

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("NegatedPropertySet(");
            output.Append(this._start.ToString());
            output.Append(", {");
            for (int i = 0; i < this._properties.Count; i++)
            {
                output.Append(this._properties[i].ToString());
                if (i < this._properties.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append("}, ");
            output.Append(this._end.ToString());
            output.Append(')');

            return output.ToString();
        }
    }

}
