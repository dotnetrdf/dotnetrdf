using System;
using System.Collections.Generic;

namespace VDS.RDF.Parsing.Events
{
    /// <summary>
    /// Abstract Base Class for <see cref="IRdfAEvent">IRdfAEvent</see> implementations
    /// </summary>
    public abstract class BaseRdfAEvent 
        : BaseEvent, IRdfAEvent
    {
        private Dictionary<string, string> _attributes;

        /// <summary>
        /// Creates a new RDFa Event
        /// </summary>
        /// <param name="eventType">Event Type</param>
        /// <param name="pos">Position Info</param>
        /// <param name="attributes">Attributes</param>
        public BaseRdfAEvent(int eventType, PositionInfo pos, IEnumerable<KeyValuePair<string, string>> attributes)
            : base(eventType, pos)
        {
            this._attributes = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> attr in attributes)
            {
                this._attributes.Add(attr.Key, attr.Value);
            }
        }

        /// <summary>
        /// Gets the attributes of the event i.e. the attributes of the source element
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Attributes
        {
            get 
            {
                return this._attributes; 
            }
        }

        /// <summary>
        /// Gets whether the Event has a given attribute
        /// </summary>
        /// <param name="name">Attribute Name</param>
        /// <returns></returns>
        public bool HasAttribute(String name)
        {
            return this._attributes.ContainsKey(name);
        }

        /// <summary>
        /// Gets the value of a specific attribute
        /// </summary>
        /// <param name="name">Attribute Name</param>
        /// <returns></returns>
        public String this[String name]
        {
            get
            {
                return this._attributes[name];
            }
        }
    }
}