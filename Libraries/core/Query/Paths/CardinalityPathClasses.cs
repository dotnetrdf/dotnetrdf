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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents a Cardinality restriction on a Path
    /// </summary>
    public abstract class Cardinality : BaseUnaryPath
    {
        /// <summary>
        /// Creates a new Cardinality Restriction
        /// </summary>
        /// <param name="path">Path</param>
        public Cardinality(ISparqlPath path)
            : base(path) { }

        /// <summary>
        /// Gets whether the path is simple
        /// </summary>
        public override abstract bool IsSimple
        {
            get;
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public abstract int MinCardinality
        {
            get;
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public abstract int MaxCardinality
        {
            get;
        }

        /// <summary>
        /// Evaluates the Path in the given Context
        /// </summary>
        /// <param name="context">Context</param>
        public abstract override void Evaluate(PathEvaluationContext context);

        /// <summary>
        /// Throws an error since non-simple Paths cannot be transformed to Algebra expressions
        /// </summary>
        /// <param name="context">Transform Context</param>
        public override void ToAlgebra(PathTransformContext context)
        {
            throw new RdfQueryException("Cannot transform a non-simple Path to an Algebra expression");
        }
    }

    /// <summary>
    /// Represents a Fixed Cardinality restriction on a Path
    /// </summary>
    public class FixedCardinality : Cardinality
    {
        private int _n;

        /// <summary>
        /// Creates a new Fixed Cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="n">N</param>
        public FixedCardinality(ISparqlPath path, int n)
            : base(path)
        {
            this._n = n;
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return this._n; 
            }
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public override int MinCardinality
        {
            get 
            {
                return this._n; 
            }
        }

        /// <summary>
        /// Evaluates the Path in the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public override void Evaluate(PathEvaluationContext context)
        {
            int c = 0, i = 0;
            //Evaluate the path as many times
            do
            {
                c = context.Paths.Count;
                this._path.Evaluate(context);

                //If we've not reached the required number of steps yet clear any complete paths
                if (i < this._n) context.CompletePaths.Clear();
                i++;

                //If we've reached the required number of steps we can stop
                if (i >= this._n) break;
            } while (c < context.Paths.Count);

            //If we've failed to reach sufficient path length to meet the cardinality requirement then
            //we'll also clear incomplete paths as we now can't make any valid paths
            if (i < this._n) context.Paths.Clear();
        }

        /// <summary>
        /// Returns true since fixed cardinalities are simple
        /// </summary>
        public override bool IsSimple
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Generates the Path transform to an Algebra expression
        /// </summary>
        /// <param name="context">Transform Context</param>
        public override void ToAlgebra(PathTransformContext context)
        {
            if (this._path.IsSimple && this._path is Property)
            {
                NodeMatchPattern nodeMatch = new NodeMatchPattern(((Property)this._path).Predicate);

                //Generate a Triple Pattern for each step in the cardinality
                for (int i = 0; i < this._n; i++)
                {
                    context.Object = context.GetNextTemporaryVariable();

                    if (i < this._n - 1 || !context.Top)
                    {
                        TriplePattern p = new TriplePattern(context.Subject, nodeMatch, context.Object);
                        context.AddTriplePattern(p);
                        context.Subject = context.Object;
                    }
                    else
                    {
                        context.ResetObject();
                        TriplePattern p = new TriplePattern(context.Subject, nodeMatch, context.Object);
                        context.AddTriplePattern(p);
                    }
                }

            }
            else
            {
                throw new RdfQueryException("Cannot transform a non-simple Path to an Algebra expression");
            }
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "{" + this._n + "}";
        }
    }

    /// <summary>
    /// Represents a Zero or More cardinality restriction on a Path
    /// </summary>
    public class ZeroOrMore : Cardinality
    {
        /// <summary>
        /// Creates a new Zero or More cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        public ZeroOrMore(ISparqlPath path)
            : base(path) { }

        /// <summary>
        /// Zero or More cardinality restrictions are not simple
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns that this Path allows zero-length paths
        /// </summary>
        public override bool AllowsZeroLength
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return Int32.MaxValue;
            }
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public override int MinCardinality
        {
            get 
            {
                return 0;
            }
        }

        /// <summary>
        /// Evaluates the Path in the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public override void Evaluate(PathEvaluationContext context)
        {
            int c = 0;
            //Evaluate as many times as we can while we are finding more paths
            do
            {
                c = context.Paths.Count;
                this._path.Evaluate(context);
            } while (c < context.Paths.Count);
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "*";
        }
    }

    /// <summary>
    /// Represents a Zero or One cardinality restriction on a Path
    /// </summary>
    public class ZeroOrOne : Cardinality
    {
        /// <summary>
        /// Creates a new Zero or One cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        public ZeroOrOne(ISparqlPath path)
            : base(path) { }

        /// <summary>
        /// Zero or One cardinality restrictions are not simple
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns that this Path allows zero-length paths
        /// </summary>
        public override bool AllowsZeroLength
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return 1; 
            }
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public override int MinCardinality
        {
            get 
            {
                return 0; 
            }
        }

        /// <summary>
        /// Evaluates the Path in the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public override void Evaluate(PathEvaluationContext context)
        {
            //Just evaluate once
            this._path.Evaluate(context);
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "?";
        }
    }

    /// <summary>
    /// Represents a One or More cardinality restriction on a Path
    /// </summary>
    public class OneOrMore : Cardinality
    {
        /// <summary>
        /// Creates a new One or More cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        public OneOrMore(ISparqlPath path)
            : base(path) { }

        /// <summary>
        /// One or More cardinality restrictions are not simple
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return Int32.MaxValue;
            }
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public override int MinCardinality
        {
            get 
            {
                return 1;
            }
        }

        /// <summary>
        /// Evalutes the Path in the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public override void Evaluate(PathEvaluationContext context)
        {
            int c = 0;
            //Evaluate as many times as we can while we are finding more paths
            do
            {
                c = context.Paths.Count;
                this._path.Evaluate(context);
            } while (c < context.Paths.Count);
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "+";
        }
    }

    /// <summary>
    /// Represents a N or More cardinality restriction on a Path
    /// </summary>
    public class NOrMore : Cardinality
    {
        private int _n;

        /// <summary>
        /// Creates a new N or More cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="n">Minimum Cardinality</param>
        public NOrMore(ISparqlPath path, int n)
            : base(path) 
        {
            this._n = n;
        }

        /// <summary>
        /// N or More cardinality restrictions are not simple
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return Int32.MaxValue;
            }
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public override int MinCardinality
        {
            get 
            {
                return this._n;
            }
        }

        /// <summary>
        /// Evaluates the Path in the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public override void Evaluate(PathEvaluationContext context)
        {
            int c = 0, i = 0;
            //Evaluate as many times as we can while we are finding more paths
            do
            {
                //If we haven't reached the minimum cardinality yet we'll clear complete paths
                if (i < this._n) context.CompletePaths.Clear();
                c = context.Paths.Count;
                this._path.Evaluate(context);
                i++;
            } while (c < context.Paths.Count);
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "{" + this._n + ",}";
        }
    }

    /// <summary>
    /// Represents a Zero to N cardinality restriction on a Path
    /// </summary>
    public class ZeroToN : Cardinality
    {
        private int _n;

        /// <summary>
        /// Creates a new Zero to N cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="n">Maximum Cardinality</param>
        public ZeroToN(ISparqlPath path, int n)
            : base(path) 
        {
            this._n = n;
        }

        /// <summary>
        /// Zero to N cardinality restrictions are not simple
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return this._n;
            }
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public override int MinCardinality
        {
            get 
            {
                return 0; 
            }
        }

        /// <summary>
        /// Returns that this Path allows zero-length paths
        /// </summary>
        public override bool AllowsZeroLength
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Evaluates the Path in the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public override void Evaluate(PathEvaluationContext context)
        {
            int c = 0, i = 0;
            //Evaluate as many times as we can while we are finding more paths
            do
            {
                c = context.Paths.Count;
                this._path.Evaluate(context);
                i++;
                //If we've reached the max cardinality then we can stop
                if (i >= this._n) break;
            } while (c < context.Paths.Count);
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "{," + this._n + "}";
        }
    }

    /// <summary>
    /// Represents a N to M cardinality restriction on a Path
    /// </summary>
    public class NToM : Cardinality
    {
        private int _n, _m;

        /// <summary>
        /// Creates a new N to M cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="n">Minimum Cardinality</param>
        /// <param name="m">Maximum Cardinality</param>
        public NToM(ISparqlPath path, int n, int m)
            : base(path)
        {
            this._n = n;
            this._m = m;
        }

        /// <summary>
        /// N to M cardinality restrictions are not simple
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return this._m; 
            }
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public override int MinCardinality
        {
            get 
            {
                return this._n;
            }
        }

        /// <summary>
        /// Gets whether the Path allows Zero Length Paths
        /// </summary>
        public override bool AllowsZeroLength
        {
            get
            {
                return (this._n == 0);
            }
        }

        /// <summary>
        /// Evaluates the Path in the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public override void Evaluate(PathEvaluationContext context)
        {
            int c = 0, i = 0;
            //Evaluate as many times as we can while we are finding more paths
            do
            {
                //If we haven't reached the minimum cardinality yet then we'll clear completed paths
                if (i < this._n) context.CompletePaths.Clear();
                c = context.Paths.Count;
                this._path.Evaluate(context);
                i++;

                //If we've reached the maximum cardinality then we'll stop
                if (i >= this._m) break;
            } while (c < context.Paths.Count);
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "{" + this._n + "," + this._m + "}";
        }
    }
}
