using System.Data;

namespace VDS.RDF.Data.DataTables
{
    public static class GraphExtensions
    {
        /// <summary>
        /// Casts a Graph to a DataTable with all Columns typed as <see cref="INode">INode</see> (Column Names are Subject, Predicate and Object
        /// </summary>
        /// <param name="g">Graph to convert</param>
        /// <returns>
        /// A DataTable containing three Columns (Subject, Predicate and Object) all typed as <see cref="INode">INode</see> with a Row per Triple
        /// </returns>
        public static DataTable ToDataTable(this IGraph g)
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Subject", typeof(INode)));
            table.Columns.Add(new DataColumn("Predicate", typeof(INode)));
            table.Columns.Add(new DataColumn("Object", typeof(INode)));

            foreach (Triple t in g.Triples)
            {
                DataRow row = table.NewRow();
                row["Subject"] = t.Subject;
                row["Predicate"] = t.Predicate;
                row["Object"] = t.Object;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
