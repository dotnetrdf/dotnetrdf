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
using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Interface for SPARQL Expression Terms that can be used in Expression Trees while evaluating Sparql Queries
    /// </summary>
    public interface IExpression
        : IEquatable<IExpression>
    {
        /// <summary>
        /// Evalutes a SPARQL Expression for the given binding in a given context
        /// </summary>
        /// <param name="set">Set the expression is evaluated on</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        IValuedNode Evaluate(ISolution set, IExpressionContext context);

        /// <summary>
        /// Gets an enumeration of all the Variables used in an expression
        /// </summary>
        IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Function Name or Operator Symbol - function names may be URIs of Keywords.  If this is not an operator or function then null is returned
        /// </summary>
        ///<returns>An operator symbol, function keyword or URI if an operator/function.  Null otherwise</returns>
        String Functor
        {
            get;
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        bool CanParallelise
        {
            get;
        }

        /// <summary>
        /// Gets whether an expression is deterministic i.e. guarantees to produce the a specific output when given a specific input
        /// </summary>
        bool IsDeterministic { get; }

        /// <summary>
        /// Gets whether the expression represents a constant
        /// </summary>
        bool IsConstant { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitor"></param>
        void Accept(IExpressionVisitor visitor);

        String ToString();

        String ToString(IAlgebraFormatter formatter);

        String ToPrefixString();

        String ToPrefixString(IAlgebraFormatter formatter);

        /// <summary>
        /// Makes a copy of the expression
        /// </summary>
        /// <returns>Copy of the expression</returns>
        IExpression Copy();
    }

    public interface INullaryExpression
        : IExpression
    {
        
    }

    public interface IUnaryExpression
        : IExpression
    {
        IExpression Argument { get; }

        IExpression Copy(IExpression argument);
    }

    public interface IBinaryExpression
        : IExpression
    {
        IExpression FirstArgument { get; }

        IExpression SecondArgument { get; }

        IExpression Copy(IExpression arg1, IExpression arg2);
    }

    public interface ITernayExpression
        : IExpression
    {
        IExpression FirstArgument { get; }

        IExpression SecondArgument { get; }

        IExpression ThirdArgument { get; }

        IExpression Copy(IExpression arg1, IExpression arg2, IExpression arg3);
    }

    public interface INAryExpression
        : IExpression
    {
        IList<IExpression> Arguments { get; }

        IExpression Copy(IEnumerable<IExpression> arguments);
    }

    public interface IAlgebraExpression
        : IExpression
    {
        IAlgebra Algebra { get; }

        IExpression Copy(IAlgebra algebra);
    }

    public interface IAggregateExpression
        : INAryExpression
    {
        IAccumulator CreateAccumulator();
    }
}