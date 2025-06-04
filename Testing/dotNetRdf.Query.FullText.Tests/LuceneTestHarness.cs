/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
#if !NO_FULLTEXT

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using VDS.RDF.Query.FullText.Schema;


namespace VDS.RDF.Query.FullText;

public class LuceneTestHarness
{
    private bool _init = false;
    private Directory _indexDir;
    private IFullTextIndexSchema _schema;
    private Analyzer _analyzer;

    public readonly static Lucene.Net.Util.LuceneVersion LuceneVersion = Lucene.Net.Util.LuceneVersion.LUCENE_48;

    public LuceneTestHarness()
    {
        _indexDir = new RAMDirectory();
        _schema = new DefaultIndexSchema();
        _analyzer = new StandardAnalyzer(LuceneVersion);
        _init = true;
    }

    public Directory Index
    {
        get => _indexDir;
    }

    public IFullTextIndexSchema Schema
    {
        get => _schema;
    }

    public Analyzer Analyzer
    {
        get => _analyzer;
    }
}
#endif