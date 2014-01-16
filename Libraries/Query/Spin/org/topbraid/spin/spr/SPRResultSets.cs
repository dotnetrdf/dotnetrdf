using VDS.RDF.Storage;
using VDS.RDF.Query;

using VDS.RDF;
using org.topbraid.spin.model;
using System.Collections.Generic;

using System;
using org.topbraid.spin.vocabulary;
using org.topbraid.spin.util;
namespace org.topbraid.spin.spr
{


    /**
     * Static utilities on SPR tables.
     * 
     * @author Holger Knublauch
     */
    public class SPRResultSets
    {

        private static ISpinQuery cellQuery = ARQFactory.get().doCreateQuery(
                "SELECT (<" + SPR.cell.Uri + ">(?table, ?row, ?col) AS ?result)\n" +
                "WHERE {\n" +
                "}");

        private static ISpinQuery colCountQuery = ARQFactory.get().doCreateQuery(
                "SELECT (<" + SPR.colCount.Uri + ">(?table) AS ?result)\n" +
                "WHERE {\n" +
                "}");

        private static ISpinQuery colNameQuery = ARQFactory.get().doCreateQuery(
                "SELECT (<" + SPR.colName.Uri + ">(?table, ?col) AS ?result)\n" +
                "WHERE {\n" +
                "}");

        private static ISpinQuery rowCountQuery = ARQFactory.get().doCreateQuery(
                "SELECT (<" + SPR.rowCount.Uri + ">(?table) AS ?result)\n" +
                "WHERE {\n" +
                "}");


        public static INode getCell(INode table, int row, int col)
        {
            Model model = table.getModel();
            QueryExecution qexec = ARQFactory.get().createQueryExecution(cellQuery, model);
            SparqlResultSet bindings = new SparqlResultSet();
            bindings.Add("table", table);
            bindings.Add("row", RDFUtil.createInteger(row));
            bindings.Add("col", RDFUtil.createInteger(col));
            qexec.setInitialBinding(bindings);
            try
            {
                SparqlResultSet rs = qexec.execSelect();
                if (rs.MoveNext())
                {
                    INode result = rs.Current.get("result");
                    return result;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                qexec.close();
            }
        }


        public static int getColCount(INode table)
        {
            return getIntFromFunction(table, colCountQuery);
        }


        public static String getColName(INode table, int col)
        {
            Model model = table.getModel();
            QueryExecution qexec = ARQFactory.get().createQueryExecution(colNameQuery, model);
            SparqlResultSet bindings = new SparqlResultSet();
            bindings.Add("table", table);
            bindings.Add("col", RDFUtil.createInteger(col));
            qexec.setInitialBinding(bindings);
            try
            {
                SparqlResultSet rs = qexec.execSelect();
                if (rs.MoveNext())
                {
                    INode result = rs.Current.get("result");
                    if (result is ILiteralNode)
                    {
                        return ((ILiteralNode)result).getString();
                    }
                }
                return null;
            }
            finally
            {
                qexec.close();
            }
        }


        public static List<String> getColNames(INode table)
        {
            List<String> results = new List<String>();
            int colCount = getColCount(table);
            for(int i = 0; i < colCount; i++)
            {
                results.Add(getColName(table, i));
            }
            return results;
        }


        private static int getIntFromFunction(INode table, ISpinQuery query)
        {
            Model model = table.getModel();
            QueryExecution qexec = ARQFactory.get().createQueryExecution(query, model);
            SparqlResultSet bindings = new SparqlResultSet();
            bindings.Add("table", table);
            qexec.setInitialBinding(bindings);
            try
            {
                SparqlResultSet rs = qexec.execSelect();
                if (rs.MoveNext())
                {
                    INode result = rs.Current.get("result");
                    if (result is ILiteralNode)
                    {
                        return ((ILiteralNode)result).getInt();
                    }
                }
                return 0;
            }
            finally
            {
                qexec.close();
            }
        }


        public static int getRowCount(INode table)
        {
            return getIntFromFunction(table, rowCountQuery);
        }
    }
}