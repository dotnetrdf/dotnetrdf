/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents a Cardinality restriction on a Path
    /// </summary>
    public abstract class Cardinality 
        : BaseUnaryPath
    {
        /// <summary>
        /// Creates a new Cardinality Restriction
        /// </summary>
        /// <param name="path">Path</param>
        protected Cardinality(IPath path)
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

        public override bool IsFixedLength
        {
            get { return this.MinCardinality == this.MaxCardinality; }
        }

        public override bool IsTerminal
        {
            get { return false; }
        }
    }

    /// <summary>
    /// Represents a Fixed Cardinality restriction on a Path
    /// </summary>
    public class FixedCardinality
        : Cardinality
    {
        private readonly int _n;

        /// <summary>
        /// Creates a new Fixed Cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="n">N</param>
        public FixedCardinality(IPath path, int n)
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
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("pathN ");
            builder.Append(this._n);
            builder.Append(' ');
            if (!this._path.IsTerminal) builder.Append('(');
            builder.Append(this.Path.ToString(formatter));
            if (!this._path.IsTerminal) builder.Append(')');
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a Zero or More cardinality restriction on a Path
    /// </summary>
    public class ZeroOrMore 
        : Cardinality
    {
        /// <summary>
        /// Creates a new Zero or More cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        public ZeroOrMore(IPath path)
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
        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("path* ");
            if (!this.Path.IsTerminal) builder.Append('(');
            builder.Append(this.Path.ToString(formatter));
            if (!this.Path.IsTerminal) builder.Append(')');
            return builder.ToString();
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
        public ZeroOrOne(IPath path)
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
        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("path? ");
            if (!this.Path.IsTerminal) builder.Append('(');
            builder.Append(this.Path.ToString(formatter));
            if (!this.Path.IsTerminal) builder.Append(')');
            return builder.ToString();
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
        public OneOrMore(IPath path)
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
        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("path+ ");
            if (!this.Path.IsTerminal) builder.Append('(');
            builder.Append(this.Path.ToString(formatter));
            if (!this.Path.IsTerminal) builder.Append(')');
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a N or More cardinality restriction on a Path
    /// </summary>
    public class NOrMore 
        : Cardinality
    {
        private readonly int _n;

        /// <summary>
        /// Creates a new N or More cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="n">Minimum Cardinality</param>
        public NOrMore(IPath path, int n)
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
        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("mod ");
            builder.Append(this.MinCardinality);
            builder.Append(" _ ");
            if (!this.Path.IsTerminal) builder.Append('(');
            builder.Append(this.Path.ToString(formatter));
            if (!this.Path.IsTerminal) builder.Append(')');
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a Zero to N cardinality restriction on a Path
    /// </summary>
    public class ZeroToN : Cardinality
    {
        private readonly int _n;

        /// <summary>
        /// Creates a new Zero to N cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="n">Maximum Cardinality</param>
        public ZeroToN(IPath path, int n)
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
        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("mod ");
            builder.Append(this.MinCardinality);
            builder.Append(' ');
            builder.Append(this.MaxCardinality);
            builder.Append(' ');
            if (!this.Path.IsTerminal) builder.Append('(');
            builder.Append(this.Path.ToString(formatter));
            if (!this.Path.IsTerminal) builder.Append(')');
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a N to M cardinality restriction on a Path
    /// </summary>
// ReSharper disable once InconsistentNaming
    public class NToM 
        : Cardinality
    {
        private readonly int _n, _m;

        /// <summary>
        /// Creates a new N to M cardinality restriction
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="n">Minimum Cardinality</param>
        /// <param name="m">Maximum Cardinality</param>
        public NToM(IPath path, int n, int m)
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
        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("mod ");
            builder.Append(this.MinCardinality);
            builder.Append(' ');
            builder.Append(this.MaxCardinality);
            builder.Append(' ');
            if (!this.Path.IsTerminal) builder.Append('(');
            builder.Append(this.Path.ToString(formatter));
            if (!this.Path.IsTerminal) builder.Append(')');
            return builder.ToString();
        }
    }
}
