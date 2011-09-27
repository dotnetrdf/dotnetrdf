using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    public interface IConnectionDefinition
        : IEnumerable<KeyValuePair<PropertyInfo, ConnectionAttribute>>
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
