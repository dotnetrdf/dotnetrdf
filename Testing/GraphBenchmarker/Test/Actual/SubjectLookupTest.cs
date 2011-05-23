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
            : base("Subject Lookup", "Picks three random subjects from the Test Data and does lookup and iteration using GetTriplesWithSubject() on each", iterations, "Triples") { }

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
}
