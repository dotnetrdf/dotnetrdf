using VDS.RDF.Storage;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query;
namespace org.topbraid.spin.spr
{
    /**
     * An interface for objects that can convert a SPARQL SparqlResultSet
     * into a SPR table.
     * 
     * @author Holger Knublauch
     */
    public interface TableEngine
    {

        INode createTable(Model model, SparqlResultSet rs);
    }
}