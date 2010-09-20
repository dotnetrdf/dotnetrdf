using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Linq
{
    interface IPendingLinqAction
    {
        void ProcessAction(LinqUpdateProcessor processor);
    }

    class AdditionAction : IPendingLinqAction
    {
        private OwlInstanceSupertype _oc;
        private String _graphUri;

        public AdditionAction(OwlInstanceSupertype oc, String graphUri)
        {
            this._oc = oc;
            this._graphUri = graphUri;
        }

        public void ProcessAction(LinqUpdateProcessor processor)
        {
            processor.SaveObject(this._oc, this._graphUri);
        }
    }

    class DeletionAction : IPendingLinqAction
    {
        private OwlInstanceSupertype _oc;
        private String _graphUri;
        private LinqDeleteMode _mode;

        public DeletionAction(OwlInstanceSupertype oc, String graphUri, LinqDeleteMode mode)
        {
            this._oc = oc;
            this._graphUri = graphUri;
            this._mode = mode;
        }

        public void ProcessAction(LinqUpdateProcessor processor)
        {
            processor.DeleteObject(this._oc, this._graphUri, this._mode);
        }
    }
}
