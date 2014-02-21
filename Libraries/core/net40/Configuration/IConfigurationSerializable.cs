namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Interface for Objects which can have their configuration serialized to RDF
    /// </summary>
    public interface IConfigurationSerializable
    {
        /// <summary>
        /// Serializes the Configuration in the given context
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        void SerializeConfiguration(ConfigurationSerializationContext context);
    }
}