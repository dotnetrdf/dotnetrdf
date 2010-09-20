using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace rdfMetal
{
    [Serializable]
    public class OntologyClass
    {
        private String _supertype = "OwlClassSupertype";
        public string Uri { get; set; }
        public string Name { get; set; }

        public string Supertype
        {
            get
            {
                return this._supertype;
            }
            set
            {
                this._supertype = value;
            }
        }

        public OntologyProperty[] Properties { get; set; }
        [XmlIgnore]
        public IEnumerable<OntologyProperty> OutgoingRelationships
        {
            get
            {
                return (from p in Properties
                        where p.IsObjectProp
                        select p).ToArray();
            }
        }
        [XmlIgnore]
        public IEnumerable<OntologyProperty> DatatypeProperties
        {
            get
            {
                return (from p in Properties
                        where p.IsObjectProp == false
                        select p).ToArray();
            }
        }

        private OntologyProperty[] incomingRelationships;

        [XmlIgnore]
        public OntologyProperty[] IncomingRelationships
        {
            get { return incomingRelationships; }
            set { incomingRelationships = value; }
        }
    }
}