using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using VDS.RDF.Parsing.Contexts;

namespace VDS.RDF.Parsing.Events
{
    /// <summary>
    /// Interface for Event Generators
    /// </summary>
    /// <remarks>
    /// <para>
    /// An Event Generator is a class which takes an input stream which contains XML and generates a series of events from it
    /// </para>
    /// <para>
    /// This interface is a marker interface which indicates that the class is an event generator, implementations should implement one of the concrete derived interfaces as appropriate to their mode of operation.
    /// </para>
    /// </remarks>
    public interface IEventGenerator<T> where T : IEvent
    {

    }

    public interface IPreProcessingEventGenerator<TEvent, TContext>
        : IEventGenerator<TEvent> 
          where TEvent : IEvent
          where TContext : IEventParserContext<TEvent>
    {
        /// <summary>
        /// Gets all available events
        /// </summary>
        /// <param name="context">Context</param>
        void GetAllEvents(TContext context);
    }

    /// <summary>
    /// Interface for event generators which generate all events in one go prior to parsing taking place
    /// </summary>
    public interface IRdfXmlPreProcessingEventGenerator : IPreProcessingEventGenerator<IRdfXmlEvent, RdfXmlParserContext>
    {
    }

    public interface IJitEventGenerator<T> : IEventGenerator<T> where T : IEvent
    {
        /// <summary>
        /// Gets the next available event
        /// </summary>
        /// <returns></returns>
        T GetNextEvent();

        /// <summary>
        /// Gets whether the Event Generator has finished reading events i.e. there are no further events available
        /// </summary>
        bool Finished
        {
            get;
        }
    }

    /// <summary>
    /// Interface for event generators which generate events as required during the parsing process
    /// </summary>
    public interface IRdfXmlJitEventGenerator : IJitEventGenerator<IRdfXmlEvent>
    {
    }
}
