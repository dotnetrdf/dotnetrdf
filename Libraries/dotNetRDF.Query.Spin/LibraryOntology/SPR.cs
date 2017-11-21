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
    internal class SPR
    {

        public const String BASE_URI = "http://spinrdf.org/spr";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "spr";


        public static readonly IUriNode ClassTable = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Table"));

        public static readonly IUriNode ClassTableClass = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "TableClass"));

        public static readonly IUriNode PropertyCell = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "cell"));

        public static readonly IUriNode PropertyCellFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "cellFunction"));

        public static readonly IUriNode PropertyColCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colCount"));

        public static readonly IUriNode PropertyColCountFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colCountFunction"));

        public static readonly IUriNode PropertyColName = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colName"));

        public static readonly IUriNode PropertyColNameFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colNameFunction"));

        public static readonly IUriNode PropertyColTypeFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colTypeFunction"));

        public static readonly IUriNode PropertyColWidthFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colWidthFunction"));

        public static readonly IUriNode PropertyColType = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colType"));

        public static readonly IUriNode PropertyColWidth = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "colWidth"));

        public static readonly IUriNode PropertyHasCell = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "hasCell"));

        public static readonly IUriNode PropertyHasCellFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "hasCellFunction"));

        public static readonly IUriNode PropertyRowCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "rowCount"));

        public static readonly IUriNode PropertyRowCountFunction = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "rowCountFunction"));

/*
    public static bool exists(Model model) {
    	return model.Contains(SPR.Table, RDF.type, (INode)null);
    }
*/
    }
}
