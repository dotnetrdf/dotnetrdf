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
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            if (this._n > 0)
            {
                //Generate a Triple Pattern for each step in the cardinality
                for (int i = 0; i < this._n; i++)
                {
                    context.Object = context.GetNextTemporaryVariable();

                    if (i < this._n - 1 || !context.Top)
                    {
                        context.AddTriplePattern(context.GetTriplePattern(context.Subject, this._path, context.Object));
                        context.Subject = context.Object;
                    }
                    else
                    {
                        context.ResetObject();
                        context.AddTriplePattern(context.GetTriplePattern(context.Subject, this._path, context.Object));
                    }
                }

                return context.ToAlgebra();
            }
            else
            {
                return new ZeroLengthPath(context.Subject, context.Object, this._path);
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
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "*";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            return new ZeroOrMorePath(context.Subject, context.Object, this._path);
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
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "?";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            PathTransformContext lhsContext = new PathTransformContext(context);
            PathTransformContext rhsContext = new PathTransformContext(context);
            ISparqlAlgebra lhs = new ZeroLengthPath(lhsContext.Subject, lhsContext.Object, this._path);
            ISparqlAlgebra rhs = this._path.ToAlgebra(context);

            return new Union(lhs, rhs);
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
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "+";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            return new OneOrMorePath(context.Subject, context.Object, this._path);
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
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "{" + this._n + ",}";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            PatternItem tempVar = context.GetNextTemporaryVariable();
            context.AddTriplePattern(new PropertyPathPattern(context.Subject, new FixedCardinality(this._path, this._n), tempVar));
            context.AddTriplePattern(new PropertyPathPattern(tempVar, new ZeroOrMore(this._path), context.Object));
            return context.ToAlgebra();
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
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "{," + this._n + "}";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            context.AddTriplePattern(new PropertyPathPattern(context.Subject, new NToM(this._path, 0, this._n), context.Object));
            return context.ToAlgebra();
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
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._path.ToString() + "{" + this._n + "," + this._m + "}";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            ISparqlAlgebra complex = null;
            int i = this._n;
            while (i <= this._m)
            {
                PathTransformContext tempContext = new PathTransformContext(context);
                tempContext.AddTriplePattern(new PropertyPathPattern(context.Subject, new FixedCardinality(this._path, i), context.Object));
                if (complex == null)
                {
                    complex = tempContext.ToAlgebra();
                }
                else
                {
                    complex = new Union(complex, tempContext.ToAlgebra());
                }
                i++;
            }
            return complex;
        }
    }
}
