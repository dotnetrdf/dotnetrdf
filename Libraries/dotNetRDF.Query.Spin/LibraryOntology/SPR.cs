/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/


using System;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Storage;
using VDS.RDF.Query.Spin.Util;
namespace VDS.RDF.Query.Spin.LibraryOntology
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


        public readonly static IUriNode ClassTable = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Table"));

        public readonly static IUriNode ClassTableClass = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TableClass"));

        public readonly static IUriNode PropertyCell = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "cell"));

        public readonly static IUriNode PropertyCellFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "cellFunction"));

        public readonly static IUriNode PropertyColCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colCount"));

        public readonly static IUriNode PropertyColCountFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colCountFunction"));

        public readonly static IUriNode PropertyColName = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colName"));

        public readonly static IUriNode PropertyColNameFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colNameFunction"));

        public readonly static IUriNode PropertyColTypeFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colTypeFunction"));

        public readonly static IUriNode PropertyColWidthFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colWidthFunction"));

        public readonly static IUriNode PropertyColType = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colType"));

        public readonly static IUriNode PropertyColWidth = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colWidth"));

        public readonly static IUriNode PropertyHasCell = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "hasCell"));

        public readonly static IUriNode PropertyHasCellFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "hasCellFunction"));

        public readonly static IUriNode PropertyRowCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "rowCount"));

        public readonly static IUriNode PropertyRowCountFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "rowCountFunction"));

/*
    public static bool exists(Model model) {
    	return model.Contains(SPR.Table, RDF.type, (INode)null);
    }
*/
    }
}
