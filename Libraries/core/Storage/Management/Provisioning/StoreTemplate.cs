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
            : this(id, "Unknown", String.Empty) { }

        /// <summary>
        /// Creates a new template
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="name">Template Name</param>
        /// <param name="description">Template Description</param>
        public StoreTemplate(String id, String name, String description)
        {
            this.ID = id;
            this.TemplateName = name;
            this.TemplateDescription = description;
        }

        /// <summary>
        /// Gets/Sets the Store ID
        /// </summary>
        [Category("Basic"), Description("The ID of the Store")]
        public String ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the type of store the template will create
        /// </summary>
        [Category("Basic"), Description("Name of the type of store the template will create"), ReadOnly(true)]
        public String TemplateName
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the description of the type of store the template will create
        /// </summary>
        [Category("Basic"), Description("Description of the type of store the template will create"), ReadOnly(true)]
        public String TemplateDescription
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the string representation of the template which is the Template Name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.TemplateName;
        }
    }
}
