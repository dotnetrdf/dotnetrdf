using System;

namespace VDS.RDF.Query.Spin.AssemblyDiscovery
{
    /// <summary>
    /// Marks the assembly as containing SPIN implementations and extensions for automated loading
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly)]
    internal class SpinExtensionsAssembly
        : System.Attribute
    {
        public SpinExtensionsAssembly()
        {
        }
    }

    /// <summary>
    /// Assigns the Uri of the RdfsClass that the Class implements
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    internal class ImplementsRDFSClass
        : System.Attribute
    {
        private String _classUri;

        public ImplementsRDFSClass(String classUri)
        {
            _classUri = classUri;
        }

        public String ClassUri
        {
            get
            {
                return _classUri;
            }
        }
    }

    /// <summary>
    /// Assigns the Uri of the RdfsClass that the Class implements
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    internal class ImplementsRDFSNamespace
        : System.Attribute
    {
        private String _namespaceUri;

        public ImplementsRDFSNamespace(String namespaceUri)
        {
            _namespaceUri = namespaceUri;
        }

        public String NamespaceUri
        {
            get
            {
                return _namespaceUri;
            }
        }
    }
}