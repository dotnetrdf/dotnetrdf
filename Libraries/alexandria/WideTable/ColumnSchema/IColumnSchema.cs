using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace VDS.Alexandria.WideTable.ColumnSchema
{
    public interface IColumnSchema<TColumn>
    {
        /// <summary>
        /// Converts to a set of Columns from a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        IEnumerable<TColumn> ToColumns(Triple t);

        /// <summary>
        /// Converts to a Triple in the given Graph from a set of Columns
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="columns">Columns</param>
        /// <returns></returns>
        Triple FromColumns(IGraph g, IEnumerable<TColumn> columns);

        /// <summary>
        /// Converts to a Triple from a set of Columns
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <returns></returns>
        Triple FromColumns(IEnumerable<TColumn> columns);

        /// <summary>
        /// Gets the Names of all Columns that are used in the Schema
        /// </summary>
        IEnumerable<String> ColumnNames
        {
            get;
        }
    }
}
