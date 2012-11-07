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

using System;
using VDS.RDF;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.Convert
{
    /// <summary>
    /// Interface for Conversion Options
    /// </summary>
    public interface IConversionOption
    {
        /// <summary>
        /// Applies the option to a RDF Writer
        /// </summary>
        /// <param name="writer">Writer</param>
        void Apply(IRdfWriter writer);

        /// <summary>
        /// Applies the option to a Store Writer
        /// </summary>
        /// <param name="writer">Writer</param>
        void Apply(IStoreWriter writer);
    }

    public class HighSpeedOption : IConversionOption
    {
        private bool _hiSpeedAllowed;

        public HighSpeedOption(bool allowed)
        {
            this._hiSpeedAllowed = allowed;
        }

        public void Apply(IRdfWriter writer)
        {
            if (writer is IHighSpeedWriter)
            {
                ((IHighSpeedWriter)writer).HighSpeedModePermitted = this._hiSpeedAllowed;
            }
        }

        public void Apply(IStoreWriter writer)
        {
            if (writer is IHighSpeedWriter)
            {
                ((IHighSpeedWriter)writer).HighSpeedModePermitted = this._hiSpeedAllowed;
            }
        }
    }

    public class PrettyPrintingOption : IConversionOption
    {
        private bool _prettyPrint;

        public PrettyPrintingOption(bool prettyPrint)
        {
            this._prettyPrint = prettyPrint;
        }

        public void Apply(IRdfWriter writer)
        {
            if (writer is IPrettyPrintingWriter)
            {
                ((IPrettyPrintingWriter)writer).PrettyPrintMode = this._prettyPrint;
            }
        }

        public void Apply(IStoreWriter writer)
        {
            if (writer is IPrettyPrintingWriter)
            {
                ((IPrettyPrintingWriter)writer).PrettyPrintMode = this._prettyPrint;
            }
        }
    }

    public class CompressionLevelOption : IConversionOption
    {
        private int _compressionLevel = WriterCompressionLevel.Default;

        public CompressionLevelOption(int compressionLevel)
        {
            this._compressionLevel = compressionLevel;
        }

        public void Apply(IRdfWriter writer)
        {
            if (writer is ICompressingWriter)
            {
                ((ICompressingWriter)writer).CompressionLevel = this._compressionLevel;
            }
        }

        public void Apply(IStoreWriter writer)
        {
            if (writer is ICompressingWriter)
            {
                ((ICompressingWriter)writer).CompressionLevel = this._compressionLevel;
            }
        }
    }

    public class StylesheetOption : IConversionOption
    {
        private String _stylesheet;

        public StylesheetOption(String stylesheet)
        {
            this._stylesheet = stylesheet;
        }

        #region IConversionOption Members

        public void Apply(IRdfWriter writer)
        {
            if (writer is IHtmlWriter)
            {
                ((IHtmlWriter)writer).Stylesheet = this._stylesheet;
            }
        }

        public void Apply(IStoreWriter writer)
        {
            if (writer is IHtmlWriter)
            {
                ((IHtmlWriter)writer).Stylesheet = this._stylesheet;
            }
        }

        #endregion
    }
}
