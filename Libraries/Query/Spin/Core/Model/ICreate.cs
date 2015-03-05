namespace VDS.RDF.Query.Spin.Model
{

    /**
     * A SPARQL Update CREATE operation.
     * 
     * @author Holger Knublauch
     */
    public interface ICreateResource : IUpdateResource
    {

        /**
         * Checks if this Update operation has been marked to be SILENT.
         * @return true if SILENT
         */
        bool isSilent();
    }
}