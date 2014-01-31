using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Datasets;
namespace  VDS.RDF.Query.Spin.Model {


public class LoadImpl : UpdateImpl, ILoad {

    public LoadImpl(INode node, SpinProcessor spinModel)
        : base(node, spinModel)
    {
		
	}

	
	override public void printSPINRDF(ISparqlFactory p) {
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