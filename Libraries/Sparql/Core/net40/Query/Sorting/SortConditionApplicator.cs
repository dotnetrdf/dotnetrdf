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
using System.Linq;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Sorting
{
    /// <summary>
    /// Comparer which applies a sequence of sort conditions
    /// </summary>
    public class SortConditionApplicator
        : IComparer<ISolution>
    {
        public SortConditionApplicator(IEnumerable<ISortCondition> conditions, IExpressionContext context)
        {
            if (conditions == null) throw new ArgumentNullException("conditions");
            if (context == null) throw new ArgumentNullException("context");
            this.SortConditions = conditions.ToList();
            this.SortComparers = this.SortConditions.Select(c => c.CreateComparer(context)).ToList();
        }

        private IList<ISortCondition> SortConditions { get; set; }

        private IList<IComparer<ISolution>> SortComparers { get; set; } 

        public int Compare(ISolution x, ISolution y)
        {
            int c = 0;
            for (int i = 0; i < this.SortComparers.Count; i++)
            {
                c = this.SortComparers[i].Compare(x, y);
                if (c != 0) break;
            }
            return c;
        }
    }
}
