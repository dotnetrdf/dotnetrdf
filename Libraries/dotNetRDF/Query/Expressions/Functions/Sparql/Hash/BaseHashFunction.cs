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
using System.Security.Cryptography;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Hash
{
    /// <summary>
    /// Abstract base class for Hash Functions
    /// </summary>
    public abstract class BaseHashFunction 
        : BaseUnaryExpression
    {
        private HashAlgorithm _crypto;

        /// <summary>
        /// Creates a new Hash function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="hash">Hash Algorithm to use</param>
        public BaseHashFunction(ISparqlExpression expr, HashAlgorithm hash)
            : base(expr)
        {
            _crypto = hash;
        }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = _expr.Evaluate(context, bindingID);
            if (temp != null)
            {
                switch (temp.NodeType)
                {
                    case NodeType.Blank:
                        throw new RdfQueryException("Cannot calculate the Hash of a Blank Node");
                    case NodeType.GraphLiteral:
                        throw new RdfQueryException("Cannot calculate the Hash of a Graph Literal");
                    case NodeType.Literal:
                        return new StringNode(null, Hash(((ILiteralNode)temp).Value));
                    case NodeType.Uri:
                        return new StringNode(null, Hash(temp.AsString()));
                    default:
                        throw new RdfQueryException("Cannot calculate the Hash of an Unknown Node Type");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot calculate the Hash of a null");
            }
        }

        /// <summary>
        /// Computes Hashes
        /// </summary>
        /// <param name="input">Input String</param>
        /// <returns></returns>
        protected virtual string Hash(string input)
        {
            Byte[] inputBytes, hashBytes;
            StringBuilder output = new StringBuilder();

            inputBytes = Encoding.UTF8.GetBytes(input);
            hashBytes = _crypto.ComputeHash(inputBytes);

            for (int i = 0; i < hashBytes.Length; i++)
            {
                output.Append(hashBytes[i].ToString("x2"));
            }

            return output.ToString();
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }
    }
}
