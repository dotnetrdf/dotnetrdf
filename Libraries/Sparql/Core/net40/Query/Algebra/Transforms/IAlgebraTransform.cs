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
    /// <summary>
    /// Interface for algebra transforms
    /// </summary>
    public interface IAlgebraTransform
    {
        IAlgebra Transform(Bgp bgp);

        IAlgebra Transform(Table table);

        IAlgebra Transform(Slice slice, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(NamedGraph namedGraph, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Filter filter, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Distinct distinct, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Reduced reduced, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Project project, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(OrderBy orderBy, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Extend extend, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(GroupBy groupBy, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Service service, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(PropertyPath path, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(TopN topN, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(PropertyFunction propertyFunction, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Union union, IAlgebra transformedLhs, IAlgebra transformedRhs);

        IAlgebra Transform(Join join, IAlgebra transformedLhs, IAlgebra transformedRhs);

        IAlgebra Transform(LeftJoin leftJoin, IAlgebra transformedLhs, IAlgebra transformedRhs);

        IAlgebra Transform(Minus minus, IAlgebra transformedLhs, IAlgebra transformedRhs);
    }
}
