namespace VDS.RDF.Query.Spin.Model
{

    /**
     * A SPARQL Update DELETE WHERE operation.
     * 
     * @author Holger Knublauch
     */
    public interface IDeleteWhereResource : IUpdateResource, ICommandWithWhereResource
    {
    }
}