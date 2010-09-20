using System;
using System.Collections.Generic;
using System.Diagnostics;
using VDS.RDF;

namespace rdfMetal
{
    public class ClassDetailQuerySink : Graph
    {
        public List<Uri> ls = new List<Uri>();

        #region Graph Members

        public bool Add(Triple s)
        {
            string output = string.Format("{0} {1} {2}", s.Subject, s.Predicate, s.Object);
            Debug.WriteLine(output);
            return true;
        }

        #endregion
    }
}