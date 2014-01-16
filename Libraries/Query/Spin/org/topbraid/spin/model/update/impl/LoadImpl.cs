using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Datasets;
namespace  org.topbraid.spin.model.update.impl {


public class LoadImpl : UpdateImpl, ILoad {

    public LoadImpl(INode node, SpinProcessor spinModel)
        : base(node, spinModel)
    {
		
	}

	
	override public void printSPINRDF(IContextualSparqlPrinter p) {
		p.printKeyword("LOAD");
		p.print(" ");
		printSilent(p);
        IResource document = getResource(SP.PropertyDocument);
		p.printURIResource(document);
        IResource into = getResource(SP.PropertyInto);
		if(into != null) {
			p.print(" ");
			p.printKeyword("INTO");
			p.print(" ");
			p.printKeyword("GRAPH");
			p.print(" ");
			p.printURIResource(into);
		}
	}
}
}