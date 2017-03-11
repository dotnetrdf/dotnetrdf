/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Linq;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for creating SPARQL functional forms
    /// </summary>
    public partial class ExpressionBuilder
    {
        /// <summary>
        /// Creates a call to the BOUND function with a variable parameter
        /// </summary>
        /// <param name="var">a SPARQL variable</param>
        public BooleanExpression Bound(VariableExpression var)
        {
            return new BooleanExpression(new BoundFunction(var.Expression));
        }

        /// <summary>
        /// Creates a call to the BOUND function with a variable parameter
        /// </summary>
        /// <param name="var">a SPARQL variable name</param>
        public BooleanExpression Bound(string var)
        {
            return Bound(Variable(var));
        }

        /// <summary>
        /// Creates a call to the IF function with an expression for the first parameter
        /// </summary>
        /// <param name="ifExpression">conditional clause expression</param>
        public IfThenPart If(BooleanExpression ifExpression)
        {
            return new IfThenPart(ifExpression.Expression);
        }

        /// <summary>
        /// Creates a call to the IF function with a variable for the first parameter
        /// </summary>
        /// <param name="ifExpression">conditional clause variable expression</param>
        public IfThenPart If(VariableExpression ifExpression)
        {
            return new IfThenPart(ifExpression.Expression);
        }

        /// <summary>
        /// Creates a call of the COALESCE function with a variable number of expression parameters
        /// </summary>
        /// <param name="expressions">SPARQL expressions</param>
        public RdfTermExpression Coalesce(params SparqlExpression[] expressions)
        {
            var coalesce = new CoalesceFunction(expressions.Select(e => e.Expression));
            return new RdfTermExpression(coalesce);
        }

        /// <summary>
        /// Creates a call of the EXISTS function
        /// </summary>
        /// <param name="buildExistsPattern">a function, which will create the graph pattern parameter</param>
        public BooleanExpression Exists(Action<IGraphPatternBuilder> buildExistsPattern)
        {
            GraphPatternBuilder builder = new GraphPatternBuilder();
            buildExistsPattern(builder);
            var existsFunction = new ExistsFunction(builder.BuildGraphPattern(Prefixes), true);
            return new BooleanExpression(existsFunction);
        }

        /// <summary>
        /// Creates a call of the SAMETERM function with two expression parameters
        /// </summary>
        /// <param name="left">a SPARQL expression</param>
        /// <param name="right">a SPARQL expression</param>
        public BooleanExpression SameTerm(SparqlExpression left, SparqlExpression right)
        {
            var sameTerm = new SameTermFunction(left.Expression, right.Expression);
            return new BooleanExpression(sameTerm);
        }

        /// <summary>
        /// Creates a call of the SAMETERM function with variable and expression parameters
        /// </summary>
        /// <param name="left">a variable name</param>
        /// <param name="right">a SPARQL expression</param>
        public BooleanExpression SameTerm(string left, SparqlExpression right)
        {
            return SameTerm(Variable(left), right);
        }

        /// <summary>
        /// Creates a call of the SAMETERM function with expression and variable parameters
        /// </summary>
        /// <param name="left">a SPARQL expression</param>
        /// <param name="right">a variable name</param>
        public BooleanExpression SameTerm(SparqlExpression left, string right)
        {
            return SameTerm(left, Variable(right));
        }

        /// <summary>
        /// Creates a call of the SAMETERM function with two variable parameters
        /// </summary>
        /// <param name="left">a variable name</param>
        /// <param name="right">a variable name</param>
        public BooleanExpression SameTerm(string left, string right)
        {
            return SameTerm(Variable(left), Variable(right));
        }
    }

    /// <summary>
    /// Provides methods to supply the "then" expression for the IF function call
    /// </summary>
    public sealed class IfThenPart
    {
        private readonly ISparqlExpression _ifExpression;

        internal IfThenPart(ISparqlExpression ifExpression)
        {
            _ifExpression = ifExpression;
        }

        /// <summary>
        /// Sets the second parameter of the IF function call
        /// </summary>
        public IfElsePart Then(SparqlExpression thenExpression)
        {
            return new IfElsePart(_ifExpression, thenExpression.Expression);
        }
    }

    /// <summary>
    /// Provides methods to supply the "else" expression for the IF function call
    /// </summary>
    public sealed class IfElsePart
    {
        private readonly ISparqlExpression _ifExpression;
        private readonly ISparqlExpression _thenExpression;

        internal IfElsePart(ISparqlExpression ifExpression, ISparqlExpression thenExpression)
        {
            _ifExpression = ifExpression;
            _thenExpression = thenExpression;
        }

        /// <summary>
        /// Sets the third parameter of the IF function call
        /// </summary>
        public RdfTermExpression Else(SparqlExpression elseExpression)
        {
            var ifElseFunc = new IfElseFunction(_ifExpression, _thenExpression, elseExpression.Expression);
            return new RdfTermExpression(ifElseFunc);
        }
    }
}