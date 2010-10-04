using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Construct
{
    public class ConstructContext
    {
        private Set _s;
        private IGraph _g;
        private bool _preserveBNodes = false;
        private Dictionary<String, INode> _bnodeMap;

        public ConstructContext(IGraph g, Set s, bool preserveBNodes)
        {
            this._g = g;
            this._s = s;
            this._preserveBNodes = preserveBNodes;
        }

        public Set Set
        {
            get
            {
                return this._s;
            }
        }

        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        public bool PreserveBlankNodes
        {
            get
            {
                return this._preserveBNodes;
            }
        }

        public INode GetBlankNode(String id)
        {
            if (this._bnodeMap == null) this._bnodeMap = new Dictionary<string, INode>();

            if (this._bnodeMap.ContainsKey(id)) return this._bnodeMap[id];

            INode temp;
            if (this._g != null)
            {
                temp = this._g.CreateBlankNode();
            }
            else if (this._s != null)
            {
                temp = new BlankNode(this._g, id.Substring(2) + "-" + this._s.ID);
            }
            else
            {
               temp = new BlankNode(this._g, id.Substring(2));
            }
            this._bnodeMap.Add(id, temp);
            return temp;
        }
    }
}
