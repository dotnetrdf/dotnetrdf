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

#if UNFINISHED

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Events;
using VDS.RDF.Parsing.Events.RdfA;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for the host language independent RDFa 1.1 Core parser
    /// </summary>
    /// <remarks>
    /// <para>
    /// Compared to the original RDFa parser this context provides for much more efficient parsing since it uses efficient data structures which avoid unecessarily replicating data
    /// </para>
    /// </remarks>
    public class RdfACoreParserContext : IParserContext, IEventParserContext<IRdfAEvent>
    {
        private IRdfHandler _handler;
        private bool _traceParsing = false;
        private IEventQueue<IRdfAEvent> _events;
        private IRdfAHostLanguage _language;

        private NestedReference<Uri> _baseUri;
        private NestedReference<Uri> _defaultVocabUri;
        private NestedReference<Uri> _defaultPrefixMapping = new NestedReference<Uri>(new Uri(RdfAParser.XHtmlVocabNamespace));
        private NestedReference<String> _literalLang = new NestedReference<string>(String.Empty);
        private NestedNamespaceMapper _nsmap = new NestedNamespaceMapper(true);
        private NestedNamespaceMapper _termMap = new NestedNamespaceMapper(true);
        private INode _parentSubj, _parentObj;
        private List<IncompleteTriple> _incompleteTriples = new List<IncompleteTriple>();
        private bool _specialBNodeSeen = false;

        public RdfACoreParserContext(IGraph g, IRdfAHostLanguage language, TextReader reader, bool traceParsing)
            : this(new GraphHandler(g), language, reader, traceParsing) { }

        public RdfACoreParserContext(IGraph g, IRdfAHostLanguage language, TextReader reader)
            : this(g, language, reader, false) { }

        public RdfACoreParserContext(IRdfHandler handler, IRdfAHostLanguage language, TextReader reader, bool traceParsing)
        {
            this._handler = handler;
            this._traceParsing = traceParsing;
            this._language = language;

            // Set up the Event Queue
            IEventGenerator<IRdfAEvent> generator = this._language.GetEventGenerator(reader);
            if (generator is IJitEventGenerator<IRdfAEvent>)
            {
                this._events = new StreamingEventQueue<IRdfAEvent>((IJitEventGenerator<IRdfAEvent>)generator);
            }
            else if (generator is IPreProcessingEventGenerator<IRdfAEvent, RdfACoreParserContext>)
            {
                this._events = new EventQueue<IRdfAEvent>(generator);
                ((IPreProcessingEventGenerator<IRdfAEvent, RdfACoreParserContext>)generator).GetAllEvents(this);
            }
            this._events = new DualEventQueue<IRdfAEvent>(this._events);

            // Setup final things
            this._baseUri = new NestedReference<Uri>((this._language.InitialBaseUri != null ? this._language.InitialBaseUri : this._handler.GetBaseUri()));
            this._defaultVocabUri = new NestedReference<Uri>(this._language.DefaultVocabularyUri);
        }

        public RdfACoreParserContext(IRdfHandler handler, IRdfAHostLanguage language, TextReader reader)
            : this(handler, language, reader, false) { }

        #region Parser Properties

        public IRdfHandler Handler
        {
            get
            { 
                return this._handler; 
            }
        }

        public bool TraceParsing
        {
            get
            {
                return this._traceParsing;
            }
            set
            {
                this._traceParsing = value;
            }
        }

        public IEventQueue<IRdfAEvent> Events
        {
            get
            {
                return this._events;
            }
        }

        public IRdfAHostLanguage HostLanguage
        {
            get
            {
                return this._language;
            }
        }

        #endregion

        #region Evaluation Context Properties

        public INamespaceMapper Namespaces
        {
            get 
            { 
                return this._nsmap; 
            }
        }

        public INamespaceMapper Terms
        {
            get
            {
                return this._termMap;
            }
        }

        public Uri BaseUri
        {
            get
            {
                return this._baseUri.Value;
            }
            set
            {
                this._baseUri.Value = value;
            }
        }

        public Uri DefaultPrefixMapping
        {
            get
            {
                return this._defaultPrefixMapping.Value;
            }
            set
            {
                this._defaultPrefixMapping.Value = value;
            }
        }

        public Uri DefaultVocabularyUri
        {
            get
            {
                return this._defaultVocabUri.Value;
            }
            set
            {
                this._defaultVocabUri.Value = value;
            }
        }

        public String Language
        {
            get
            {
                return this._literalLang.Value;
            }
            set
            {
                this._literalLang.Value = value;
            }
        }

        public INode ParentSubject
        {
            get
            {
                return this._parentSubj;
            }
            set
            {
                this._parentSubj = value;
            }
        }

        public INode ParentObject
        {
            get
            {
                return this._parentObj;
            }
            set
            {
                this._parentObj = value;
            }
        }

        public List<IncompleteTriple> IncompleteTriples
        {
            get
            {
                return this._incompleteTriples;
            }
        }

        public bool SpecialBNodeSeen
        {
            get
            {
                return this._specialBNodeSeen;
            }
            set
            {
                if (!this._specialBNodeSeen)
                {
                    this._specialBNodeSeen = value;
                }
            }
        }

        #endregion

        public void Requeue(IRdfAEvent evt)
        {
            if (this._events is DualEventQueue<IRdfAEvent>)
            {
                ((DualEventQueue<IRdfAEvent>)this._events).Requeue(evt);
            }
            else
            {
                throw new RdfParseException("Cannot requeue an event as the event queue is not of the correct type");
            }
        }

        public void IncrementNesting()
        {
            this._nsmap.IncrementNesting();
            this._termMap.IncrementNesting();
            this._baseUri.IncrementNesting();
            this._defaultVocabUri.IncrementNesting();
            this._literalLang.IncrementNesting();
        }

        public void DecrementNesting()
        {
            this._nsmap.DecrementNesting();
            this._termMap.DecrementNesting();
            this._baseUri.DecrementNesting();
            this._defaultVocabUri.DecrementNesting();
            this._literalLang.DecrementNesting();
        }

    }
}

#endif