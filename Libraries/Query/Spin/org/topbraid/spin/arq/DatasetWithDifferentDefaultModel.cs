using VDS.RDF.Query.Spin;
using VDS.RDF.Storage;
namespace  org.topbraid.spin.arq {
/**
 * A DelegatingDataset that uses a different default model than the delegate.
 * 
 * @author Holger Knublauch
 */
public class DatasetWithDifferentDefaultModel : DelegatingDataset {

	private IUpdateableStorage defaultModel;
	
	
	public DatasetWithDifferentDefaultModel(IUpdateableStorage defaultModel, Dataset _delegate) : base(_delegate){
		
		this.defaultModel = defaultModel;
	}

	
	override public IUpdateableStorage getDefaultModel() {
		return defaultModel;
	}	
}
}