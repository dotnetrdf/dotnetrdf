using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class Annotation : IAnnotation
    {
        public IEnumerable<IAnnotation> Annotations { get; protected set; }

        public IAnnotationValue Value { get; protected set; }
    }
}
