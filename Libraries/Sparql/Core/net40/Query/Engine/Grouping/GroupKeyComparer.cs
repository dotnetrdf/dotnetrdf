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