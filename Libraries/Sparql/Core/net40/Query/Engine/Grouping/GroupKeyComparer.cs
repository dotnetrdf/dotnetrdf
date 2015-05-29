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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine.Grouping
{
    public class GroupKeyComparer
        : IEqualityComparer<ISolution>
    {
        public GroupKeyComparer(IEnumerable<String> groupKeyVars)
        {
            if (groupKeyVars == null) throw new ArgumentNullException("groupKeyVars");
            this.GroupKeyVariables = groupKeyVars.ToList();
        }

        private IList<String> GroupKeyVariables { get; set; }

        public bool Equals(ISolution x, ISolution y)
        {
            if (this.GroupKeyVariables.Count == 0) return true;

            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;

            foreach (String keyVar in this.GroupKeyVariables)
            {
                INode xNode = x[keyVar];
                INode yNode = y[keyVar];

                if (!EqualityHelper.AreNodesEqual(xNode, yNode)) return false;
                if (xNode == null && yNode == null) return true;
            }

            return true;
        }

        public int GetHashCode(ISolution obj)
        {
            if (this.GroupKeyVariables.Count == 0) return 0;

            INode n = obj[this.GroupKeyVariables[0]];
            if (n == null) return 0;
            int hashCode = n.GetHashCode();

            for (int i = 1; i < this.GroupKeyVariables.Count; i++)
            {
                n = obj[this.GroupKeyVariables[i]];
                if (n == null) return hashCode;
                hashCode = Tools.CombineHashCodes(hashCode, n);
            }
            return hashCode;
        }
    }
}