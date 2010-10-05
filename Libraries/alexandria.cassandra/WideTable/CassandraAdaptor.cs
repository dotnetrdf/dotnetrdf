using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquiles;
using Aquiles.Connection;
using Aquiles.Command;
using Aquiles.Model;

namespace Alexandria.WideTable
{
    public class CassandraAdaptor : IWideTableAdaptor<String, AquilesColumn>
    {
        private IAquilesConnection _connection;

        public const String KeySpace = "dotNetRDF";
        public const String ColumnFamily = "Graphs";

        public CassandraAdaptor(String clusterName)
        {
            this._connection = AquilesHelper.RetrieveConnection(clusterName);
        }

        ~CassandraAdaptor()
        {
            this.Dispose(false);
        }

        public bool CreateRow(String rowKey)
        {
            try
            {
                InsertCommand command = new InsertCommand();
                command.KeySpace = CassandraAdaptor.KeySpace;
                command.ColumnFamily = CassandraAdaptor.ColumnFamily;
                command.Key = rowKey;

                this._connection.Execute(command);
                return true;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to create a row", ex);
            }
        }

        public bool InsertData(String rowKey, AquilesColumn column)
        {
            throw new NotImplementedException();
        }

        public bool InsertData(String rowKey, IEnumerable<AquilesColumn> columns)
        {
            throw new NotImplementedException();
        }

        public bool DeleteData(String rowKey, AquilesColumn column)
        {
            throw new NotImplementedException();
        }

        public bool DeleteData(String rowKey, IEnumerable<AquilesColumn> column)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            this._connection.Dispose();
        }
    }
}
