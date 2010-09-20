using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Linq
{
    /// <summary>
    /// Possible ways to delete Objects
    /// </summary>
    public enum LinqDeleteMode
    {
        /// <summary>
        /// Delete only the specific values in the given Object
        /// </summary>
        DeleteValues,
        /// <summary>
        /// Delete all Triples where the Instance URI is the Subject/Object of a Triple
        /// </summary>
        DeleteAll
    }

    /// <summary>
    /// A LINQ Update Processor is a wrapper around a <see cref="ISparqlUpdateProcessor">ISparqlUpdateProcessor</see> and is used to save/delete Objects from an underlying store
    /// </summary>
    public class LinqUpdateProcessor
    {
        private ISparqlUpdateProcessor _underlyingProcessor;
        private SparqlFormatter _formatter = new SparqlFormatter();
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        public LinqUpdateProcessor(ISparqlUpdateProcessor processor)
        {
            this._underlyingProcessor = processor;
        }

        public LinqUpdateProcessor(IInMemoryQueryableStore store)
            : this(new LeviathanUpdateProcessor(store)) { }

        public LinqUpdateProcessor(String endpointUri)
            : this(new RemoteUpdateProcessor(endpointUri)) { }

        public LinqUpdateProcessor(Uri endpointUri)
            : this(new RemoteUpdateProcessor(endpointUri)) { }

        public LinqUpdateProcessor(SparqlRemoteUpdateEndpoint endpoint)
            : this(new RemoteUpdateProcessor(endpoint)) { }

        public LinqUpdateProcessor(IUpdateableGenericIOManager manager)
            : this(new GenericUpdateProcessor(manager)) { }


        /// <summary>
        /// Gets the Text of the SPARQL Update Command which will be used to Save the Object to the underlying Store
        /// </summary>
        /// <param name="oc">Object</param>
        /// <param name="graphUri">URI of the Graph to save to</param>
        /// <returns></returns>
        public String GetSaveCommandText(OwlInstanceSupertype oc, String graphUri)
        {
            if (oc == null) throw new ArgumentNullException("oc", "Cannot persist a Null Object");

            Type t = oc.GetType();
            PropertyInfo[] propInfo = t.GetProperties();

            //Start building an INSERT DATA command
            StringBuilder persistCommand = new StringBuilder();
            persistCommand.AppendLine("INSERT DATA {");

            //Need to add a GRAPH clause if a Graph URI has been provided
            if (!String.IsNullOrEmpty(graphUri))
            {
                persistCommand.Append("GRAPH <");
                persistCommand.Append(this._formatter.FormatUri(graphUri));
                persistCommand.AppendLine("> {");
            }

            //Assert the Type Triple
            persistCommand.Append(this.FormatAsUri(oc.InstanceUri));
            persistCommand.Append(" a ");
            persistCommand.Append(this.FormatAsUri(OwlClassSupertype.GetOwlClassUri(t)));
            persistCommand.AppendLine(" .");

            //Assert Triples for annotated properties
            Graph g = new Graph();
            foreach (PropertyInfo info in propInfo)
            {
                this.GetTripleText(oc, info, g, persistCommand);
            }

            //Need to append an extra } to close the GRAPH clause if using one
            if (!String.IsNullOrEmpty(graphUri))
            {
                persistCommand.AppendLine("}");
            }

            //Close the INSERT DATA command
            persistCommand.AppendLine("}");

            return persistCommand.ToString();
        }

        /// <summary>
        /// Gets the Text of the SPARQL Update Command which will be used to Delete the Object from the underlying Store
        /// </summary>
        /// <param name="oc">Object</param>
        /// <param name="graphUri">URI of the Graph to delete from</param>
        /// <param name="mode">Delete Mode</param>
        /// <returns></returns>
        public String GetDeletionCommandText(OwlInstanceSupertype oc, String graphUri, LinqDeleteMode mode)
        {
            if (oc == null) throw new ArgumentNullException("oc", "Cannot delete a Null Object");

            StringBuilder persistCommand = new StringBuilder();

            switch (mode)
            {
                case LinqDeleteMode.DeleteAll:
                    //Delete all the Triples with this Object's Instance URI as the Subject/Object

                    //Start building a pair of DELETE WHERE commands
                    persistCommand.AppendLine("DELETE WHERE {");

                    //Need to add a GRAPH clause if a Graph URI has been provided
                    if (!String.IsNullOrEmpty(graphUri))
                    {
                        persistCommand.Append("GRAPH <");
                        persistCommand.Append(this._formatter.FormatUri(graphUri));
                        persistCommand.AppendLine("> {");
                    }

                    persistCommand.Append(this.FormatAsUri(oc.InstanceUri));
                    persistCommand.AppendLine(" ?p ?o .");

                    //Need to append an extra } to close the GRAPH clause if using one
                    if (!String.IsNullOrEmpty(graphUri))
                    {
                        persistCommand.AppendLine("}");
                    }

                    //Close the first DELETE WHERE command
                    persistCommand.AppendLine("} ;");

                    //Create the second DELETE WHERE command
                    persistCommand.AppendLine("DELETE WHERE {");

                    //Need to add a GRAPH clause if a Graph URI has been provided
                    if (!String.IsNullOrEmpty(graphUri))
                    {
                        persistCommand.Append("GRAPH <");
                        persistCommand.Append(this._formatter.FormatUri(graphUri));
                        persistCommand.AppendLine("> {");
                    }

                    persistCommand.Append("?s ?p ");
                    persistCommand.AppendLine(this.FormatAsUri(oc.InstanceUri) + " .");

                    //Need to append an extra } to close the GRAPH clause if using one
                    if (!String.IsNullOrEmpty(graphUri))
                    {
                        persistCommand.AppendLine("}");
                    }

                    //Close the second DELETE WHERE command
                    persistCommand.AppendLine("} ;");

                    return persistCommand.ToString();

                case LinqDeleteMode.DeleteValues:
                    //Delete the specific Values associated with this Object

                    Type t = oc.GetType();
                    PropertyInfo[] propInfo = t.GetProperties();

                    //Start building an DELETE DATA command
                    persistCommand.AppendLine("DELETE DATA {");

                    //Need to add a GRAPH clause if a Graph URI has been provided
                    if (!String.IsNullOrEmpty(graphUri))
                    {
                        persistCommand.Append("GRAPH <");
                        persistCommand.Append(this._formatter.FormatUri(graphUri));
                        persistCommand.AppendLine("> {");
                    }

                    //Assert the Type Triple
                    persistCommand.Append(this.FormatAsUri(oc.InstanceUri));
                    persistCommand.Append(" a ");
                    persistCommand.Append(this.FormatAsUri(OwlClassSupertype.GetOwlClassUri(t)));
                    persistCommand.AppendLine(" .");

                    //Assert Triples for annotated properties
                    Graph g = new Graph();
                    foreach (PropertyInfo info in propInfo)
                    {
                        this.GetTripleText(oc, info, g, persistCommand);
                    }

                    //Need to append an extra } to close the GRAPH clause if using one
                    if (!String.IsNullOrEmpty(graphUri))
                    {
                        persistCommand.AppendLine("}");
                    }

                    //Close the DELETE DATA command
                    persistCommand.AppendLine("}");

                    return persistCommand.ToString();
                default:
                    throw new LinqToRdfException("Not a valid Linq Delete Mode");
            }
        }

        private void GetTripleText(OwlInstanceSupertype oc, PropertyInfo info, IGraph g, StringBuilder sb)
        {
            if (info.GetValue(oc, null) != null)
            {
                if (info.GetOwlResource() == null) return;

                sb.Append(this.FormatAsUri(oc.InstanceUri));
                sb.Append(' ');
                sb.Append(this.FormatAsUri(info.GetOwlResourceUri()));
                sb.Append(' ');
                String dtUri = info.GetDatatypeUri();
                if (dtUri == null)
                {
                    sb.Append(this.Format(g.CreateLiteralNode(info.GetValue(oc, null).ToString())));
                }
                else
                {
                    sb.Append(this.Format(g.CreateLiteralNode(info.GetValue(oc, null).ToString(), new Uri(dtUri))));
                }
                sb.AppendLine(" .");
            }
        }

        /// <summary>
        /// Saves an Object to the underlying Store using SPARQL Update
        /// </summary>
        /// <param name="oc">Object</param>
        /// <param name="graphUri">Graph URI</param>
        /// <remarks>
        /// Does not remove any previous version of the Object, to do this use the <see cref="LinqUpdateProcessor.DeleteObject">DeleteObject()</see> method prior to saving an Object
        /// </remarks>
        public void SaveObject(OwlInstanceSupertype oc, String graphUri)
        {
            if (oc == null) throw new ArgumentNullException("oc", "Cannot persist a Null Object");

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(this.GetSaveCommandText(oc, graphUri));
            this._underlyingProcessor.ProcessCommandSet(cmds);
        }

        /// <summary>
        /// Deletes an Object from the underlying Store using SPARQL Update
        /// </summary>
        /// <param name="oc">Object</param>
        /// <param name="graphUri">URI of the Graph to delete from</param>
        /// <param name="mode">Delete Mode</param>
        public void DeleteObject(OwlInstanceSupertype oc, String graphUri, LinqDeleteMode mode)
        {
            if (oc == null) throw new ArgumentNullException("oc", "Cannot delete a Null Object");

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(this.GetDeletionCommandText(oc, graphUri, mode));
            this._underlyingProcessor.ProcessCommandSet(cmds);
        }

        private String Format(INode n)
        {
            return this._formatter.Format(n);
        }

        private String FormatAsUri(String u)
        {
            return "<" + this._formatter.FormatUri(u) + ">";
        }
    }
}
