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

If this license is not suitable for your intended use please contact
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
    /// Represents a standard forwards path
    /// </summary>
    public class SequencePath : BaseBinaryPath
    {
        /// <summary>
        /// Creates a new Sequence Path
        /// </summary>
        /// <param name="lhs">LHS Path</param>
        /// <param name="rhs">RHS Path</param>
        public SequencePath(ISparqlPath lhs, ISparqlPath rhs)
            : base(lhs, rhs) { }

        /// <summary>
        /// Evaluates the Path in the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public override void Evaluate(PathEvaluationContext context)
        {
            bool last = context.IsLast;
            bool first = context.IsFirst;
            context.IsLast = false;

            //Evaluate the LHS
            this._lhs.Evaluate(context);
            context.IsFirst = false;
            context.IsLast = last;

            //If the LHS allowed for zero-length paths then we need to add these in
            if (this._lhs.AllowsZeroLength)
            {
                if (first)
                {
                    //If it was the first item in the path then we need to add all zero length paths
                    String var = context.PathStart.VariableName;
                    bool subjVar = (var != null);
                    if (subjVar)
                    {
                        //The Path has a variable as a starting point
                        if (context.SparqlContext.InputMultiset.ContainsVariable(var))
                        {
                            //The Variable is already bound so we add every possible value as a zero-length path
                            IEnumerable<INode> values = (from s in context.SparqlContext.InputMultiset.Sets
                                                         where s.ContainsVariable(var)
                                                         select s[var]).Distinct();

                            foreach (INode val in values)
                            {
                                PotentialPath p = new PotentialPath(val, val);
                                p.Length = 0;
                                context.AddPath(p);
                            }
                        }
                        else
                        {
                            //The Variable is not bound (which is very annoying and inefficient)
                            context.PermitsNewPaths = true;
                        }
                    }
                    else
                    {
                        //The Path has a fixed starting point which is nice and easy for us to process
                        //All we do is add a single path
                        INode fixedStart = ((NodeMatchPattern)context.PathStart).Node;
                        PotentialPath p = new PotentialPath(fixedStart, fixedStart);
                        p.Length = 0;
                        context.AddPath(p);
                    }
                }
                else
                {
                    //If it was not the first item in the path then we don't need to do anything explicitly
                    //We'll still have partial paths from the previous step in the sequence and so these
                    //can be used to continue the path with the next step
                }
            }
            else
            {
                //If it didn't allow zero-length paths any existing partial paths are now considered dead-ends
                foreach (PotentialPath p in context.Paths)
                {
                    if (p.IsPartial) p.IsDeadEnd = true;
                }
            }

            //Mark all Paths from the LHS as partial paths
            foreach (PotentialPath p in context.Paths)
            {
                p.IsPartial = true;
            }

            //Evaluate the RHS
            this._rhs.Evaluate(context);
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(this._lhs.ToString());
            output.Append(" / ");
            output.Append(this._rhs.ToString());
            return output.ToString();
        }

        /// <summary>
        /// Generates the Path transform to an Algebra expression
        /// </summary>
        /// <param name="context">Transform Context</param>
        public override void ToAlgebra(PathTransformContext context)
        {
            if (this.IsSimple)
            {
                bool top = context.Top;

                //The Object becomes a temporary variable then we transform the LHS of the path
                context.Object = context.GetNextTemporaryVariable();
                context.Top = false;
                this._lhs.ToAlgebra(context);

                //The Subject is then the Object that results from the LHS transform since the
                //Transform may adjust the Object
                context.Subject = context.Object;

                //We then reset the Object to be the target Object so that if the RHS is the last part
                //of the Path then it will complete the path transformation
                //If it isn't the last part of the path it will be set to a new temporary variable
                context.Top = top;
                if (context.Top)
                {
                    context.ResetObject();
                }
                else
                {
                    context.Object = context.GetNextTemporaryVariable();
                }
                context.Top = top;
                this._rhs.ToAlgebra(context);
            }
            else
            {
                throw new RdfQueryException("Cannot transform a non-simple Path to an Algebra expression");
            }
        }

        public override ISparqlAlgebra ToAlgebraOperator(PathTransformContext context)
        {
            this.ToAlgebra(context);
            return context.ToAlgebra();
        }
    }
}
