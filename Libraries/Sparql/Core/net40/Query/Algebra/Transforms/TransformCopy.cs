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

namespace VDS.RDF.Query.Algebra.Transforms
{
    public class TransformCopy
        : IAlgebraTransform
    {
        public virtual IAlgebra Transform(Bgp bgp)
        {
            return bgp.Copy(bgp.TriplePatterns);
        }

        public virtual IAlgebra Transform(Table table)
        {
            return table.Copy(table.Data);
        }

        public virtual IAlgebra Transform(Slice slice, IAlgebra transformedInnerAlgebra)
        {
            return slice.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(NamedGraph namedGraph, IAlgebra transformedInnerAlgebra)
        {
            return namedGraph.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(Filter filter, IAlgebra transformedInnerAlgebra)
        {
            return filter.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(Distinct distinct, IAlgebra transformedInnerAlgebra)
        {
            return distinct.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(Reduced reduced, IAlgebra transformedInnerAlgebra)
        {
            return reduced.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(Project project, IAlgebra transformedInnerAlgebra)
        {
            return project.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(OrderBy orderBy, IAlgebra transformedInnerAlgebra)
        {
            return orderBy.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(Extend extend, IAlgebra transformedInnerAlgebra)
        {
            return extend.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(GroupBy groupBy, IAlgebra transformedInnerAlgebra)
        {
            return groupBy.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(Service service, IAlgebra transformedInnerAlgebra)
        {
            return service.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(PropertyPath path, IAlgebra transformedInnerAlgebra)
        {
            return path.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(TopN topN, IAlgebra transformedInnerAlgebra)
        {
            return topN.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(PropertyFunction propertyFunction, IAlgebra transformedInnerAlgebra)
        {
            return propertyFunction.Copy(transformedInnerAlgebra);
        }

        public virtual IAlgebra Transform(Union union, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            return union.Copy(transformedLhs, transformedRhs);
        }

        public virtual IAlgebra Transform(Join join, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            return join.Copy(transformedLhs, transformedRhs);
        }

        public virtual IAlgebra Transform(LeftJoin leftJoin, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            return leftJoin.Copy(transformedLhs, transformedRhs);
        }

        public virtual IAlgebra Transform(Minus minus, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            return minus.Copy(transformedLhs, transformedRhs);
        }
    }
}
