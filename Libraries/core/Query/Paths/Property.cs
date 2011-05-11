/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents a Predicate which is part of a Path
    /// </summary>
    public class Property : ISparqlPath
    {
        private INode _predicate;

        /// <summary>
        /// Creates a new Property
        /// </summary>
        /// <param name="predicate">Predicate</param>
        public Property(INode predicate)
        {
            this._predicate = predicate;
        }

        /// <summary>
        /// Gets the Predicate this part of the Path represents
        /// </summary>
        public INode Predicate
        {
            get
            {
                return this._predicate;
            }
        }

        /// <summary>
        /// Evaluates the Path using the given Path Evaluation Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public void Evaluate(PathEvaluationContext context)
        {
            IEnumerable<Triple> ts;

            if (context.IsFirst)
            {
                //First thing firsts we'll set that the path is no longer at the first thing since we're the first thing!
                context.IsFirst = false;

                //We're the start of a Path
                if (context.PathStart.VariableName != null)
                {
                    //Path starts from a Variable
                    String var = context.PathStart.VariableName;
                    if (context.SparqlContext.InputMultiset.ContainsVariable(var))
                    {
                        //Path starts from a Variable for which we have existing values
                        IEnumerable<INode> values = (from s in context.SparqlContext.InputMultiset.Sets
                                                     where s.ContainsVariable(var)
                                                     select s[var]).Distinct();
                        foreach (INode n in values)
                        {
                            if (context.IsReversed)
                            {
                                //If the Path is currently reversed then we'll look at triples with the given Predicate-Object pair
                                ts = context.SparqlContext.Data.GetTriplesWithPredicateObject(this._predicate, n);
                            }
                            else
                            {
                                //For normal forward paths we'll look at triples with the given Subject-Predicate pair
                                ts = context.SparqlContext.Data.GetTriplesWithSubjectPredicate(n, this._predicate);
                            }

                            //Bind path as appropriate
                            foreach (Triple t in ts)
                            {
                                PotentialPath p;
                                if (context.IsReversed) 
                                {
                                    p = new PotentialPath(n, t.Subject);  
                                } 
                                else 
                                {
                                    p = new PotentialPath(n, t.Object);   
                                }
                                context.AddPath(p);
                            }
                        }
                    }
                    else
                    {
                        //Path starts from an as yet unbound Variable
                        ts = context.SparqlContext.Data.GetTriplesWithPredicate(this._predicate);

                        //Bind paths as appropriate
                        foreach (Triple t in ts)
                        {
                            PotentialPath p;
                            if (context.IsReversed)
                            {
                                p = new PotentialPath(t.Object, t.Subject);
                            }
                            else
                            {
                                p = new PotentialPath(t.Subject, t.Object);
                            }
                            context.AddPath(p);
                        }
                    }
                }
                else
                {
                    //Path starts from a fixed value
                    INode fixedSubj = ((NodeMatchPattern)context.PathStart).Node;
                    if (context.IsReversed)
                    {
                        //If the Path is currently reversed then we'll look at triples with the given Predicate-Object pair
                        ts = context.SparqlContext.Data.GetTriplesWithPredicateObject(this._predicate, fixedSubj);
                    }
                    else
                    {
                        //For normal forward paths we'll look at triples with the given Subject-Predicate pair
                        ts = context.SparqlContext.Data.GetTriplesWithSubjectPredicate(fixedSubj, this._predicate);
                    }

                    //Map each Triple as a Path
                    foreach (Triple t in ts)
                    {
                        PotentialPath p;
                        if (context.IsReversed)
                        {
                            p = new PotentialPath(t.Object, fixedSubj);
                        }
                        else
                        {
                            p = new PotentialPath(fixedSubj, t.Object);
                        }
                        context.AddPath(p);
                    }
                }
            }
            else
            {
                //We're not the start of a Path so we're continuing from a previous path
                //This means all paths must continue from the Current position of an incomplete path

                //If we are permitted to introduce new paths we can go ahead and do that here
                if (context.PermitsNewPaths)
                {
                    //Introduce new paths based on the predicate at this stage
                    ts = context.SparqlContext.Data.GetTriplesWithPredicate(this._predicate);
                    foreach (Triple t in ts)
                    {
                        PotentialPath p;
                        if (context.IsReversed)
                        {
                            p = new PotentialPath(t.Object, t.Object);
                            p.Length = 0;
                        }
                        else
                        {
                            p = new PotentialPath(t.Subject, t.Subject);
                            p.Length = 0;
                        }
                        context.AddPath(p);
                    }
                    context.PermitsNewPaths = false;
                }
                
                //If there are no incomplete paths we abort
                if (context.Paths.Count == 0) return;

                //For each incomplete path we attempt to extend the Path
                HashSet<PotentialPath> incomplete = new HashSet<PotentialPath>();
                foreach (PotentialPath p in context.Paths)
                {
                    //Ignore dead-end paths
                    if (p.IsDeadEnd) continue;

                    if (context.IsReversed)
                    {
                        ts = context.SparqlContext.Data.GetTriplesWithPredicateObject(this._predicate, p.Current);
                    }
                    else
                    {
                        ts = context.SparqlContext.Data.GetTriplesWithSubjectPredicate(p.Current, this._predicate);
                    }

                    //int c = 0;
                    foreach (Triple t in ts)
                    {
                        PotentialPath p2; ;
                        if (context.IsReversed)
                        {
                            p2 = new PotentialPath(p.Start, t.Subject);
                            p2.Length = p.Length + 1;
                        }
                        else
                        {
                            p2 = new PotentialPath(p.Start, t.Object);
                            p2.Length = p.Length + 1;
                        }
                        incomplete.Add(p2);
                    }
                }

                //Add the newly found paths to the set of incomplete paths
                foreach (PotentialPath p in incomplete)
                {
                    context.AddPath(p);
                }
            }

            //If we're the Last thing in the Path we can now mark any completed paths
            if (context.IsLast)
            {
                bool objVar = (context.PathEnd.VariableName != null);
                bool objVarBound = (objVar && context.SparqlContext.InputMultiset.ContainsVariable(context.PathEnd.VariableName));
                foreach (PotentialPath p in context.Paths)
                {
                    if (!p.IsDeadEnd && !p.IsComplete && !p.IsPartial)
                    {
                        if (objVar)
                        {
                            if (objVarBound)
                            {
                                //If the end of the Path is a previously bound variable then we do an accepts check
                                if (context.PathEnd.Accepts(context.SparqlContext, p.Current))
                                {
                                    p.IsComplete = true;
                                    context.AddCompletePath(p);
                                }
                                else
                                {
                                    //We can mark this path as partial (I think) since it itself can't be completed
                                    //but there's a possibility that it might lead to a completable path at a later point
                                    p.IsPartial = true;
                                }
                            }
                            else
                            {
                                //If the end of the Path is an as yet unbound variable then any value is acceptable
                                p.IsComplete = true;
                                context.AddCompletePath(p);
                            }
                        }
                        else
                        {
                            if (context.PathEnd.Accepts(context.SparqlContext, p.Current))
                            {
                                //If the Path End accepts the current path end we can complete it
                                p.IsComplete = true;
                                context.AddCompletePath(p);
                            }
                            else
                            {
                                //We can mark this path as partial (I think) since it itself can't be completed
                                //but there's a possibility that it might lead to a completable path at a later point
                                p.IsPartial = true;
                            }
                        }
                    }
                }

                if (context.CanAbortEarly)
                {
                    if (context.CompletePaths.Any(p => context.PathStart.Accepts(context.SparqlContext, p.Start) && context.PathEnd.Accepts(context.SparqlContext, p.Current)))
                    {
                        throw new RdfQueryPathFoundException();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            switch (this._predicate.NodeType)
            {
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)this._predicate;
                    bool longlit = (lit.Value.Contains('\n') || lit.Value.Contains('\r') || lit.Value.Contains('"'));

                    if (longlit)
                    {
                        output.Append("\"\"\"");
                    }
                    else
                    {
                        output.Append("\"");
                    }

                    output.Append(lit.Value);

                    if (longlit)
                    {
                        output.Append("\"\"\"");
                    }
                    else
                    {
                        output.Append("\"");
                    }

                    if (!lit.Language.Equals(String.Empty))
                    {
                        output.Append("@");
                        output.Append(lit.Language);
                    }
                    else if (lit.DataType != null)
                    {
                        output.Append("^^<");
                        output.Append(lit.DataType.ToString());
                        output.Append(">");
                    }

                    break;

                case NodeType.Uri:
                    IUriNode uri = (IUriNode)this._predicate;
                    output.Append('<');
                    output.Append(this._predicate.ToString());
                    output.Append('>');
                    break;

                default:
                    output.Append(this._predicate.ToString());
                    break;
            }

            return output.ToString();
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            context.AddTriplePattern(context.GetTriplePattern(context.Subject, this, context.Object));
            return context.ToAlgebra();
        }
    }
}
