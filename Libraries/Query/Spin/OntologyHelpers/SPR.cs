/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.OntologyHelpers
{
    /// <summary>
    /// Vocabulary for http://spinrdf.org/spr
    /// @author Holger Knublauch
    /// </summary>
    public class SPR
    {
        public const String BASE_URI = "http://spinrdf.org/spr";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "spr";

        public readonly static IUriNode ClassTable = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Table"));

        public readonly static IUriNode ClassTableClass = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TableClass"));

        public readonly static IUriNode PropertyCell = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "cell"));

        public readonly static IUriNode PropertyCellFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "cellFunction"));

        public readonly static IUriNode PropertyColCount = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "colCount"));

        public readonly static IUriNode PropertyColCountFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "colCountFunction"));

        public readonly static IUriNode PropertyColName = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "colName"));

        public readonly static IUriNode PropertyColNameFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "colNameFunction"));

        public readonly static IUriNode PropertyColTypeFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "colTypeFunction"));

        public readonly static IUriNode PropertyColWidthFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "colWidthFunction"));

        public readonly static IUriNode PropertyColType = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "colType"));

        public readonly static IUriNode PropertyColWidth = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "colWidth"));

        public readonly static IUriNode PropertyHasCell = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hasCell"));

        public readonly static IUriNode PropertyHasCellFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hasCellFunction"));

        public readonly static IUriNode PropertyRowCount = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "rowCount"));

        public readonly static IUriNode PropertyRowCountFunction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "rowCountFunction"));

#if UNDEFINED
    public static bool exists(Model model) {
    	return model.Contains(SPR.Table, RDF.type, (INode)null);
    }
#endif
    }
}