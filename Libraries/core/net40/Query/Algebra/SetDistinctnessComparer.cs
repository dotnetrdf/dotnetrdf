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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Algebra
{

    /// <summary>
    /// Comparer for checking whether sets are distinct, check may either be using the entire set or by using only a subset of variables
    /// </summary>
    public class SetDistinctnessComparer
        : IEqualityComparer<ISet>
    {
        private List<String> _vars = new List<String>();

        public SetDistinctnessComparer() { }

        public SetDistinctnessComparer(IEnumerable<String> variables)
        {
            this._vars.AddRange(variables);
        }

        public bool Equals(ISet x, ISet y)
        {
            if (this._vars.Count == 0)
            {
                return x.Equals(y);
            }
            else
            {
                return this._vars.All(v => (x[v] == null && y[v] == null) || (x[v] != null && x[v].Equals(y[v])));
            }
        }

        public int GetHashCode(ISet obj)
        {
            if (this._vars.Count == 0)
            {
                return obj.GetHashCode();
            }
            else
            {
                StringBuilder output = new StringBuilder();
                foreach (String var in this._vars)
                {
                    output.Append("?" + var + " = " + obj[var].ToSafeString());
                    output.Append(" , ");
                }
                output.Remove(output.Length - 3, 3);
                return output.ToString().GetHashCode();
            }
        }
    }
}
