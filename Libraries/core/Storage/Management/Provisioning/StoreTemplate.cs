using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Management.Provisioning
{
    /// <summary>
    /// A basic store template where the only parameter is the Store ID
    /// </summary>
    public class StoreTemplate
        : IStoreTemplate
    {
        /// <summary>
        /// Creates a new template
        /// </summary>
        /// <param name="id">Store ID</param>
        public StoreTemplate(String id)
        {
            this.ID = id;
        }

        /// <summary>
        /// Gets/Sets the Store ID
        /// </summary>
        [Description("The ID of the Store")]
        public String ID
        {
            get;
            set;
        }
    }
}
