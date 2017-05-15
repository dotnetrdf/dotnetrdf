using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;

namespace VDS.OWL.Term.Entity
{
    public interface INamedEntity
        : IEntity, INamedTerm, IAnnotationValue
    {
        IEntityAnnotation CreateAnnotation(IAnnotationProperty p, IAnnotationValue value);

        IEntityAnnotation CreateAnnotation(IAnnotationProperty p, String value);

        T As<T>(T entityType);

        IEntityAnnotation CreateComment(IAnnotationValue value);

        IEntityAnnotation CreateComment(String value);

        IDeclaration CreateDeclaration();

        bool IsVariable
        {
            get;
        }

        IEntityAnnotation CreateLabel(IAnnotationValue value);

        IEntityAnnotation CreateLabel(String value);
    }
}
