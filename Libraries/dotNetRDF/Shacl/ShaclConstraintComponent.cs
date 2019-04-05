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

namespace VDS.RDF.Shacl
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal class ShaclConstraintComponent : WrapperNode
    {
        [DebuggerStepThrough]
        internal ShaclConstraintComponent(INode node)
            : base(node)
        {
        }

        internal IEnumerable<ShaclParameter> Parameters =>
            from parameter in Shacl.Parameter.ObjectsOf(this)
            select new ShaclParameter(parameter);

        internal bool Matches(ShaclShape shape) => Parameters.All(p => p.Optional || p.Matches(shape));

        internal IEnumerable<ShaclConstraint> Constraints(ShaclShape shape)
        {

            return
                CartesianProduct(
                    from p in this.Parameters
                    let path = p.Path
                    select
                        from o in path.ObjectsOf(shape)
                        select new KeyValuePair<string, INode>(path.ToString().Split('/','#').Last(), o))
                .Select(
                    x => new ShaclComponentConstraint(shape, this, x));
        }

        // See https://web.archive.org/web/20190405023324/https://ericlippert.com/2010/06/28/computing-a-cartesian-product-with-linq/
        private static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item }));
        }
    }
}
