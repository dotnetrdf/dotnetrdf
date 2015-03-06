

using System;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.OntologyHelpers
{
    /**
     * Vocabulary for http://spinrdf.org/spinmap
     *
     * Automatically generated with TopBraid Composer.
     */
    public class SPINMAP
    {

        public const String BASE_URI = "http://spinrdf.org/spinmap";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "spinmap";

        public const String TARGET_PREDICATE = "targetPredicate";

        public const String SOURCE_PREDICATE = "sourcePredicate";

        public readonly static IUriNode ClassContext = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Context"));

        public readonly static IUriNode ClassMapping = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Mapping"));

        public readonly static IUriNode ClassMapping_0_1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Mapping-0-1"));

        public readonly static IUriNode ClassMapping_1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Mapping-1"));

        public readonly static IUriNode ClassMapping_1_1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Mapping-1-1"));

        public readonly static IUriNode ClassMapping_2_1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Mapping-2-1"));

        public readonly static IUriNode ClassTargetFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TargetFunction"));

        public readonly static IUriNode ClassTargetFunctions = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TargetFunctions"));

        public readonly static IUriNode ClassTransformationFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TransformationFunction"));

        public readonly static IUriNode ClassTransformationFunctions = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TransformationFunctions"));

        public readonly static IUriNode PropertyContext = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "context"));

        public readonly static IUriNode PropertyEquals = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "equals"));

        public readonly static IUriNode PropertyExpression = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "expression"));

        public readonly static IUriNode PropertyFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "function"));

        public readonly static IUriNode PropertyInverseExpression = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "inverseExpression"));

        public readonly static IUriNode PropertyPostRule = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "postRule"));

        public readonly static IUriNode PropertyPredicate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "predicate"));

        public readonly static IUriNode PropertyPrepRule = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "prepRule"));

        public readonly static IUriNode PropertyRule = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "rule"));

        public readonly static IUriNode PropertyShortLabel = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "shortLabel"));

        public readonly static IUriNode PropertySource = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "source"));

        public readonly static IUriNode PropertySourceClass = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "sourceClass"));

        public readonly static IUriNode PropertySourcePredicate1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + SOURCE_PREDICATE + "1"));

        public readonly static IUriNode PropertySourcePredicate2 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + SOURCE_PREDICATE + "2"));

        public readonly static IUriNode PropertySourcePredicate3 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + SOURCE_PREDICATE + "3"));

        public readonly static IUriNode PropertySourceVariable = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "_source"));

        public readonly static IUriNode PropertySuggestion_0_1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "suggestion-0-1"));

        public readonly static IUriNode PropertySuggestion_1_1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "suggestion-1-1"));

        public readonly static IUriNode PropertySuggestionScore = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "suggestionScore"));

        public readonly static IUriNode PropertyTarget = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "target"));

        public readonly static IUriNode PropertyTargetClass = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "targetClass"));

        public readonly static IUriNode PropertyTargetPredicate1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + TARGET_PREDICATE + "1"));

        public readonly static IUriNode PropertyTargetPredicate2 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + TARGET_PREDICATE + "2"));

        public readonly static IUriNode PropertyTargetResource = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "targetResource"));

        public readonly static IUriNode PropertyTemplate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "template"));

        public readonly static IUriNode PropertyType = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "type"));

        public readonly static IUriNode PropertyValue = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "value"));

        public readonly static IUriNode PropertyValue1 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "value1"));

        public readonly static IUriNode PropertyValue2 = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "value2"));


#if UNDEFINED
        public static bool exists(Model model)
        {
            return model.Contains(model.Object(SPINMAP.BASE_URI), RDF.type, OWL.Ontology);
        }


        public static String getURI()
        {
            return NS_URI;
        }
#endif
    }
}
