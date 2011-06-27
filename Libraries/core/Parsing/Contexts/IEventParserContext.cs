using System;
using VDS.RDF.Parsing.Events;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Interface for Event Parser contexts
    /// </summary>
    /// <typeparam name="T">Event Type</typeparam>
    public interface IEventParserContext<T> where T : IEvent
    {
        /// <summary>
        /// Queue of Events
        /// </summary>
        IEventQueue<T> Events 
        { 
            get;
        }
    }
}
