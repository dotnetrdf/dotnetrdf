namespace org.topbraid.spin.model.update
{


    /**
     * A SPARQL Update LOAD operation.
     * 
     * @author Holger Knublauch
     */
    public interface ILoad : IUpdate
    {

        /**
         * Checks if this Update operation has been marked to be SILENT.
         * @return true if SILENT
         */
        bool isSilent();
    }
}