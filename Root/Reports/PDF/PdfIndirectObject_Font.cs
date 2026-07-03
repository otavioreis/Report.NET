using System;
using System.Drawing;

// Creation date: 19.01.2006
// Checked: 29.07.2006
// Author: Otto Mayer (mot@root.ch)
// Version: 2.01

// Report.NET copyright � 2002-2006 root-software ag, B�rglen Switzerland - Otto Mayer, Stefan Spirig, all rights reserved
// This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation, version 2.1 of the License.
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. You
// should have received a copy of the GNU Lesser General Public License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA www.opensource.org/licenses/lgpl-license.html

namespace Root.Reports {
  //------------------------------------------------------------------------------------------29.07.2006
  #region PdfIndirectObject_Font
  //----------------------------------------------------------------------------------------------------

  /// <summary>PDF Indirect Object: Font</summary>
  /// <remarks>Each font data object that is used in the PDF document must point to an object of this type (FontData.oFontDataX).</remarks>
  internal abstract class PdfIndirectObject_Font : PdfIndirectObject {
    /// <summary>Font data</summary>
    protected readonly FontData fontData;

    internal readonly String sKey;

    /// <summary>This variable allows a quick test, whether the font properties are registered for the current page.
    /// If <c>pdfPageData_Registered</c> contains the current page, then it has been registered before.</summary>
    internal PdfIndirectObject_Page pdfIndirectObject_Page;

    //------------------------------------------------------------------------------------------29.07.2006
    /// <summary>Creates a font indirect object.</summary>
    /// <param name="pdfFormatter">PDF formatter</param>
    /// <param name="fontProp">Font property</param>
    internal PdfIndirectObject_Font(PdfFormatter pdfFormatter, FontData fontData) : base(pdfFormatter) {
      this.fontData = fontData;

      sKey = fontData.fontDef.sFontName;
      if ((fontData.fontStyle & FontStyle.Bold) > 0) {
        sKey += ";B";
      }
      if ((fontData.fontStyle & FontStyle.Italic) > 0) {
        sKey += ";I";
      }
    }
  }
  #endregion

  //------------------------------------------------------------------------------------------29.07.2006
  #region PdfIndirectObject_Font_Type1
  //----------------------------------------------------------------------------------------------------

  /// <summary>PDF Indirect Object: Font Type1</summary>
  internal sealed class PdfIndirectObject_Font_Type1 : PdfIndirectObject_Font {
    /// <summary>Font descriptor for an embedded font; null if the font is referenced by name only</summary>
    private readonly PdfIndirectObject_FontDescriptor_Embedded pdfIndirectObject_FontDescriptor;

    //------------------------------------------------------------------------------------------29.07.2006
    /// <summary>Creates a font indirect object for a Type1 font.</summary>
    /// <param name="pdfFormatter">PDF formatter</param>
    /// <param name="type1FontData">Type1 font data</param>
    internal PdfIndirectObject_Font_Type1(PdfFormatter pdfFormatter, Type1FontData type1FontData)
      : base(pdfFormatter, type1FontData)
    {
      if (type1FontData.bEmbedded) {
        pdfIndirectObject_FontDescriptor = new PdfIndirectObject_FontDescriptor_Embedded(pdfFormatter, type1FontData);
      }
    }

    //------------------------------------------------------------------------------------------29.07.2006
    /// <summary>Writes the object to the buffer.</summary>
    internal override void Write() {
      Type1FontData type1FontData = (Type1FontData)fontData;
      StartObj();
      Dictionary_Start();
      Dictionary_Key("Type");  Name("Font");
      if (pdfIndirectObject_FontDescriptor == null) {
        Dictionary_Key("Subtype");  Name("Type1");
        Dictionary_Key("BaseFont");  Name(type1FontData.sFontName);
        if (type1FontData.sFamilyName != "ZapfDingbats" && type1FontData.sFamilyName != "Symbol") {
          Dictionary_Key("Encoding");  Name("WinAnsiEncoding");
        }
      }
      else {
        Dictionary_Key("Subtype");  Name("TrueType");
        Dictionary_Key("BaseFont");  Name(type1FontData.sFontName);
        Dictionary_Key("Encoding");  Name("WinAnsiEncoding");
        Dictionary_Key("FontDescriptor");  IndirectReference(pdfIndirectObject_FontDescriptor);
        Dictionary_Key("FirstChar");  Number(32);
        Dictionary_Key("LastChar");  Number(255);
        Dictionary_Key("Widths");
        ArrayStart();
        for (Int32 iChar = 32;  iChar <= 255;  iChar++) {
          Space();
          Type1FontData.CharMetrics charMetrics = type1FontData.afmCharMetrics(iChar);
          Number(charMetrics == null ? 0 : (Int32)charMetrics.fWidthX);
        }
        ArrayEnd();
      }
      Dictionary_End();
      EndObj();
    }
  }
  #endregion

