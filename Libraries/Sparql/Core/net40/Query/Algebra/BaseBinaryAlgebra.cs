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
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public abstract class BaseBinaryAlgebra
        : IBinaryAlgebra
    {
        protected BaseBinaryAlgebra(IAlgebra lhs, IAlgebra rhs)
        {
            if (lhs == null) throw new ArgumentNullException("lhs");
            if (rhs == null) throw new ArgumentNullException("rhs");

            this.Lhs = lhs;
            this.Rhs = rhs;
        }

        public IAlgebra Lhs { get; private set; }

        public IAlgebra Rhs { get; private set; }

        public IAlgebra Copy()
        {
            return this.Copy(this.Lhs.Copy(), this.Rhs.Copy());
        }

        public abstract IAlgebra Copy(IAlgebra lhs, IAlgebra rhs);

        public abstract IEnumerable<string> ProjectedVariables { get; }

        public abstract IEnumerable<string> FixedVariables { get; }

        public abstract IEnumerable<string> FloatingVariables { get; }

        public abstract void Accept(IAlgebraVisitor visitor);

        public abstract IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context);

        public abstract bool Equals(IAlgebra other);

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public abstract string ToString(IAlgebraFormatter formatter);
    }
}