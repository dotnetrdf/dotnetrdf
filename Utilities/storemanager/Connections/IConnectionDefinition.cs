using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    public interface IConnectionDefinition
    {
        String StoreName
        {
            get;
        }

        String StoreDescription
        {
            get;
        }

        IGenericIOManager OpenConnection();
    }
}
