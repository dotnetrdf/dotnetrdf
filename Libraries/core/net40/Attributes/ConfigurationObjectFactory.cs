using System;
using VDS.RDF.Configuration;

namespace VDS.RDF.Attributes
{
    /// <summary>
    /// An attribute used to declare available configuration object factories
    /// </summary>
    /// <remarks>
    /// When the Configuration API provided by the 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ConfigurationObjectFactoryAttribute
        : Attribute
    {
        /// <summary>
        /// Gets/Sets a factory type, this must implement the <see cref="IObjectFactory"/> interface to be honoured
        /// </summary>
        public Type FactoryType { get; set; }
    }
}
