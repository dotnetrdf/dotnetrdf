/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
