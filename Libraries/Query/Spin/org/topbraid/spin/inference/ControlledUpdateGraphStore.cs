using VDS.RDF.Storage;

using VDS.RDF;
using System.Collections.Generic;
using System;

namespace org.topbraid.spin.inference 
{ 

/**
 * A GraphStore that wraps a given Dataset, so that each updateable
 * graph is wrapped with a ControlledUpdateGraph instead of the default.
 * 
 * @author Holger Knublauch
 */
class ControlledUpdateGraphStore 
    : IGraphStore 
{

    private Dictionary<Graph, ControlledUpdateGraph> cugs = new Dictionary<Graph, ControlledUpdateGraph>();
	
	private Dataset dataset;
	
	
	ControlledUpdateGraphStore(Dataset dataset, IEnumerable<Graph> controlledGraphs) {
		this.dataset = dataset;
		foreach(Graph graph in controlledGraphs) {
			ControlledUpdateGraph cug = new ControlledUpdateGraph(graph);
			cugs[graph] = cug;
		}
	}
	
	
	private Graph getControlledUpdateGraph(Graph graph) {
        Graph cug = null;
        if (cugs.ContainsKey(graph)) {
            cug = cugs[graph];
        }
		if(cug != null) {
			return cug;
		}
		else {
			return graph;
		}
	}


    public IEnumerable<ControlledUpdateGraph> getControlledUpdateGraphs()
    {
		return cugs.values();
	}


	override public Graph getDefaultGraph() {
		IUpdateableStorage defaultModel = dataset.getDefaultModel();
		if(defaultModel != null) {
			return getControlledUpdateGraph(defaultModel.getGraph());
		}
		else {
			return null;
		}
	}


	override public IGraph getGraph(INode graphNode) {
		IUpdateableStorage model = dataset.getNamedModel(graphNode.Uri);
		if(model != null) {
			return getControlledUpdateGraph(model.getGraph());
		}
		else {
			return null;
		}
	}


    override public bool containsGraph(INode graphNode)
    {
		return dataset.containsNamedModel(graphNode.Uri);
	}


	override public void setDefaultGraph(Graph g) {
	}


    override public void addGraph(Uri graphName, Graph graph)
    {
	}


    override public void removeGraph(Uri graphName)
    {
	}


    override public IEnumerator<Uri> listGraphNodes()
    {
        List<Uri> results = new List<Uri>();
        IEnumerator<String> it = dataset.listNames();
		while(it.MoveNext()) {
			results.Add(UriFactory.Create(it.Current));
		}
		return results.GetEnumerator();
	}


	override public void add(Quad quad) {
		Graph graph;
		if(quad.isDefaultGraph()) {
			graph = getDefaultGraph();
		}
		else {
			graph = getGraph(quad.getGraph());
		}
		if(graph != null) {
			graph.Add(quad.asTriple());
		}
	}


	override public void delete(Quad quad) {
		Graph graph;
		if(quad.isDefaultGraph()) {
			graph = getDefaultGraph();
		}
		else {
			graph = getGraph(quad.getGraph());
		}
		if(graph != null) {
			graph.delete(quad.asTriple());
		}
	}


	override public void deleteAny(INode g, INode s, INode p, INode o) {
        IEnumerator<Quad> iter = find(g, s, p, o) ;
        List<Quad> list = Iter.toList(iter) ;
        foreach(Quad q in list) {
            delete(q);
        }
	}


	override public IEnumerator<Quad> find() {
		return null;
	}


	override public IEnumerator<Quad> find(Quad quad) {
		return null;
	}


	override public IEnumerator<Quad> find(INode g, INode s, INode p, INode o) {
		return null;
	}


	override public IEnumerator<Quad> findNG(INode g, INode s, INode p, INode o) {
		return null;
	}


	override public bool contains(INode g, INode s, INode p, INode o) {
		Graph graph = getGraph(g);
		if(graph != null) {
			return graph.Contains(s, p, o);
		}
		else {
			return false;
		}
	}


	override public bool contains(Quad quad) {
		return false;
	}


	override public bool isEmpty() {
		return false;
	}


	override public Lock getLock() {
		return null;
	}


	override public Context getContext() {
		return ARQ.getContext() ;
	}


	override public long size() {
		return 0;
	}


	override public void close() {
	}


	override public Dataset toDataset() {
		return null;
	}


	override public void startRequest() {
	}


	override public void finishRequest() {
	}


    override public void add(INode g, INode s, INode p, INode o)
    {
		add(Quad.create(g, s, p, o));
	}


	override public void delete(INode g, INode s, INode p, INode o) {
		delete(Quad.create(g, s, p, o));
	}
}
}
