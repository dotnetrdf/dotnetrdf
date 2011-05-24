using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    int i = rnd.Next(1, testCase.Instance.Triples.Count) - 1;
                    Triple t = testCase.Instance.Triples.Skip(i).FirstOrDefault();
                    if (t != null)
                    {
                        this._subjects.Add(t.Subject);
                    }
                }
            }
        }

        protected override int RunIteration(TestCase testCase)
        {
            int actions = 0;
            foreach (INode subj in this._subjects)
            {
                foreach (Triple t in testCase.Instance.GetTriplesWithSubject(subj))
                {
                    actions++;
                }
            }
            return actions;
        }
    }

    public class PredicateLookupTest : IterationTest
    {
        private List<INode> _predicates = new List<INode>();

        public PredicateLookupTest(int iterations)
            : base("Predicate Lookup", "Picks three random predicates from the Test Data and does lookup and iteration using GetTriplesWithPredicate() on each", iterations, "Triples/Second") { }

        protected override void PreIterationSetup(TestCase testCase)
        {
            if (this._predicates.Count == 0)
            {
                Random rnd = new Random();
                while (this._predicates.Count < 3)
                {
                    int i = rnd.Next(1, testCase.Instance.Triples.Count) - 1;
                    Triple t = testCase.Instance.Triples.Skip(i).FirstOrDefault();
                    if (t != null)
                    {
                        this._predicates.Add(t.Predicate);
                    }
                }
            }
        }

        protected override int RunIteration(TestCase testCase)
        {
            int actions = 0;
            foreach (INode subj in this._predicates)
            {
                foreach (Triple t in testCase.Instance.GetTriplesWithPredicate(subj))
                {
                    actions++;
                }
            }
            return actions;
        }
    }

    public class ObjectLookupTest : IterationTest
    {
        private List<INode> _objects = new List<INode>();

        public ObjectLookupTest(int iterations)
            : base("Object Lookup", "Picks three random objects from the Test Data and does lookup and iteration using GetTriplesWithObject() on each", iterations, "Triples/Second") { }

        protected override void PreIterationSetup(TestCase testCase)
        {
            if (this._objects.Count == 0)
            {
                Random rnd = new Random();
                while (this._objects.Count < 3)
                {
                    int i = rnd.Next(1, testCase.Instance.Triples.Count) - 1;
                    Triple t = testCase.Instance.Triples.Skip(i).FirstOrDefault();
                    if (t != null)
                    {
                        this._objects.Add(t.Object);
                    }
                }
            }
        }

        protected override int RunIteration(TestCase testCase)
        {
            int actions = 0;
            foreach (INode obj in this._objects)
            {
                foreach (Triple t in testCase.Instance.GetTriplesWithObject(obj))
                {
                    actions++;
                }
            }
            return actions;
        }
    }
}
