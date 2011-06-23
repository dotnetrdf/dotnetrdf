using System;
using VDS.RDF.Parsing.Events;

namespace VDS.RDF.Parsing.Contexts
{
    public interface IEventParserContext<T> where T : IEvent
    {
        IEventQueue<T> Events 
        { 
            get;
        }
    }
}
