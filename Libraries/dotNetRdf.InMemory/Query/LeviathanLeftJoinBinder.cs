using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Special Temporary Results Binder used during LeftJoin's.
    /// </summary>
    public class LeviathanLeftJoinBinder : SparqlResultBinder
    {
        private BaseMultiset _input;

        /// <summary>
        /// Creates a new LeftJoin Binder.
        /// </summary>
        /// <param name="multiset">Input Multiset.</param>
        public LeviathanLeftJoinBinder(BaseMultiset multiset)
            : base()
        {
            _input = multiset;
        }

        /// <summary>
        /// Gets the Value for a given Variable from the Set with the given Binding ID.
        /// </summary>
        /// <param name="name">Variable.</param>
        /// <param name="bindingID">Set ID.</param>
        /// <returns></returns>
        public override INode Value(string name, int bindingID)
        {
            return _input[bindingID][name];
        }

        /// <summary>
        /// Gets the Variables in the Input Multiset.
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                return _input.Variables;
            }
        }

        /// <summary>
        /// Gets the IDs of Sets.
        /// </summary>
        public override IEnumerable<int> BindingIDs
        {
            get
            {
                return _input.SetIDs;
            }
        }
    }
}
