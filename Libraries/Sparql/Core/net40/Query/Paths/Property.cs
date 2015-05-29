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
using VDS.RDF.Nodes;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents a Predicate which is part of a Path
    /// </summary>
    public class Property
        : IPath
    {
        private readonly INode _predicate;

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

        public bool IsTerminal
        {
            get { return true; }
        }

        public bool IsFixedLength
        {
            get { return true; }
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return formatter.Format(this.Predicate);
        }
    }
}
