using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class AnonymousIndividual : Individual, IAnnotationValue, IAnnotationSubject
    {
        public String NodeID { get; protected set; }
    }
}
