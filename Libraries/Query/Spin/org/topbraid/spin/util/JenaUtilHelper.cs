using VDS.RDF.Storage;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Query.Spin;
namespace  org.topbraid.spin.util {

/**
 * This is an extension point for the SPIN library
 * allowing modification of some low level utilities
 * that are exposed through {@link JenaUtil}.
 * 
 * Note: Unstable - should not be used outside of TopBraid.
 * 
 * @author Jeremy Carroll
 */
public class JenaUtilHelper {
	
	/**
	 * Return a multiunion.
	 * @return
	 */
	public MultiUnion createMultiUnion() {
		return new MultiUnion();
	}
	
	
	/**
	 * Return a multiunion, initialized with the given graphs.
	 * @return
	 */
	public MultiUnion createMultiUnion(IEnumerator<IGraph> graphs) {
		return new MultiUnion(graphs);
	}

	
	/**
	 * Return a multiunion, initialized with the given graphs.
	 * @return
	 */
    public MultiUnion createMultiUnion(IGraph[] graphs)
    {
		return new MultiUnion(graphs);
	}
	
	
	/**
	 * A memory graph with no reification.
	 */
	public Graph createDefaultGraph() {
		return Factory.createDefaultGraph();
	}

	
	/**
	 * Returns true if optimizations for faster graphs should
	 * be applied; false if graph is slower. A typical fast graph
	 * is stored in memory, a typical slow graph is stored in a database.
	 * The calling code {@link JenaUtil#isMemoryGraph(Graph)}
	 * deals with {@link MultiUnion}s by taking
	 * the logical AND of the subgraphs.
	 * @param graph A simple graph, not a {@link MultiUnion}
	 * @return true if the graph is fast
	 */
    public bool isMemoryGraph(IGraph graph)
    {
		return (graph is GraphMemBase);
	}
	
	
	/**
	 * The default implementation does nothing. In TB this is enforced.
	 * @param m
	 * @return
	 */
	public IUpdateableStorage asReadOnlyModel(IUpdateableStorage m) {
		return m;
	}


    public IGraph asReadOnlyGraph(IGraph g)
    {
		return g;
	}
	
	
	public OntModel createOntologyModel(OntModelSpec spec, IUpdateableStorage baseModel) {
		return ModelFactory.createOntologyModel(spec, baseModel);
	}
	
	
	public OntModel createOntologyModel() {
		return ModelFactory.createOntologyModel();
	}
	
	
	public OntModel createOntologyModel(OntModelSpec spec) {
		return ModelFactory.createOntologyModel(spec);
	}


    public IGraph createConcurrentGraph()
    {
		return createDefaultGraph();
	}
	
	
	public void setGraphReadOptimization(bool b) {
	}

    public IGraph deepCloneReadOnlyGraph(IGraph g)
    {
		return asReadOnlyGraph(g);
	}
}}