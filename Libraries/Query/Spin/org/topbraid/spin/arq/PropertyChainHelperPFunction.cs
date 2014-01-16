using VDS.RDF.Storage;
using VDS.RDF;
using System.Collections.Generic;
using VDS.RDF.Query.Spin;
namespace  org.topbraid.spin.arq {

/**
 * A helper property function needed by OWL 2 RL rule prp-spo2.
 * This rule needs to walk rdf:Lists of arbitrary length and
 * match triple along the way - very hard to express in pure SPARQL.
 * 
 * @author Holger Knublauch
 */
public class PropertyChainHelperPFunction : PropertyFunctionBase {

	override public QueryIterator exec(/*sealed*/ Binding binding, PropFuncArg argSubject,
            INode predicate, PropFuncArg argObject, /*sealed*/ ExecutionContext execCxt)
    {
		
		/*sealed*/ QueryIterConcat concat = new QueryIterConcat(execCxt);
		
		argSubject = Substitute.substitute(argSubject, binding);
		argObject = Substitute.substitute(argObject, binding);
		IUpdateableStorage model = ModelFactory.createModelForGraph(execCxt.getActiveGraph());
		RDFList rdfList = model.asRDFNode(argSubject.getArg()).As(RDFList);
		List<INode> ps = rdfList.asJavaList();
		INode[] properties = new INode[ps.size()];
		for(int i = 0; i < ps.size(); i++) {
			properties[i] = ps.get(i).As(INode);
		}
		List<INode> objectList = argObject.getArgList();
		/*sealed*/ Var subjectVar = (Var) objectList.get(0);
		/*sealed*/ Var objectVar = (Var) objectList.get(1);
		
		if(ps.size() > 1) {
			IEnumerator<Triple> it = model.listStatements(null, properties[0], (INode)null);
			while(it.MoveNext()) {
				Triple s = it.Current;
                List<INode> tails = new List<INode>();
				if(!(s.Object is ILiteralNode)) {
					addTails(properties, 1, s.getResource(), tails);
                    foreach (INode tail in tails)
                    {
						BindingMap map = new BindingHashMap(binding);
						map.Add(subjectVar, s.Subject);
						map.Add(objectVar, tail);
						concat.Add(IterLib.result(map, execCxt));
					}
				}
			}
		}
		
		return concat;
	}
	
	
	private void addTails(INode[] properties, int i, INode subject, List<INode> results) {
		if(i == properties.length) {
			results.Add(subject);
		}
		else {
			IEnumerator<Triple> it = subject.listProperties(properties[i]);
			while(it.MoveNext()) {
				Triple s = it.Current;
				if(!(s.Object is ILiteralNode)) {
					addTails(properties, i + 1, s.getResource(), results);
				}
			}
		}
	}
}
}