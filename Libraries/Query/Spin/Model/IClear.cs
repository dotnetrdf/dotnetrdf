namespace VDS.RDF.Query.Spin.Model
{

    /**
     * A SPARQL Update CLEAR operation.
     * 
     * @author Holger Knublauch
     */
    public interface IClear : IUpdate
    {

        /**
         * Checks if this Update operation has been marked to be SILENT.
         * @return true if SILENT
         */
        bool isSilent();
    }
}