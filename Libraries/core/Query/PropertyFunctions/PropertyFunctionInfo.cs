using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    public class PropertyFunctionInfo
    {
        private Uri _funcUri;
        private List<IMatchTriplePattern> _patterns = new List<IMatchTriplePattern>();
        private List<PatternItem> _lhsArgs = new List<PatternItem>();
        private List<PatternItem> _rhsArgs = new List<PatternItem>();

        public PropertyFunctionInfo(Uri u)
        {
            this._funcUri = u;
        }

        public Uri FunctionUri
        {
            get
            {
                return this._funcUri;
            }
        }

        public List<IMatchTriplePattern> Patterns
        {
            get
            {
                return this._patterns;
            }
        }

        public List<PatternItem> LhsArgs
        {
            get
            {
                return this._lhsArgs;
            }
        }

        public List<PatternItem> RhsArgs
        {
            get
            {
                return this._rhsArgs;
            }
        }
    }
}
