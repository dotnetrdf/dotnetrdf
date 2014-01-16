using System;
using org.topbraid.spin.model;
using org.topbraid.spin.model.update;
using VDS.RDF;
using VDS.RDF.Update;

namespace org.topbraid.spin.util
{
    /**
     * A CommandWrapper that wraps SPARQL UPDATE requests
     * (in contrast to QueryWrapper for SPARQL queries).
     * 
     * @author Holger Knublauch
     */
    public class UpdateWrapper : CommandWrapper
    {

        private SparqlUpdateCommand update;

        private IUpdate spinUpdate;


        public UpdateWrapper(SparqlUpdateCommand update, INode source, String text, IUpdate spinUpdate, String label, Triple statement, bool thisUnbound, int thisDepth)
            : base(source, text, label, statement, thisUnbound, thisDepth)
        {
            
            this.update = update;
            this.spinUpdate = spinUpdate;
        }


        public SparqlUpdateCommand getUpdate()
        {
            return update;
        }


        override public ICommand getSPINCommand()
        {
            return getSPINUpdate();
        }


        public IUpdate getSPINUpdate()
        {
            return spinUpdate;
        }
    }
}