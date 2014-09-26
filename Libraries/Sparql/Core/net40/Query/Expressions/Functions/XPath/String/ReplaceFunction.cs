/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:replace() function
    /// </summary>
    public class ReplaceFunction
        : Sparql.String.ReplaceFunction
    {
        /// <summary>
        /// Creates a new XPath Replace function
        /// </summary>
        /// <param name="text">Text Expression</param>
        /// <param name="find">Search Expression</param>
        /// <param name="replace">Replace Expression</param>
        public ReplaceFunction(IExpression text, IExpression find, IExpression replace)
            : this(text, find, replace, null) {}

        /// <summary>
        /// Creates a new XPath Replace function
        /// </summary>
        /// <param name="text">Text Expression</param>
        /// <param name="find">Search Expression</param>
        /// <param name="replace">Replace Expression</param>
        /// <param name="options">Options Expression</param>
        public ReplaceFunction(IExpression text, IExpression find, IExpression replace, IExpression options)
            : base(text, find, replace, options) {}

        protected override IValuedNode CreateOutputNode(INode lit, string output)
        {
            return new StringNode(output, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Replace; }
        }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> argList = args.ToList();
            return argList.Count == 3 ? new ReplaceFunction(argList[0], argList[1], argList[2]) : new ReplaceFunction(argList[0], argList[1], argList[2], argList[3]);
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is ReplaceFunction)) return false;

            ReplaceFunction func = (ReplaceFunction)other;
            if (this.Arguments.Count != func.Arguments.Count) return false;
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (!this.Arguments[i].Equals(func.Arguments[i])) return false;
            }
            return true;
        }
    }
}