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

using System.Collections.Generic;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Expressions.Transforms
{
    /// <summary>
    /// An expression transform that simply copies expressions
    /// </summary>
    public class ExprTransformCopy
        : IExpressionTransform
    {
        public virtual IExpression Transform(INullaryExpression expression)
        {
            return expression.Copy();
        }

        public virtual IExpression Transform(IUnaryExpression expression, IExpression transformedInnerExpression)
        {
            return expression.Copy(transformedInnerExpression);
        }

        public virtual IExpression Transform(IBinaryExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument)
        {
            return expression.Copy(transformedFirstArgument, transformedSecondArgument);
        }

        public virtual IExpression Transform(ITernayExpression expression, IExpression transformedFirstArgument, IExpression transformedSecondArgument, IExpression transformedThirdArgument)
        {
            return expression.Copy(transformedFirstArgument, transformedSecondArgument, transformedThirdArgument);
        }

        public virtual IExpression Transform(INAryExpression expression, IEnumerable<IExpression> transformedArguments)
        {
            return expression.Copy(transformedArguments);
        }

        public virtual IExpression Transform(IAggregateExpression expression, IEnumerable<IExpression> transformedArguments)
        {
            return expression.Copy(transformedArguments);
        }

        public virtual IExpression Transform(IAlgebraExpression expression, IAlgebra transformedAlgebra)
        {
            return expression.Copy(transformedAlgebra);
        }
    }
}
