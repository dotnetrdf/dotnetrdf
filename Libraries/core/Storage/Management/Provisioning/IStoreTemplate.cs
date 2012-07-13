using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Management.Provisioning
{
    /// <summary>
    /// Interface for templates for the provisioning of new stores
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is intentionally very limited, the generic type constraints on the <see cref="IStorageServer"/> interface allow for specific implementations of that interface to futher constrain their implementation to accept only relevant implementations of this interface when provisioning new stores.
    /// </para>
    /// <para>
    /// Specific implementations will likely add various properties that allow end users to configure implementation specific parameters.  It is suggested that implementors include System.ComponentModel attributes on their implementations.
    /// </para>
    /// </remarks>
    public interface IStoreTemplate
    {
        /// <summary>
        /// Gets/Sets the ID for the Store
        /// </summary>
        String ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the type of store the template will create
        /// </summary>
        String TemplateName
        {
            get;
        }

        /// <summary>
        /// Gets the description of the type of store the template will create
        /// </summary>
        String TemplateDescription
        {
            get;
        }
    }
}
