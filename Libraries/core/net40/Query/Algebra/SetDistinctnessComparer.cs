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
