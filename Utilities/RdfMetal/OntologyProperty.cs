using System;
using System.Xml.Serialization;

namespace rdfMetal
{
    [Serializable]
    public class OntologyProperty
    {
        public string Uri { get; set; }
        public bool IsObjectProp { get; set; }
        public string Name { get; set; }
        public string RangeUri { get; set; }
        public string Range { get; set; }
        [XmlIgnore]
        public OntologyClass HostClass { get; set; }
    }
}
