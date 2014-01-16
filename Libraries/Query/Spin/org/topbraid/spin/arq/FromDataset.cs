using VDS.RDF.Storage;
using System.Collections.Generic;
using System;
using VDS.RDF.Query.Spin;
using org.topbraid.spin.model;
namespace  org.topbraid.spin.arq {

/**
 * A Dataset that wraps another Dataset but changes its default and
 * named graphs based on the FROM and FROM NAMED clauses of a given
 * Query.
 * 
 * @author Holger Knublauch
 */
public class FromDataset : DelegatingDataset {
	
	private HashSet<String> defaultGraphs;
	
	private IUpdateableStorage defaultModel;

	private HashSet<String> namedGraphs;
	
	
	public FromDataset(Dataset _delegate, Query query) :base (_delegate){
		
		defaultGraphs = new HashSet<String>(query.getGraphURIs());
		namedGraphs = new HashSet<String>(query.getNamedGraphURIs());
	}


	override public bool containsNamedModel(String uri) {
		if(namedGraphs.isEmpty()) {
			return true;
		}
		else {
			return namedGraphs.Contains(uri);
		}
	}


	override public IUpdateableStorage getDefaultModel() {
		if(defaultGraphs.isEmpty()) {
			return base.getDefaultModel();
		}
		else {
			if(defaultModel == null) {
				if(defaultGraphs.size() == 1) {
					String defaultGraphURI = defaultGraphs.GetEnumerator().Current;
					defaultModel = getNamedModel(defaultGraphURI);
				}
				else {
					MultiUnion multiUnion = JenaUtil.createMultiUnion();
					foreach(String baseURI in defaultGraphs) {
						IUpdateableStorage model = getNamedModel(baseURI);
						multiUnion.addGraph(model.getGraph());
					}
					defaultModel = ModelFactory.createModelForGraph(multiUnion);
				}
			}
			return defaultModel;
		}
	}


	override public IEnumerator<String> listNames() {
		if(namedGraphs.isEmpty()) {
			return base.listNames();
		}
		else {
			return namedGraphs.GetEnumerator();
		}
	}
}
}