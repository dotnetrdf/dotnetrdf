using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF;

namespace VDS.RDF.Utilities.GraphBenchmarker.Test
{
    public class TestCase
    {
        private Type _graphType, _tripleCollectionType;
        private IGraph _instance;
        private BindingList<TestResult> _results = new BindingList<TestResult>();

        public TestCase(Type graphType)
        {
            this._graphType = graphType;
        }

        public TestCase(Type graphType, Type tripleCollectionType)
            : this(graphType)
        {
            this._tripleCollectionType = tripleCollectionType;
        }

        public IGraph Instance
        {
            get
            {
                if (this._instance == null)
                {
                    if (this._tripleCollectionType != null)
                    {
                        this._instance = (IGraph)Activator.CreateInstance(this._graphType, new Object[] { Activator.CreateInstance(this._tripleCollectionType) });
                    }
                    else
                    {
                        this._instance = (IGraph)Activator.CreateInstance(this._graphType);
                    }
                }
                return this._instance;
            }
        }

        public BindingList<TestResult> Results
        {
            get
            {
                return this._results;
            }
        }

        public void Reset(bool clearResults)
        {
            if (this._instance != null)
            {
                this._instance.Dispose();
                this._instance = null;
            }
            if (clearResults) this._results.Clear();
        }

        public override string ToString()
        {
            if (this._tripleCollectionType != null)
            {
                return this._graphType.FullName + " with " + this._tripleCollectionType.FullName;
            }
            else
            {
                return this._graphType.FullName;
            }
        }
    }
}
