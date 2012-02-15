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
        private Type _graphType, _tripleCollectionType, _nodeCollectionType;
        private IGraph _instance;
        private BindingList<TestResult> _results = new BindingList<TestResult>();
        private long _initMemory = 0;

        public TestCase(Type graphType)
        {
            this._graphType = graphType;
        }

        public TestCase(Type graphType, Type tripleCollectionType)
            : this(graphType)
        {
            this._tripleCollectionType = tripleCollectionType;
        }

        public TestCase(Type graphType, Type tripleCollectionType, Type nodeCollectionType)
            : this(graphType, tripleCollectionType)
        {
            this._nodeCollectionType = nodeCollectionType;
        }

        public IGraph Instance
        {
            get
            {
                if (this._instance == null)
                {
                    if (this._tripleCollectionType != null)
                    {
                        if (this._nodeCollectionType != null)
                        {
                            this._instance = (IGraph)Activator.CreateInstance(this._graphType, new Object[] { Activator.CreateInstance(this._tripleCollectionType) });
                        }
                        else
                        {
                            this._instance = (IGraph)Activator.CreateInstance(this._graphType, new Object[] { Activator.CreateInstance(this._tripleCollectionType), Activator.CreateInstance(this._nodeCollectionType) });
                        }
                    }
                    else if (this._nodeCollectionType != null)
                    {
                        this._instance = (IGraph)Activator.CreateInstance(this._graphType, new Object[] { Activator.CreateInstance(this._nodeCollectionType) });
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

        public long InitialMemory
        {
            get
            {
                return this._initMemory;
            }
            set
            {
                this._initMemory = value;
            }
        }

        public void Reset(bool clearResults)
        {
            if (this._instance != null)
            {
                this._instance.Dispose();
                this._instance = null;
                this._initMemory = 0;
            }
            if (clearResults) this._results.Clear();
        }

        public override string ToString()
        {
            if (this._tripleCollectionType != null)
            {
                if (this._nodeCollectionType != null)
                {
                    return this._graphType.Name + " with " + this._tripleCollectionType.Name + " and " + this._nodeCollectionType.Name;
                }
                else
                {
                    return this._graphType.Name + " with " + this._tripleCollectionType.Name;
                }
            }
            else if (this._nodeCollectionType != null)
            {
                return this._graphType.Name + " with " + this._nodeCollectionType.Name;
            }
            else
            {
                return this._graphType.Name;
            }
        }
    }
}
