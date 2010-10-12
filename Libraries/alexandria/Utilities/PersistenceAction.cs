using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace VDS.Alexandria.Utilities
{
    public class PersistenceAction
    {
        private bool _delete = false;
        private Triple _t;

        public PersistenceAction(Triple t, bool toDelete)
        {
            this._t = t;
            this._delete = toDelete;
        }

        public PersistenceAction(Triple t)
            : this(t, false) { }

        public Triple Triple
        {
            get
            {
                return this._t;
            }
        }

        public bool IsDelete
        {
            get
            {
                return this._delete;
            }
        }
    }
}
