/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
    public interface IEventGenerator<T> 
        where T : IEvent
    {

    }

    /// <summary>
    /// Interface for pre-processing event generators
    /// </summary>
    /// <typeparam name="TEvent">Event Type</typeparam>
    /// <typeparam name="TContext">Event Parser Context Type</typeparam>
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
    /// Interface for event generators which generate all RDF/XML events in one go prior to parsing taking place
    /// </summary>
    public interface IRdfXmlPreProcessingEventGenerator
        : IPreProcessingEventGenerator<IRdfXmlEvent, RdfXmlParserContext>
    {
    }

    /// <summary>
    /// Interface for Just-in-time event generators
    /// </summary>
    /// <typeparam name="T">Event Type</typeparam>
    public interface IJitEventGenerator<T>
        : IEventGenerator<T> 
          where T : IEvent
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
    /// Interface for RDF/XML event generators which generate events as required during the parsing process
    /// </summary>
    public interface IRdfXmlJitEventGenerator 
        : IJitEventGenerator<IRdfXmlEvent>
    {
    }
}
