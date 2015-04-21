namespace VDS.RDF.Query.Spin.Model
{
    /**
     * A SPARQL Update DROP operation.
     *
     * @author Holger Knublauch
     */

    public interface IDropResource : IUpdateResource
    {
        /**
         * Checks if this Update operation has been marked to be SILENT.
         * @return true if SILENT
         */

        bool isSilent();
    }
}