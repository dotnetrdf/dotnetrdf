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

using System.Text;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents Alternative Paths
    /// </summary>
    public class AlternativePath 
        : BaseBinaryPath
    {
        /// <summary>
        /// Creates a new Alternative Path
        /// </summary>
        /// <param name="lhs">LHS Path</param>
        /// <param name="rhs">RHS Path</param>
        public AlternativePath(IPath lhs, IPath rhs)
            : base(lhs, rhs) { }

        public override bool IsTerminal
        {
            get { return false; }
        }

        public override bool IsFixedLength
        {
            get { return this._lhs.IsFixedLength && this._rhs.IsFixedLength; }
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("alt ");
            if (!this._lhs.IsTerminal) builder.Append('(');
            builder.Append(this._lhs.ToString(formatter));
            if (!this._lhs.IsTerminal) builder.Append(')');
            builder.Append(' ');
            if (!this._rhs.IsTerminal) builder.Append('(');
            builder.Append(this._rhs.ToString(formatter));
            if (!this._rhs.IsTerminal) builder.Append(')');
            return builder.ToString();
        }
    }
}
