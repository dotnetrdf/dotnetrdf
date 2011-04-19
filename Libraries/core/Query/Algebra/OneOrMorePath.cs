using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    public class OneOrMorePath : BaseArbitraryLengthPathOperator
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
            bool reverse = false;

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
                reverse = true;
            }

            if (paths.Count == 0)
            {
                this.GetPathStarts(context, paths, reverse);
            }

            //Traverse the Paths
            do
            {
                prevCount = paths.Count;
                foreach (List<INode> path in paths.Skip(skipCount).ToList())
                {
                    foreach (INode nextStep in this.EvaluateStep(context, path, reverse))
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
                        if (reverse)
                        {
                            if (this.PathEnd.Accepts(context, path[0]) && this.PathStart.Accepts(context, path[path.Count - 1]))
                            {
                                exit = true;
                                break;
                            }
                        }
                        else
                        {
                            if (this.PathStart.Accepts(context, path[0]) && this.PathEnd.Accepts(context, path[path.Count - 1]))
                            {
                                exit = true;
                                break;
                            }
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
                    if (reverse)
                    {
                        if (this.PathEnd.Accepts(context, path[0]) && this.PathStart.Accepts(context, path[path.Count - 1]))
                        {
                            Set s = new Set();
                            if (!bothTerms)
                            {
                                if (subjVar != null) s.Add(subjVar, path[path.Count - 1]);
                                if (objVar != null) s.Add(objVar, path[0]);
                            }
                            context.OutputMultiset.Add(s);

                            //If both are terms can short circuit evaluation here
                            //It is sufficient just to determine that there is one path possible
                            if (bothTerms) break;
                        }
                    }
                    else
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

                            //If both are terms can short circuit evaluation here
                            //It is sufficient just to determine that there is one path possible
                            if (bothTerms) break;
                        }
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

            context.InputMultiset = initialInput;
            return context.OutputMultiset;
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
}