  //------------------------------------------------------------------------------------------xx.07.2026
  #region PdfIndirectObject_FontDescriptor_Embedded
  //----------------------------------------------------------------------------------------------------

  /// <summary>PDF Indirect Object: Font Descriptor with an embedded TrueType font program</summary>
  /// <remarks>The metric values are taken from the AFM definition of the font.</remarks>
  internal sealed class PdfIndirectObject_FontDescriptor_Embedded : PdfIndirectObject {
    /// <summary>Type1 font data</summary>
    private readonly Type1FontData type1FontData;

    /// <summary>Embedded font program that belongs to this font descriptor</summary>
    private readonly PdfIndirectObject_FontFile2 pdfIndirectObject_FontFile2;

    //------------------------------------------------------------------------------------------xx.07.2026
    /// <summary>Creates a font descriptor indirect object for an embedded font.</summary>
    /// <param name="pdfFormatter">PDF formatter</param>
    /// <param name="type1FontData">Type1 font data</param>
    internal PdfIndirectObject_FontDescriptor_Embedded(PdfFormatter pdfFormatter, Type1FontData type1FontData)
      : base(pdfFormatter)
    {
      this.type1FontData = type1FontData;
      pdfIndirectObject_FontFile2 = new PdfIndirectObject_FontFile2(pdfFormatter, type1FontData);
    }

    //------------------------------------------------------------------------------------------xx.07.2026
    /// <summary>Writes the object to the buffer.</summary>
    internal override void Write() {
      StartObj();
      Dictionary_Start();
      Dictionary_Key("Type");  Name("FontDescriptor");
      Dictionary_Key("FontName");  Name(type1FontData.sFontName);
      Int32 iFlags = 32;  // nonsymbolic
      if (type1FontData.bIsFixedPitch) {
        iFlags |= 1;
      }
      if (!Single.IsNaN(type1FontData.fItalicAngle) && type1FontData.fItalicAngle != 0) {
        iFlags |= 64;
      }
      Dictionary_Key("Flags");  Number(iFlags);
      Dictionary_Key("FontBBox");
      ArrayStart();
      Number((Int32)type1FontData.fFontBBox_llx);  Space();
      Number((Int32)type1FontData.fFontBBox_lly);  Space();
      Number((Int32)type1FontData.fFontBBox_urx);  Space();
      Number((Int32)type1FontData.fFontBBox_ury);
      ArrayEnd();
      Dictionary_Key("ItalicAngle");
      Number(Single.IsNaN(type1FontData.fItalicAngle) ? 0 : (Int32)type1FontData.fItalicAngle);
      Dictionary_Key("Ascent");
      Number(Single.IsNaN(type1FontData.fAscender) ? (Int32)type1FontData.fFontBBox_ury : (Int32)type1FontData.fAscender);
      Dictionary_Key("Descent");
      Number(Single.IsNaN(type1FontData.fDescender) ? (Int32)type1FontData.fFontBBox_lly : (Int32)type1FontData.fDescender);
      Dictionary_Key("CapHeight");
      Number(Single.IsNaN(type1FontData.fCapHeight) ? (Int32)type1FontData.fFontBBox_ury : (Int32)type1FontData.fCapHeight);
      Dictionary_Key("StemV");
      Number(Single.IsNaN(type1FontData.fStdVW) ? 80 : (Int32)type1FontData.fStdVW);
      Dictionary_Key("FontFile2");  IndirectReference(pdfIndirectObject_FontFile2);
      Dictionary_End();
      EndObj();
    }
  }
  #endregion

  //------------------------------------------------------------------------------------------xx.07.2026
  #region PdfIndirectObject_FontFile2
  //----------------------------------------------------------------------------------------------------

  /// <summary>PDF Indirect Object: embedded TrueType font program (FontFile2)</summary>
  /// <remarks>The font program is compressed with the flate (zlib) algorithm.</remarks>
  internal sealed class PdfIndirectObject_FontFile2 : PdfIndirectObject {
    /// <summary>Type1 font data</summary>
    private readonly Type1FontData type1FontData;

    //------------------------------------------------------------------------------------------xx.07.2026
    /// <summary>Creates a font file indirect object.</summary>
    /// <param name="pdfFormatter">PDF formatter</param>
    /// <param name="type1FontData">Type1 font data</param>
    internal PdfIndirectObject_FontFile2(PdfFormatter pdfFormatter, Type1FontData type1FontData)
      : base(pdfFormatter)
    {
      this.type1FontData = type1FontData;
    }

