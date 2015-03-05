namespace VDS.RDF.Query.Spin.Model
{
    /**
     * An abstraction for Query, Modify and DeleteWhere, i.e. all SPARQL commands
     * that may contain a WHERE clause.
     * 
     * @author Holger Knublauch
     */
    public interface ICommandWithWhereResource : ICommandResource
    {

        /**
         * Gets the ElementList of the WHERE clause of this query.
         * Might be null or RDF.nil.
         * @return the WHERE clause as an ElementList
         */
        IElementListResource getWhere();
    }
}