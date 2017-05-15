using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface INamedAnnotationProperty
     : INamedProperty, IAnnotationProperty
    {

    }

    public static class NamedAnnotationProperties
    {
        public static INamedAnnotationProperty BackwardsCompatibleWith
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedAnnotationProperty Comment
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedAnnotationProperty IncompatibleWith
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedAnnotationProperty IsDefinedBy
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedAnnotationProperty Label
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedAnnotationProperty PriorVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedAnnotationProperty SeeAlso
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedAnnotationProperty VersionInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
