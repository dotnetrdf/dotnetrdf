namespace Grom
{
    using System;
    using VDS.RDF;

    // TODO: Maybe no Engine but static builder on Node?
    public partial class Node
    {
        internal readonly INode graphNode;
        private readonly Engine context;

        internal Node(INode node, Engine context)
        {
            this.graphNode = node;
            this.context = context;
        }

        private object Convert(INode tripleObject)
        {
            if (tripleObject.NodeType == NodeType.Uri || tripleObject.NodeType == NodeType.Blank)
            {
                return this.context.GetByNode(tripleObject);
            }

            if (tripleObject is ILiteralNode literalObject)
            {
                if (literalObject.DataType != null)
                {
                    var dataType = literalObject.DataType.ToString();

                    // See https://www.w3.org/TR/xmlschema-2/#built-in-datatypes
                    if (dataType == "http://www.w3.org/2001/XMLSchema#int" || dataType == "http://www.w3.org/2001/XMLSchema#integer")
                    {
                        if (int.TryParse(literalObject.Value, out int literalResult))
                        {
                            return literalResult;
                        }
                    }

                    if (dataType == "http://www.w3.org/2001/XMLSchema#long")
                    {
                        if (long.TryParse(literalObject.Value, out long literalResult))
                        {
                            return literalResult;
                        }
                    }

                    if (dataType == "http://www.w3.org/2001/XMLSchema#decimal")
                    {
                        if (decimal.TryParse(literalObject.Value, out decimal literalResult))
                        {
                            return literalResult;
                        }
                    }

                    if (dataType == "http://www.w3.org/2001/XMLSchema#double")
                    {
                        if (double.TryParse(literalObject.Value, out double literalResult))
                        {
                            return literalResult;
                        }
                    }

                    if (dataType == "http://www.w3.org/2001/XMLSchema#boolean")
                    {
                        if (bool.TryParse(literalObject.Value, out bool literalResult))
                        {
                            return literalResult;
                        }
                    }

                    if (dataType == "http://www.w3.org/2001/XMLSchema#date")
                    {
                        if (DateTime.TryParse(literalObject.Value, out DateTime literalResult))
                        {
                            return literalResult;
                        }
                    }
                }

                return literalObject.Value;
            }

            throw new ArgumentOutOfRangeException("tripleObject", "Unrecognized node type.");
        }

        //public override bool Equals(object obj)
        //{
        //    if (!(obj is Node))
        //    {
        //        return false;
        //    }

        //    return this.node.Equals((obj as Node).node);
        //}

        //public static bool operator ==(Node first, Node second)
        //{
        //    return first.Equals(second);
        //}

        //public static bool operator !=(Node first, Node second)
        //{
        //    return !(first == second);
        //}

        //public override int GetHashCode()
        //{
        //    return this.node.GetHashCode();
        //}
    }
}
