using VDS.RDF.Storage;
using VDS.RDF;
using System;

using System.Collections.Generic;
using org.topbraid.spin.util;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query;
using org.topbraid.spin.vocabulary;
namespace org.topbraid.spin.spr.spra
{

    /**
     * Implementation of the SPR protocol http://spinrdf.org/spra
     * 
     * @author Holger Knublauch
     */
    public class ATableEngine : AbstractTableEngine
    {

        override public INode createTable(Model model, SparqlResultSet rs) {
		String ns = SPRA.NS_URI;
		
		String id = AnonId.create().getLabelString().replaceAll(":", "_");
		INode table = model.Object("http://atables.org/data" + id);
		
		List<String> varNames = rs.getResultVars();
		addVarNames(ns, table, varNames);

		// First copy the cells into an auxiliary data structure
		// (avoiding concurrency changes under the hood that might impact the SparqlResultSet)
		INode valueProperty = getValueProperty(ns);
		Dictionary<INode,INode> cell2Value = new Dictionary<INode,INode>();
		int row = 0;
        IEnumerator<SparqlResult> rsEnum = rs.GetEnumerator();
        for (; rsEnum.MoveNext(); row++)
        {
            SparqlResult qs = rsEnum.Current;
			for(int col = 0; col < varNames.size(); col++) {
				String varName = varNames.get(col);
				INode value = qs.get(varName);
				if(value != null) {
					INode cell = getCell(table, row, col);
					cell2Value[cell] = value;
				}
			}
		}

		// Now create the actual triples
		table.addProperty(RDF.type, SPRA.Table);
		foreach(INode cell in cell2Value.Keys) {
			INode value = cell2Value[cell];
			cell.addProperty(valueProperty, value);
		}
		
		table.addProperty(getColCountProperty(ns), RDFUtil.createInteger(varNames.size()));
		table.addProperty(getRowCountProperty(ns), RDFUtil.createInteger(row));
		
		return table;
	}


        private INode getCell(INode table, int row, int col)
        {
            return table.getModel().Object(table.Uri + "-r" + row + "-c" + col);
        }
    }
}