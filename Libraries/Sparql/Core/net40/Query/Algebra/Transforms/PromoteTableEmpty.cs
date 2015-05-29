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
    public class PromoteTableEmpty
        : TransformCopy
    {
        public override IAlgebra Transform(Join join, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            // If either side is table empty then join will produce empty result
            if (IsTableEmpty(transformedLhs) || IsTableEmpty(transformedRhs)) return Table.CreateEmpty();

            return base.Transform(join, transformedLhs, transformedRhs);
        }

        public override IAlgebra Transform(LeftJoin leftJoin, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            // If LHS is table empty whole join will produce empty result
            if (IsTableEmpty(transformedLhs)) return Table.CreateEmpty();

            // If RHS is table empty then only preserve LHS
            if (IsTableEmpty(transformedRhs)) return transformedLhs;

            return base.Transform(leftJoin, transformedLhs, transformedRhs);
        }

        public override IAlgebra Transform(Union union, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            // If LHS is table empty then may only need to preserve RHS
            if (IsTableEmpty(transformedLhs))
            {
                // If RHS is also table empty then union will produce empty result
                if (IsTableEmpty(transformedRhs)) return Table.CreateEmpty();

                // Otherwise just preserve RHS
                return transformedRhs;
            }
            
            // If RHS is table empty then only need to preserve LHS
            if (IsTableEmpty(transformedRhs)) return transformedLhs;

            return base.Transform(union, transformedLhs, transformedRhs);
        }

        private static bool IsTableEmpty(IAlgebra algebra)
        {
            Table t = algebra as Table;
            return t != null && t.IsEmpty;
        }
    }
}