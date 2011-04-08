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
using System.Security.Cryptography;
using HashLib;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Abstract base class for Hash Functions
    /// </summary>
    public abstract class BaseHashFunction : BaseUnaryExpression
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
            this._crypto = hash;
        }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                switch (temp.NodeType)
                {
                    case NodeType.Blank:
                        throw new RdfQueryException("Cannot calculate the Hash of a Blank Node");
                    case NodeType.GraphLiteral:
                        throw new RdfQueryException("Cannot calculate the Hash of a Graph Literal");
                    case NodeType.Literal:
                        return new LiteralNode(null, this.Hash(((ILiteralNode)temp).Value));
                    case NodeType.Uri:
                        return new LiteralNode(null, this.Hash(temp.ToString()));
                    default:
                        throw new RdfQueryException("Cannot calculate the Hash of an Unknown Node Type");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot calculate the SHA 1 Sum of a null");
            }
        }

        /// <summary>
        /// Gets the effective boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Computes Hashes
        /// </summary>
        /// <param name="input">Input String</param>
        /// <returns></returns>
        protected virtual String Hash(String input)
        {
            Byte[] inputBytes, hashBytes;
            StringBuilder output = new StringBuilder();

            inputBytes = Encoding.UTF8.GetBytes(input);
            hashBytes = this._crypto.ComputeHash(inputBytes);

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

    /// <summary>
    /// Abstract base class for Hash Functions that use the parts of the HashLib that are integrated into dotNetRDF
    /// </summary>
    public abstract class BaseHashLibFunction : BaseUnaryExpression
    {
        private HashCryptoNotBuildIn _crypto;

        /// <summary>
        /// Creates a new Hash function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="hash">Hash Algorithm to use</param>
        public BaseHashLibFunction(ISparqlExpression expr, HashCryptoNotBuildIn hash)
            : base(expr)
        {
            this._crypto = hash;
        }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                switch (temp.NodeType)
                {
                    case NodeType.Blank:
                        throw new RdfQueryException("Cannot calculate the Hash of a Blank Node");
                    case NodeType.GraphLiteral:
                        throw new RdfQueryException("Cannot calculate the Hash of a Graph Literal");
                    case NodeType.Literal:
                        return new LiteralNode(null, this.Hash(((ILiteralNode)temp).Value));
                    case NodeType.Uri:
                        return new LiteralNode(null, this.Hash(temp.ToString()));
                    default:
                        throw new RdfQueryException("Cannot calculate the Hash of an Unknown Node Type");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot calculate the SHA 1 Sum of a null");
            }
        }

        /// <summary>
        /// Gets the effective boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Computes Hashes
        /// </summary>
        /// <param name="input">Input String</param>
        /// <returns></returns>
        protected virtual String Hash(String input)
        {
            HashResult r = this._crypto.ComputeString(input, Encoding.UTF8);
            return r.ToString().Replace("-","").ToLower();
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
