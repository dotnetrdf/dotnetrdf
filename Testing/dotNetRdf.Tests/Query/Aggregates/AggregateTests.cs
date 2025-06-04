/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Globalization;
using System.IO;
using VDS.RDF.Parsing;
using Xunit;
using System.Threading;

namespace VDS.RDF.Query.Aggregates;

public class AggregateTests : IDisposable
{
    private readonly CultureInfo _previousCulture;

    public AggregateTests()
    {
        _previousCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
    }

    public void Dispose()
    {
        Thread.CurrentThread.CurrentCulture = _previousCulture;
    }

    private static IGraph ExecuteGraphQuery(IInMemoryQueryableStore store, string query, Action<LeviathanQueryOptions> options = null)
    {
        return ExecuteQuery(store, query, options) as IGraph;
    }

    private static object ExecuteQuery(IInMemoryQueryableStore store, string query, Action<LeviathanQueryOptions> options = null)
    {
        var qp = new LeviathanQueryProcessor(store, options);
        var parser = new SparqlQueryParser();
        var q = parser.ParseFromString(query);
        return qp.ProcessQuery(q);
    }

    [Fact]
    public void SparqlAggregatesMaxBug1()
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources", "LearningStyles.rdf"));

        
        IGraph graph = ExecuteGraphQuery(store, @"prefix sage:
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
}");
        Assert.NotNull(graph);

        INode zero = (0).ToLiteral(graph);
        foreach (Triple t in graph.Triples)
        {
            Assert.False(t.Object.CompareTo(zero) == 0, "Unexpected zero value returned");
        }
    }

    [Fact]
    public void SparqlAggregatesMaxBug2()
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources","LearningStyles.rdf"));

        IGraph graph = ExecuteGraphQuery(store, @"prefix sage:
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
}");
        Assert.NotNull(graph);

        INode zero = (0).ToLiteral(graph);
        foreach (Triple t in graph.Triples)
        {
            Assert.False(t.Object.CompareTo(zero) == 0, "Unexpected zero value returned");
        }
    }

    [Fact]
    public void SparqlAggregatesMaxBug3()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "LearningStyles.rdf"));
        Assert.False(g.IsEmpty);
        g.BaseUri = null;
        store.Add(g);

        var graph = ExecuteGraphQuery(store, @"
                    PREFIX sage: <http://www.semanticsage.home.lc/LearningStyles.owl#>
                    PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
                    PREFIX : <http://www.semanticsage.home.lc/LearningStyles.rdf#>
                    CONSTRUCT 
                    { 
                       ?MTech :max ?maxScore  
                    }
                    WHERE 
                    { 
                       SELECT ?MTech (MAX(?max) AS ?maxScore)
                       WHERE
                       {
                          SELECT ?MTech ?LessonType (SUM(?hasValue) AS ?max)
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
                    }", options => { options.AlgebraOptimisation = false;});

        Assert.False(graph.IsEmpty, "CONSTRUCTed graph should not be empty");

        // here a graph name is given to the result graph
        var graph2 = new Graph(new UriNode(new Uri("http://semanticsage.home.lc/files/LearningStyles.rdf#maxValues")));
        graph2.Assert(graph.Triples);
        store.Add(graph2, true);

        var actualResults = ExecuteQuery(store, @"
                    PREFIX sage: <http://www.semanticsage.home.lc/LearningStyles.owl#>
                    PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
                    PREFIX : <http://www.semanticsage.home.lc/LearningStyles.rdf#>
                    SELECT ?MTech ?LessonType ?max
                    WHERE
                    {
                       GRAPH <http://semanticsage.home.lc/files/LearningStyles.rdf#maxValues>
                       {
                          ?MTech :max ?max
                       }
                       {
                            SELECT ?MTech ?LessonType (SUM(?hasValue) AS ?Score)
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
                            ORDER BY ?MTech
                       }
                       FILTER(?Score = ?max)
                    }", options => { options.AlgebraOptimisation = false;}) as SparqlResultSet;
        Assert.NotNull(actualResults);
        Assert.False(actualResults.IsEmpty, "Final results should not be empty");
    }
}
