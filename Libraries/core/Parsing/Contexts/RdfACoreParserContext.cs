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

            //Set up the Event Queue
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

            //Setup final things
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