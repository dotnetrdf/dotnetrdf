using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test.Actual
{
    public class SubjectLookupTest : IterationTest
    {
        private List<INode> _subjects = new List<INode>();

        public SubjectLookupTest(int iterations)
            : base("Subject Lookup", "Picks three random subjects from the Test Data and does lookup and iteration using GetTriplesWithSubject() on each", iterations, "Triples/Second") { }

        protected override void PreIterationSetup(TestCase testCase)
        {
            if (this._subjects.Count == 0)
            {
                Random rnd = new Random();
                while (this._subjects.Count < 3)
                {
                    int i = rnd.Next(1, (int) testCase.Instance.Count) - 1;
                    Triple t = testCase.Instance.Triples.Skip(i).FirstOrDefault();
                    if (t != null)
                    {
                        this._subjects.Add(t.Subject);
                    }
                }
            }
        }

        protected override long RunIteration(TestCase testCase)
        {
            return this._subjects.SelectMany(subj => testCase.Instance.GetTriplesWithSubject(subj)).LongCount();
        }
    }

    public class PredicateLookupTest : IterationTest
    {
        private readonly List<INode> _predicates = new List<INode>();

        public PredicateLookupTest(int iterations)
            : base("Predicate Lookup", "Picks three random predicates from the Test Data and does lookup and iteration using GetTriplesWithPredicate() on each", iterations, "Triples/Second") { }

        protected override void PreIterationSetup(TestCase testCase)
        {
            if (this._predicates.Count == 0)
            {
                Random rnd = new Random();
                while (this._predicates.Count < 3)
                {
                    int i = rnd.Next(1, (int) testCase.Instance.Count) - 1;
                    Triple t = testCase.Instance.Triples.Skip(i).FirstOrDefault();
                    if (t != null)
                    {
                        this._predicates.Add(t.Predicate);
                    }
                }
            }
        }

        protected override long RunIteration(TestCase testCase)
        {
            return this._predicates.SelectMany(subj => testCase.Instance.GetTriplesWithPredicate(subj)).Count();
        }
    }

    public class ObjectLookupTest : IterationTest
    {
        private readonly List<INode> _objects = new List<INode>();

        public ObjectLookupTest(int iterations)
            : base("Object Lookup", "Picks three random objects from the Test Data and does lookup and iteration using GetTriplesWithObject() on each", iterations, "Triples/Second") { }

        protected override void PreIterationSetup(TestCase testCase)
        {
            if (this._objects.Count == 0)
            {
                Random rnd = new Random();
                while (this._objects.Count < 3)
                {
                    int i = rnd.Next(1, (int) testCase.Instance.Count) - 1;
                    Triple t = testCase.Instance.Triples.Skip(i).FirstOrDefault();
                    if (t != null)
                    {
                        this._objects.Add(t.Object);
                    }
                }
            }
        }

        protected override long RunIteration(TestCase testCase)
        {
            return this._objects.SelectMany(obj => testCase.Instance.GetTriplesWithObject(obj)).LongCount();
        }
    }
}