    //------------------------------------------------------------------------------------------xx.07.2026
    /// <summary>Writes the object to the buffer.</summary>
    internal override void Write() {
      Byte[] aByte_FontProgram = type1FontData.aByte_FontProgram;
      Byte[] aByte_Compressed = aByte_FlateCompress(aByte_FontProgram);

      StartObj();
      Dictionary_Start();
      Dictionary_Key("Filter");  Name("FlateDecode");
      Dictionary_Key("Length");  Number(aByte_Compressed.Length);
      Dictionary_Key("Length1");  Number(aByte_FontProgram.Length);
      Dictionary_End();
      NewLine();
      Command("stream");
      pdfFormatter.FlushBuffer();

      pdfFormatter.bufferedStream.Write(aByte_Compressed, 0, aByte_Compressed.Length);
      pdfFormatter.iBytesWrittenToStream += aByte_Compressed.Length;
      WriteLine("\nendstream");
      EndObj();
    }

    //------------------------------------------------------------------------------------------xx.07.2026
    /// <summary>Compresses the specified data with the flate algorithm (zlib format, RFC 1950).</summary>
    /// <param name="aByte">Uncompressed data</param>
    /// <returns>Compressed data with the zlib header and the Adler-32 checksum</returns>
    private static Byte[] aByte_FlateCompress(Byte[] aByte) {
      using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(aByte.Length / 2)) {
        // zlib header: deflate, 32K window, default compression level
        memoryStream.WriteByte(0x78);
        memoryStream.WriteByte(0x9C);
        using (System.IO.Compression.DeflateStream deflateStream =
          new System.IO.Compression.DeflateStream(memoryStream, System.IO.Compression.CompressionMode.Compress, true))
        {
          deflateStream.Write(aByte, 0, aByte.Length);
        }
        // Adler-32 checksum of the uncompressed data (big-endian)
        UInt32 uA = 1;
        UInt32 uB = 0;
        foreach (Byte byte_ in aByte) {
          uA = (uA + byte_) % 65521;
          uB = (uB + uA) % 65521;
        }
        memoryStream.WriteByte((Byte)(uB >> 8));
        memoryStream.WriteByte((Byte)uB);
        memoryStream.WriteByte((Byte)(uA >> 8));
        memoryStream.WriteByte((Byte)uA);
        return memoryStream.ToArray();
      }
    }
  }
  #endregion

  //------------------------------------------------------------------------------------------04.08.2006
  #region PdfIndirectObject_Font_OpenType
  //----------------------------------------------------------------------------------------------------

  /// <summary>PDF Indirect Object: Font Open Type</summary>
  internal sealed class PdfIndirectObject_Font_OpenType : PdfIndirectObject_Font {
    /// <summary>Font descriptor that belongs to this font type</summary>
    private readonly PdfIndirectObject_FontDescriptor pdfIndirectObject_FontDescriptor;

    //------------------------------------------------------------------------------------------04.08.2006
    /// <summary>Creates a font indirect object for an open type font.</summary>
    /// <param name="pdfFormatter">PDF formatter</param>
    /// <param name="openTypeFontData">Open type font data</param>
    internal PdfIndirectObject_Font_OpenType(PdfFormatter pdfFormatter, OpenTypeFontData openTypeFontData)
      : base(pdfFormatter, openTypeFontData)
    {
      pdfIndirectObject_FontDescriptor = new PdfIndirectObject_FontDescriptor(pdfFormatter, openTypeFontData);
    }

    //------------------------------------------------------------------------------------------01.02.2006
    /// <summary>Writes the object to the buffer.</summary>
    internal override void Write() {
      OpenTypeFontData openTypeFontData = (OpenTypeFontData)fontData;

      StartObj();
      Dictionary_Start();
      Dictionary_Key("Type");  Name("Font");
      Dictionary_Key("Subtype");  Name("TrueType");
      System.Diagnostics.Debug.Assert(openTypeFontData.sBaseFontName != null);
      Dictionary_Key("BaseFont");  Name(openTypeFontData.sBaseFontName);
      Dictionary_Key("Encoding");  Name("WinAnsiEncoding");
      Dictionary_Key("FontDescriptor");  IndirectReference(pdfIndirectObject_FontDescriptor);
      Int32 iFirstChar = openTypeFontData.iFirstChar;
      Dictionary_Key("FirstChar");  Number(iFirstChar);
      Int32 iLastChar = openTypeFontData.iLastChar;
      Dictionary_Key("LastChar");
      Number(iLastChar);
      Dictionary_Key("Widths");
      ArrayStart();
      for (int i = iFirstChar; i <= iLastChar; i++) {
        Space();
        Int32 iWidth = openTypeFontData.iGetRawWidth(iFirstChar + i);
        Number(iWidth);
      }
      ArrayEnd();
      Dictionary_End();
      EndObj();
    }
  }
  #endregion

  //------------------------------------------------------------------------------------------xx.02.2006
  #region PdfIndirectObject_FontDescriptor
  //----------------------------------------------------------------------------------------------------

  /// <summary>PDF Indirect Object: Font Descriptor</summary>
  internal sealed class PdfIndirectObject_FontDescriptor : PdfIndirectObject {
    /// <summary>Font property</summary>
    private readonly OpenTypeFontData openTypeFontData;

    //------------------------------------------------------------------------------------------04.05.2006
    /// <summary>Creates a font descriptor indirect object.</summary>
    /// <param name="pdfFormatter">PDF formatter</param>
    /// <param name="fontProp">Font property</param>
    internal PdfIndirectObject_FontDescriptor(PdfFormatter pdfFormatter, OpenTypeFontData openTypeFontData)
      : base(pdfFormatter)
    {
      this.openTypeFontData = openTypeFontData;
    }

//2 0 obj
//<</FontDescriptor 5 0 R
///BaseFont /PalatinoLinotype-Roman
///FirstChar 32
///Encoding /WinAnsiEncoding
///Subtype /TrueType
///Widths [250 0 0 0 0 0 0 208 0 0 0 0 0 0 250 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 708 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 612 0 0 0 0 0 0 0 0 0 0 0 0 500 0 443 0 479 333 0 582 291 0 0 291 882 582 545 601 560 395 423 326 603 0 0 0 556 0 0 0 0 0 0 500 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 277 0 0 0 0 0 0 0 0 0 0 0 0 0 333 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 500 0 0 0 500 0 0 443 0 479 0 0 0 0 0 0 0 0 0 0 0 0 545 0 0 0 0 0 603]
///LastChar 252
///Type /Font
//>>
//endobj

// 5 0 obj
// <</FontName /PalatinoLinotype-Roman
///StemV 80
// /Descent -284
// /Ascent 731
// /Flags 32
///ItalicAngle 0
// /CapHeight 699
// /FontBBox [-169 -291 1419 1049]
// /Type /FontDescriptor
// >>
// endobj

    //------------------------------------------------------------------------------------------xx.02.2006
    /// <summary>Writes the object to the buffer.</summary>
    internal override void Write() {
      //PdfFontPropX pdfFontPropX = (PdfFontPropX)fontProp.oFontPropX;
      //Type1FontData type1FontData = (Type1FontData)pdfFontPropData.fontData;

      StartObj();
      Dictionary_Start();
      Dictionary_Key("Type");  Name("FontDescriptor");
      //Dictionary_Key("FontName");  Name(openTypeFontData.sFullFontName);
      Dictionary_Key("FontName");  Name("PalatinoLinotype-Roman");
      Int32 iFlags = 0;
      if (openTypeFontData.bFixedPitch) {
        iFlags |= 1;
      }
      //if (openTypeFontData.bFontSpecific) {
      //  iFlags |= 4;
      //}
      //else {
      //  iFlags |= 32;
      //}
      //if (openTypeFontData.rItalicAngle < 0) {
      //  iFlags |= 64;
      //}
      //  iFlags |= 131072;
      //if (sWeight.Equals("Bold")) {
      //  iFlags |= 262144;
      //}
      iFlags = 32;  // !!!
      Dictionary_Key("Flags");  Number(iFlags);
      Dictionary_Key("Ascent");  Number(/*openTypeFontData.fAscender*/731);
      Dictionary_Key("CapHeight");  Number(/*openTypeFontData.fCapHeight*/699);
      Dictionary_Key("Descent");  Number(/*openTypeFontData.fDescender*/-284);
      Dictionary_Key("FontBBox");
      ArrayStart();
      Number(/*openTypeFontData.fFontBBox_llx*/-169);
      Space();
        Number(/*openTypeFontData.fFontBBox_lly*/-291);  Space();
        Number(/*openTypeFontData.fFontBBox_urx*/1419);  Space();
        Number(/*openTypeFontData.fFontBBox_ury*/1049);
      ArrayEnd();
      Dictionary_Key("ItalicAngle");  Number(openTypeFontData.rItalicAngle);
      Dictionary_Key("StemV");  Number(/*openTypeFontData.fStdVW*/80);
      // FontFile
      Dictionary_End();
      EndObj();
    }
  }
  #endregion
}
