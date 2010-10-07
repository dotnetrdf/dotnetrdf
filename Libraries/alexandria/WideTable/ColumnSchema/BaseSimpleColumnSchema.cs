using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;

namespace VDS.Alexandria.WideTable.ColumnSchema
{
    /// <summary>
    /// The Simple Column Schema stores each part of the Triple in a single column
    /// </summary>
    /// <typeparam name="TColumn"></typeparam>
    public abstract class BaseSimpleColumnSchema<TColumn> : IColumnSchema<TColumn>
    {
        private String _graphColumn = "Graph";
        private String _subjectColumn = "Subject";
        private String _predicateColumn = "Predicate";
        private String _objectColumn = "Object";
        private String[] _columns = new String[4];

        public BaseSimpleColumnSchema(bool requireSorted)
        {
            if (requireSorted)
            {
                //Prepend alphabetic identifiers to column names so the columns get stored in the order Graph, Subject, Predicate, Object
                this._graphColumn = "a" + this._graphColumn;
                this._subjectColumn = "b" + this._subjectColumn;
                this._predicateColumn = "c" + this._predicateColumn;
                this._objectColumn = "d" + this._objectColumn;
            }
            this._columns[0] = this._graphColumn;
            this._columns[1] = this._subjectColumn;
            this._columns[2] = this._predicateColumn;
            this._columns[3] = this._objectColumn;
        }

        public BaseSimpleColumnSchema()
            : this(false) { }

        public IEnumerable<String> ColumnNames
        {
            get
            {
                return this._columns;
            }
        }

        public IEnumerable<TColumn> ToColumns(Triple t)
        {
            List<TColumn> columns = new List<TColumn>();
            if (t.GraphUri != null)
            {
                columns.Add(this.ToColumn(this._graphColumn, t.GraphUri));
            }
            columns.Add(this.ToColumn(this._subjectColumn, t.Subject));
            columns.Add(this.ToColumn(this._predicateColumn, t.Predicate));
            columns.Add(this.ToColumn(this._objectColumn, t.Object));
            return columns;
        }

        protected abstract TColumn ToColumn(String columnName, INode value);

        protected abstract TColumn ToColumn(String columnName, Uri uri);

        public Triple FromColumns(IGraph g, IEnumerable<TColumn> columns)
        {
            //Get the Triple Nodes
            TColumn subjColumn, predColumn, objColumn;
            subjColumn = this.GetColumnWithName(columns, this._subjectColumn);
            predColumn = this.GetColumnWithName(columns, this._predicateColumn);
            objColumn = this.GetColumnWithName(columns, this._objectColumn);

            return new Triple(this.FromColumn(g, subjColumn), this.FromColumn(g, predColumn), this.FromColumn(g, objColumn));
        }

        public Triple FromColumns(IEnumerable<TColumn> columns)
        {
            //Determine the Graph we're extracting this from
            TColumn graphColumn = this.GetColumnWithName(columns, this._graphColumn);
            Uri graphUri = this.FromColumn(graphColumn);
            NonIndexedGraph g = new NonIndexedGraph();

            return this.FromColumns(g, columns);
        }

        protected abstract TColumn GetColumnWithName(IEnumerable<TColumn> columns, String name);

        protected abstract INode FromColumn(IGraph g, TColumn column);

        protected abstract Uri FromColumn(TColumn column);
    }
}
