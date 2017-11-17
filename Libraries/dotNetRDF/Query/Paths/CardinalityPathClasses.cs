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
            _n = n;
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return _n; 
            }
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public override int MinCardinality
        {
            get 
            {
                return _n; 
            }
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            if (_n > 0)
            {
                // Generate a Triple Pattern for each step in the cardinality
                for (int i = 0; i < _n; i++)
                {
                    context.Object = context.GetNextTemporaryVariable();

                    if (i < _n - 1 || !context.Top)
                    {
                        context.AddTriplePattern(context.GetTriplePattern(context.Subject, _path, context.Object));
                        context.Subject = context.Object;
                    }
                    else
                    {
                        context.ResetObject();
                        context.AddTriplePattern(context.GetTriplePattern(context.Subject, _path, context.Object));
                    }
                }

                return context.ToAlgebra();
            }
            else
            {
                return new ZeroLengthPath(context.Subject, context.Object, _path);
            }
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _path.ToString() + "{" + _n + "}";
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
            return _path.ToString() + "*";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            return new ZeroOrMorePath(context.Subject, context.Object, _path);
        }
    }

    /// <summary>
    /// Represents a Zero or One cardinality restriction on a Path
    /// </summary>
    public class ZeroOrOne
        : Cardinality
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
            return _path.ToString() + "?";
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
            ISparqlAlgebra lhs = new ZeroLengthPath(lhsContext.Subject, lhsContext.Object, _path);
            ISparqlAlgebra rhs = _path.ToAlgebra(rhsContext);

            return new Distinct(new Union(lhs, rhs));
        }
    }

    /// <summary>
    /// Represents a One or More cardinality restriction on a Path
    /// </summary>
    public class OneOrMore
        : Cardinality
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
            return _path.ToString() + "+";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            return new OneOrMorePath(context.Subject, context.Object, _path);
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
            _n = n;
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
                return _n;
            }
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _path.ToString() + "{" + _n + ",}";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            PatternItem tempVar = context.GetNextTemporaryVariable();
            context.AddTriplePattern(new PropertyPathPattern(context.Subject, new FixedCardinality(_path, _n), tempVar));
            context.AddTriplePattern(new PropertyPathPattern(tempVar, new ZeroOrMore(_path), context.Object));
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
            _n = n;
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return _n;
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
            return _path.ToString() + "{," + _n + "}";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            context.AddTriplePattern(new PropertyPathPattern(context.Subject, new NToM(_path, 0, _n), context.Object));
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
            _n = n;
            _m = m;
        }

        /// <summary>
        /// Gets the Maximum Cardinality of the Path
        /// </summary>
        public override int MaxCardinality
        {
            get 
            {
                return _m; 
            }
        }

        /// <summary>
        /// Gets the Minimum Cardinality of the Path
        /// </summary>
        public override int MinCardinality
        {
            get 
            {
                return _n;
            }
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _path.ToString() + "{" + _n + "," + _m + "}";
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            ISparqlAlgebra complex = null;
            int i = _n;
            while (i <= _m)
            {
                PathTransformContext tempContext = new PathTransformContext(context);
                tempContext.AddTriplePattern(new PropertyPathPattern(context.Subject, new FixedCardinality(_path, i), context.Object));
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
