using VDS.RDF.Storage;
using System.Collections.Generic;
using System;
using VDS.RDF.Query.Spin;
namespace org.topbraid.spin.arq
{

    /**
     * A Dataset that simply delegates all its calls, allowing to wrap an existing
     * Dataset (e.g. the TopBraid Dataset).
     * 
     * @author Holger Knublauch
     */
    public abstract class DelegatingDataset : Dataset
    {

        private Dataset _delegate;

        public DelegatingDataset(Dataset _delegate)
        {
            this._delegate = _delegate;
        }

        override public DatasetGraph asDatasetGraph()
        {
            return null;
            //return new DatasetGraphBase() {

            //    override 		public void close() {
            //        DelegatingDataset.this.close();
            //    }

            //    override 		public bool containsGraph(INode graphNode) {
            //        return DelegatingDataset.this.containsNamedModel(graphNode.Uri);
            //    }

            //    override 		public Graph getDefaultGraph() {
            //        IUpdateableStorage defaultModel = DelegatingDataset.this.getDefaultModel();
            //        if(defaultModel != null) {
            //            return defaultModel.getGraph();
            //        }
            //        else {
            //            return null;
            //        }
            //    }

            //    override 		public Graph getGraph(INode graphNode) {
            //        IUpdateableStorage model = DelegatingDataset.this.getNamedModel(graphNode.Uri);
            //        if(model != null) {
            //            return model.getGraph();
            //        }
            //        else {
            //            return null;
            //        }
            //    }

            //    override 		public Lock getLock() {
            //        return DelegatingDataset.this.getLock();
            //    }

            //    override 		public IEnumerator<INode> listGraphNodes() {
            //        List<INode> results = new List<INode>();
            //        IEnumerator<String> names = DelegatingDataset.this.listNames();
            //        while(names.MoveNext()) {
            //            String name = names.Current;
            //            results.Add(UriFactory.Create(name));
            //        }
            //        return results.GetEnumerator();
            //    }

            //    override 		public long size() {
            //        int count = 0;
            //        IEnumerator<INode> it = listGraphNodes();
            //        while(it.MoveNext()) {
            //            it.Current;
            //            count++;
            //        }
            //        return count;
            //    }

            //    override 		public IEnumerator<Quad> find(INode g, INode s, INode p, INode o) {
            //        return null;
            //    }

            //    override 		public IEnumerator<Quad> findNG(INode g, INode s, INode p,
            //            INode o) {
            //        return null;
            //    }
            //};
        }

        override public void close()
        {
            _delegate.close();
        }

        override public bool containsNamedModel(String uri)
        {
            return _delegate.containsNamedModel(uri);
        }

        override public IUpdateableStorage getDefaultModel()
        {
            return _delegate.getDefaultModel();
        }

        public Dataset getDelegate()
        {
            return _delegate;
        }

        override public Lock getLock()
        {
            return _delegate.getLock();
        }

        override public IUpdateableStorage getNamedModel(String uri)
        {
            return _delegate.getNamedModel(uri);
        }

        override public IEnumerator<String> listNames()
        {
            return _delegate.listNames();
        }


        override public void setDefaultModel(IUpdateableStorage model)
        {
            _delegate.setDefaultModel(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="model"></param>
        /// throws LabelExistsException
        override public void addNamedModel(String uri, IUpdateableStorage model)
        {
            _delegate.addNamedModel(uri, model);
        }


        override public void removeNamedModel(String uri)
        {
            _delegate.removeNamedModel(uri);
        }


        override public void replaceNamedModel(String uri, IUpdateableStorage model)
        {
            _delegate.replaceNamedModel(uri, model);
        }


        override public Context getContext()
        {
            return _delegate.getContext();
        }


        override public bool supportsTransactions()
        {
            return _delegate.supportsTransactions();
        }


        override public void begin(ReadWrite readWrite)
        {
            _delegate.begin(readWrite);
        }


        override public void commit()
        {
            _delegate.commit();
        }


        override public void abort()
        {
            _delegate.abort();
        }


        override public bool isInTransaction()
        {
            return _delegate.isInTransaction();
        }


        override public void end()
        {
            _delegate.end();
        }
    }
}