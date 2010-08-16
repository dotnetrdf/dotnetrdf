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
    public interface IEventGenerator
    {

    }

    /// <summary>
    /// Interface for event generators which generate all events in one go prior to parsing taking place
    /// </summary>
    public interface IPreProcessingEventGenerator : IEventGenerator
    {
        /// <summary>
        /// Gets all the Events
        /// </summary>
        /// <param name="context">Parser Context</param>
        void GetAllEvents(RdfXmlParserContext context);
    }

    /// <summary>
    /// Interface for event generators which generate events as required during the parsing process
    /// </summary>
    public interface IJitEventGenerator : IEventGenerator
    {
        /// <summary>
        /// Causes the Event Generator to attempt to retrieve the next Event
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException">Thrown if there is a problem reading the input</exception>
        /// <exception cref="XmlException">Thrown if there is a problem with the XML being read</exception>
        IRdfXmlEvent GetNextEvent();

        /// <summary>
        /// Gets whether the Event Generator has finished reading events i.e. there are no further events available
        /// </summary>
        bool Finished
        {
            get;
        }
    }
}
