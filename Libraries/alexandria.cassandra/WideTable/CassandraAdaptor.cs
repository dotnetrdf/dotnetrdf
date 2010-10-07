using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquiles;
using Aquiles.Connection;
using Aquiles.Command;
using Aquiles.Model;
using VDS.Alexandria.WideTable.ColumnSchema;
using VDS.RDF;

namespace VDS.Alexandria.WideTable
{
    public class CassandraAdaptor : BaseWideTableAdaptor<AquilesColumn>
    {
        private IAquilesConnection _connection;
        private IColumnSchema<AquilesColumn> _schema;
        private String _keySpace;
        private String _columnFamily;

        private const String DefaultKeySpace = "dotNetRDF";
        private const String DefaultColumnFamily = "Graphs";

        public CassandraAdaptor(String clusterName, String keySpace, String columnFamily, IColumnSchema<AquilesColumn> schema)
        {
            throw new AlexandriaException("Implementation Work on Cassandra Adaptor is currently halted until the 0.7 release of Cassandra stabilises the Secondary Indexes feature and the Aquiles C# library for Cassandra adds support this this");
            this._connection = AquilesHelper.RetrieveConnection(clusterName);
            this._keySpace = keySpace;
            this._columnFamily = columnFamily;
            this._schema = schema;
        }

        public CassandraAdaptor(String clusterName, String keySpace, String columnFamily)
            : this(clusterName, keySpace, columnFamily, new CassandraSchema()) { }

        public CassandraAdaptor(String clusterName, String keySpace)
            : this(clusterName, keySpace, DefaultColumnFamily) { }

        public CassandraAdaptor(String clusterName)
            : this(clusterName, DefaultKeySpace, DefaultColumnFamily) { }

        ~CassandraAdaptor()
        {
            this.Dispose(false);
        }

        private InsertCommand GetInsertCommand(String rowKey)
        {
            InsertCommand command = new InsertCommand();
            command.KeySpace = this._keySpace;
            command.ColumnFamily = this._columnFamily;
            command.Key = rowKey;
            return command;
        }

        private DeleteCommand GetDeleteCommand(String rowKey)
        {
            DeleteCommand command = new DeleteCommand();
            command.KeySpace = this._keySpace;
            command.ColumnFamily = this._columnFamily;
            command.Key = rowKey;
            return command;
        }

        public override IColumnSchema<AquilesColumn> ColumnSchema
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal String KeySpace
        {
            get
            {
                return this._keySpace;
            }
        }

        internal String ColumnFamily
        {
            get
            {
                return this._columnFamily;
            }
        }

        protected override bool InsertData(String rowKey, AquilesColumn column)
        {
            try
            {
                InsertCommand insert = GetInsertCommand(rowKey);
                insert.Column = column;

                this._connection.Execute(insert);
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to insert data into the Cassandra Store", ex);
            }
        }

        protected override bool InsertData(String rowKey, IEnumerable<AquilesColumn> columns)
        {
            foreach (AquilesColumn column in columns)
            {
                this.InsertData(rowKey, column);
            }
            return true;
        }

        protected override bool DeleteData(String rowKey, AquilesColumn column)
        {
            try
            {
                DeleteCommand delete = GetDeleteCommand(rowKey);
                delete.Column = column;

                this._connection.Execute(delete);
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to insert data into the Cassandra Store", ex);
            }
        }

        protected override bool DeleteData(String rowKey, IEnumerable<AquilesColumn> columns)
        {
            foreach (AquilesColumn column in columns)
            {
                this.DeleteData(rowKey, column);
            }
            return true;
        }

        protected override bool DeleteRow(string rowKey)
        {
            try
            {
                DeleteCommand delete = this.GetDeleteCommand(rowKey);
                this._connection.Execute(delete);
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to delete a row from the Cassandra Store", ex);
            }
        }

        protected override IEnumerable<string> GetRowsForGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<AquilesColumn> GetColumnsForRow(string rowKey, IEnumerable<string> columNames)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            this._connection.Dispose();
        }
    }
}
