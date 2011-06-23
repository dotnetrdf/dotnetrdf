using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Events;

namespace VDS.RDF.Parsing.Events.RdfA
{
    /// <summary>
    /// Interface for RDFa Host Languages
    /// </summary>
    public interface IRdfAHostLanguage
    {
        /// <summary>
        /// Initialises the Term Mappings
        /// </summary>
        /// <param name="context">Parser Context</param>
        void InitTermMappings(RdfACoreParserContext context);

        /// <summary>
        /// Gets the Default Vocabulary URI for the host language
        /// </summary>
        Uri DefaultVocabularyUri
        {
            get;
        }

        /// <summary>
        /// Gets the Initial Base URI for the host language (if any)
        /// </summary>
        /// <remarks>
        /// The value of this is typically not available until after the <see cref="IRdfAHostLanguage.GetEventGenerator">GetEventGenerator()</see> method has been called
        /// </remarks>
        Uri InitialBaseUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the declared RDFa Version (if any)
        /// </summary>
        /// <remarks>
        /// The value of this is typically not available until after the <see cref="IRdfAHostLanguage.GetEventGenerator">GetEventGenerator()</see> method has been called
        /// </remarks>
        String Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an event generator
        /// </summary>
        /// <param name="reader">Input</param>
        /// <returns></returns>
        IEventGenerator<IRdfAEvent> GetEventGenerator(TextReader reader);

        /// <summary>
        /// Parses any extensions that a host language supports
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evt">Event</param>
        void ParseExtensions(RdfACoreParserContext context, IRdfAEvent evt);

        /// <summary>
        /// Parses any literal language specifier on the current event
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evt">Event</param>
        void ParseLiteralLanguage(RdfACoreParserContext context, IRdfAEvent evt);

        /// <summary>
        /// Parses any custom prefix mappings mechanism that the host language supports
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evt">Event</param>
        void ParsePrefixMappings(RdfACoreParserContext context, IRdfAEvent evt);

        /// <summary>
        /// Gets whether an event is considered to be a root
        /// </summary>
        /// <param name="evt">Event</param>
        /// <returns></returns>
        bool IsRootElement(IRdfAEvent evt);
    }

    /// <summary>
    /// Abstract Base Class for RDFa Host Languages
    /// </summary>
    public abstract class BaseHost : IRdfAHostLanguage
    {
        public virtual void InitTermMappings(RdfACoreParserContext context)
        {}

        public virtual Uri DefaultVocabularyUri
        {
            get 
            {
                return null;
            }
        }

        public virtual Uri InitialBaseUri
        {
            get 
            {
                return null;
            }
            set { }
        }

        public virtual String Version
        {
            get
            {
                return null;
            }
            set { }
        }

        public abstract IEventGenerator<IRdfAEvent> GetEventGenerator(TextReader reader);

        public virtual void ParseExtensions(RdfACoreParserContext context, IRdfAEvent evt)
        { }

        public virtual void ParseLiteralLanguage(RdfACoreParserContext context, IRdfAEvent evt)
        { }

        public virtual void ParsePrefixMappings(RdfACoreParserContext context, IRdfAEvent evt)
        { }

        public virtual bool IsRootElement(IRdfAEvent evt)
        {
            return false;
        }
    }
}
