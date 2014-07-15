using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Compiler
{
    /// <summary>
    /// Interface for query compilers which take in a query and compile it to the equivalent algebra
    /// </summary>
    public interface IQueryCompiler
    {
        IAlgebra Compile(IQuery query);
    }
}
