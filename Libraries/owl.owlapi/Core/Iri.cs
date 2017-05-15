using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class Iri : IAnnotationValue, IAnnotationSubject
    {
        public Uri Value { get; protected set; }
    }
}
