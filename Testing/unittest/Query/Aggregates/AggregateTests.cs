using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Aggregates
{
    [TestClass]
    public class AggregateTests
    {
        [TestMethod]
        public void SparqlAggregatesMaxBug1()
        {
            try
            {
                TripleStore store = new TripleStore();
                store.LoadFromUri(UriFactory.Create("http://semanticsage.home.lc/files/LearningStyles.rdf"));

                Options.AlgebraOptimisation = false;

                IGraph graph = store.ExecuteQuery(@"prefix sage:
<http://www.semanticsage.home.lc/LearningStyles.owl#>
prefix xsd: <http://www.w3.org/2001/XMLSchema#>
prefix : <http://semanticsage.home.lc/files/LearningStyles.rdf#>

CONSTRUCT
{
    ?MTech :max ?maxScore
}
WHERE
{
    SELECT ?MTech max(?max) as ?maxScore
    WHERE
    {
        SELECT ?MTech ?LessonType sum(?hasValue) as ?max
        WHERE
        {
            ?MTech sage:attendsLessons ?Lesson.
            ?Lesson sage:hasLessonType ?LessonType.
            ?MTech sage:undergoesEvaluation ?Quiz.
            ?Quiz sage:isForLesson ?Lesson.
            ?MTech sage:hasQuizMarks ?QuizMarks.
            ?QuizMarks sage:belongsToQuiz ?Quiz.
            ?QuizMarks sage:hasValue ?hasValue.
            ?Lesson sage:inRound '1'^^xsd:int.
        }
        GROUP BY ?MTech ?LessonType
    }
    GROUP BY ?MTech
}") as IGraph;
                Assert.IsNotNull(graph);

                NTriplesFormatter formatter = new NTriplesFormatter();
                INode zero = (0).ToLiteral(graph);
                foreach (Triple t in graph.Triples)
                {
                    Assert.IsFalse(t.Object.CompareTo(zero) == 0, "Unexpected zero value returned");
                }
            }
            finally
            {
                Options.AlgebraOptimisation = true;
            }
        }

        [TestMethod]
        public void SparqlAggregatesMaxBug2()
        {
            try
            {
                TripleStore store = new TripleStore();
                store.LoadFromUri(UriFactory.Create("http://semanticsage.home.lc/files/LearningStyles.rdf"));

                Options.AlgebraOptimisation = true;

                IGraph graph = store.ExecuteQuery(@"prefix sage:
<http://www.semanticsage.home.lc/LearningStyles.owl#>
prefix xsd: <http://www.w3.org/2001/XMLSchema#>
prefix : <http://semanticsage.home.lc/files/LearningStyles.rdf#>

CONSTRUCT
{
    ?MTech :max ?maxScore
}
WHERE
{
    SELECT ?MTech max(?max) as ?maxScore
    WHERE
    {
        SELECT ?MTech ?LessonType sum(?hasValue) as ?max
        WHERE
        {
            ?MTech sage:attendsLessons ?Lesson.
            ?Lesson sage:hasLessonType ?LessonType.
            ?MTech sage:undergoesEvaluation ?Quiz.
            ?Quiz sage:isForLesson ?Lesson.
            ?MTech sage:hasQuizMarks ?QuizMarks.
            ?QuizMarks sage:belongsToQuiz ?Quiz.
            ?QuizMarks sage:hasValue ?hasValue.
            ?Lesson sage:inRound '1'^^xsd:int.
        }
        GROUP BY ?MTech ?LessonType
    }
    GROUP BY ?MTech
}") as IGraph;
                Assert.IsNotNull(graph);

                NTriplesFormatter formatter = new NTriplesFormatter();
                INode zero = (0).ToLiteral(graph);
                foreach (Triple t in graph.Triples)
                {
                    Assert.IsFalse(t.Object.CompareTo(zero) == 0, "Unexpected zero value returned");
                }
            }
            finally
            {
                Options.AlgebraOptimisation = true;
            }
        }
    }
}
