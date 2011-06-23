using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Events;

namespace VDS.RDF.Parsing.Events.RdfA
{
    public interface IRdfAHostLanguage
    {
        void InitTermMappings(RdfACoreParserContext context);

        String DefaultVocabularyPrefix
        {
            get;
        }

        IEventGenerator<IRdfAEvent> GetEventGenerator(TextReader reader);
    }
}
