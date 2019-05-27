using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using TiffBinaryReader.TagTypeEnums;

namespace TiffBinaryReader
{
  public class TagMetadata
  {
    #region Constants
    const int BytesPerBigTIFFTag = 20;
    const int BytesPerRegTIFFTag = 12;
    #endregion

    // Processed tags
    private List<Tag> _tags = new List<Tag>();
    private ReadOnlyCollection<Tag> _roTags;
    public ReadOnlyCollection<Tag> Tags => _roTags;

    public bool CanReadData { get; private set; } = false;
    public string ErrorStatus { get; private set; }

    // Well known, (probably) necessary tag values.
    public long DirectoryOffset { get; internal set; }     // Byte location in the file where dir starts
    public long TagCount { get; internal set; }            // Number of tags in this dir.
    public long NextDirectoryOffset { get; internal set; } // Byte location in file where *next* dir starts. 0 if no more dirs.
    public long ImageWidth { get; internal set; } = long.MinValue;       // Pixels wide, entire image.
    public long ImageHeight { get; internal set; } = long.MinValue;      // Pixels tall, entire image.
    public int BitsPerSample { get; internal set; } = int.MinValue;      // Bits per value: 32 = 4-byte, etc.
    public int SamplesPerPixel { get; internal set; } = int.MinValue;    // Values per 'pixel', e.g. 1 = elev, 3 = RGB, etc.
    public SampleFormats SampleFormat { get; internal set; } = SampleFormats.IEEEFP;  // Usually, float vs integer.
    public Orientations Orientation { get; internal set; } = Orientations.TopLeft;    // How are the values represented spatially? e.g. Tile[0] = TopLeft
    public PlanarConfigs PlanarConfiguration { get; internal set; } = PlanarConfigs.Contiguous;
    public SubfileTypes SubfileType { get; internal set; } = SubfileTypes.None; // Supposed to be 1-hot, really just use Overlay/None?
    public bool IsTiled { get; internal set; } = false;
    public bool IsCompressed { get; internal set; } = false; // If the tag DNE, assume uncompressed?
    public CompressionTypes CompressionType { get; internal set; } = CompressionTypes.Uncompressed;
    public FillOrders FillOrder { get; internal set; } = FillOrders.MSBFirst;
    public string NoData { get; internal set; }


    // Only valid if IsTiled = true
    public int TileWidth { get; internal set; } = int.MinValue;
    public int TileHeight { get; internal set; } = int.MinValue;
    public int TilesWide => IsTiled ? (int)Math.Ceiling(ImageWidth / (float)TileWidth) : 0;
    public int TilesTall => IsTiled ? (int)Math.Ceiling(ImageHeight / (float)TileHeight) : 0;    
    public int TileCount => TilesWide * TilesTall;
    public int NumberCellsPerTile => IsTiled ? TileWidth * TileHeight : 0;


    internal long[] TileByteOffsets = null;   // If tiled, where do they start?
    internal long[] TileByteCounts = null;    // If tiled, how big is each one?



    private int BytesPerTag
    {
      get
      {
        if (_isBigTiff)
          return BytesPerBigTIFFTag;
        else
          return BytesPerRegTIFFTag;
      }
    }                // 12 for reg-tiff, 20 for big-tiff.
    private byte[] _tagBytes;                 // Byte array of tag defs, 20 bytes per tag (for BT)
    private bool _isBigTiff;


    public static TagMetadata Read(BinaryReader br, long dirStartPosition, bool isBigTiff) => new TagMetadata(br, dirStartPosition, isBigTiff);

    private TagMetadata(BinaryReader br, long dirStartPosition, bool isBigTiff) : this(isBigTiff)
    {
      DirectoryOffset = dirStartPosition;

      // Jump to the header position
      br.BaseStream.Position = dirStartPosition;

      if (_isBigTiff)
      {
        // The next 8 bytes denote the number of tags. (BIG NUMBER)
        TagCount = (long)br.ReadUInt64();

        // Read in the byte definitions of all tags, 20 bytes each.
        _tagBytes = br.ReadBytes((int)TagCount * BytesPerTag);

        // 0 if there's no 'next' directory
        NextDirectoryOffset = (long)br.ReadUInt64();
      }
      else
      {
        // The next 2 bytes denote the number of tags. (up to 65536).
        TagCount = (long)br.ReadUInt16();

        // Read in the byte definitions of all tags, 12 bytes each.
        _tagBytes = br.ReadBytes((int)TagCount * BytesPerTag);

        // 0 if there's no 'next' directory
        NextDirectoryOffset = (long)br.ReadUInt32();
      }

      ImportTags(br);
      ProcessTagMetadata();
    }
    private TagMetadata(bool isBigTiff)
    {
      _isBigTiff = isBigTiff;
      _roTags = new ReadOnlyCollection<Tag>(_tags);
    }

    public static TagMetadata ForWriting(long imgWidth, long imgHeight, bool isOverlay, float noData)
    {
      var meta = new TagMetadata(true);

      // Input params
      meta.ImageHeight = imgHeight;
      meta.ImageWidth = imgWidth;
      meta.NoData = noData.ToString();
      if (isOverlay) meta.SubfileType = SubfileTypes.Page;

      // Default writing params
      meta.CompressionType = CompressionTypes.Deflate;
      meta.IsTiled = true;
      meta.IsCompressed = true;
      meta.BitsPerSample = 32;
      meta.SamplesPerPixel = 1;

      // Only write in 256x256 increments for now.
      const int TileSize = 256;
      meta.TileWidth = TileSize;
      meta.TileHeight = TileSize;
      meta.TileByteOffsets = new long[meta.TileCount];
      meta.TileByteCounts = new long[meta.TileCount];


      //if (meta.ValidateCanRead() == false)
      //  throw new Exception("hey");

      // To complete this object, the writer needs to add...
      // TileByteOffsets, TileByteCounts

      return meta;
    }
    /// <summary>
    /// The inverse process of importing - if we've edited any property-tags (i.e. creating a file), 
    /// we write-back to the tag properties.
    /// </summary>
    /// <param name="bw"></param>
    /// <param name="extraTags"></param>
    /// <param name="tileByteOffsets"></param>
    /// <param name="tileByteCounts"></param>
    /// <returns>The file-offset where the caller needs to write the *next* directory offset, if it exists. (Function writes a '0' there.) </returns>
    public long AppendDirectory(FileStream fs, List<Tag> extraTags, long[] tileByteOffsets, long[] tileByteCounts)
    {
      _tags.Clear();

      // Necessary to cast long->ulong, so we get the right unsigned data-type on import.
      _tags.Add(new Tag(TagTypes.ImageWidth, DataTypes.UInt, new object[] { (uint)ImageWidth }));      // LibTiff says it *must* be uint, not ulong etc.
      _tags.Add(new Tag(TagTypes.ImageLength, DataTypes.UInt, new object[] { (uint)ImageHeight }));    //
      _tags.Add(new Tag(TagTypes.BitsPerSample, DataTypes.UInt, new object[] { (uint)BitsPerSample }));
      _tags.Add(new Tag(TagTypes.SamplesPerPixel, DataTypes.UInt, new object[] { (uint)SamplesPerPixel }));
      _tags.Add(new Tag(TagTypes.SampleFormat, DataTypes.UShort, new object[] { (ushort)SampleFormat }));

      _tags.Add(new Tag(TagTypes.FillOrder, DataTypes.UShort, new object[] { (ushort)FillOrder }));
      _tags.Add(new Tag(TagTypes.Compression, DataTypes.UShort, new object[] { (ushort)CompressionType }));
      _tags.Add(new Tag(TagTypes.Orientation, DataTypes.UShort, new object[] { (ushort)Orientation }));
      _tags.Add(new Tag(TagTypes.SubfileType, DataTypes.UShort, new object[] { (ushort)SubfileType}));
      _tags.Add(new Tag(TagTypes.PlanarConfig, DataTypes.UShort, new object[] { (ushort)PlanarConfiguration }));
      if(NoData != "") _tags.Add(new Tag(TagTypes.GDALNoData, DataTypes.Ascii, new object[] { NoData }));

      _tags.Add(new Tag(TagTypes.TileWidth, DataTypes.UInt, new object[] { (uint)TileWidth }));
      _tags.Add(new Tag(TagTypes.TileLength, DataTypes.UInt, new object[] { (uint)TileHeight }));

      object[] tbOff = new object[tileByteOffsets.Length];
      for (int i = 0; i < tbOff.Length; i++)
        tbOff[i] = (object)(ulong)tileByteOffsets[i];
      _tags.Add(new Tag(TagTypes.TileByteOffsets, DataTypes.ULong8, tbOff));
      
      object[] tbCt = new object[tileByteCounts.Length];
      for (int i = 0; i < tbCt.Length; i++)
        tbCt[i] = (object)(ulong)tileByteCounts[i];
      _tags.Add(new Tag(TagTypes.TileByteCounts, DataTypes.ULong8, tbCt));

      // TESTING TESTING TESTING
      // DANGER REMOVE THIS
      var asdf = new object[1];
      asdf[0] = "HEC-RAS 5.0.2"; // sample
      _tags.Add(new Tag(TagTypes.Artist, DataTypes.Ascii, asdf));

      var aa = new object[1];
      aa[0] = "Depth grid"; // sample
      _tags.Add(new Tag(TagTypes.ImageDescription, DataTypes.Ascii, aa));

      // User defined tags
      if (extraTags != null) _tags.AddRange(extraTags); // Check for dupes?
      TagCount = _tags.Count;


      //              ----- BIGTIFF -----
      //     2            2           8           8
      // [Tag Index] [Data Type] [Data Count] [Location]
      
      byte[] tagSpace = new byte[BytesPerTag * TagCount];
      List<byte> tagFalloutSpace = new List<byte>();
      int tagStartIdx = 0;

      // After we write <(ulong)TagCount>, <tagSpace[]>, <(ulong)NextDirOffset>, we have free space to write extra tag data.
      long nextDataWriteStartLocation = fs.Length + sizeof(ulong)*2 + tagSpace.Length;
      
      // List of data to be appended once the directory is written. Push to file in-order after the directory header.
      var toBeAppended = new List<byte[]>();

      foreach (Tag t in Tags.OrderBy(t=>t.TagType.ID))
      {
        // Write them out ordered numerically - I think that's techincally to-spec.
        byte[] tagBytes = BitConverter.GetBytes((ushort)t.TagType.ID);
        tagSpace[tagStartIdx] = tagBytes[0];
        tagSpace[tagStartIdx + 1] = tagBytes[1];

        byte[] dtBytes = BitConverter.GetBytes((ushort)t.ActualDataType.UID);
        tagSpace[tagStartIdx + 2] = dtBytes[0];
        tagSpace[tagStartIdx + 3] = dtBytes[1];

        byte[] valuesAsBytes = t.ActualDataType.GetBytes(t.Values);
        ulong valueCt = (ulong)(valuesAsBytes.Length / t.ActualDataType.ByteCount);
        
        byte[] valueCtBytes = BitConverter.GetBytes(valueCt);
        tagSpace[tagStartIdx + 4] = valueCtBytes[0];
        tagSpace[tagStartIdx + 5] = valueCtBytes[1];
        tagSpace[tagStartIdx + 6] = valueCtBytes[2];
        tagSpace[tagStartIdx + 7] = valueCtBytes[3];
        tagSpace[tagStartIdx + 8] = valueCtBytes[4];
        tagSpace[tagStartIdx + 9] = valueCtBytes[5];
        tagSpace[tagStartIdx + 10] = valueCtBytes[6];
        tagSpace[tagStartIdx + 11] = valueCtBytes[7];

        if (valuesAsBytes.Length <= 8)
        {
          // Write it to this space directly, left-justified. 
          // (I.E. 4-byte value gets written to first 4-bytes of this space)
          for(int i = 0; i < valuesAsBytes.Length; i++)
            tagSpace[tagStartIdx + 12 + i] = valuesAsBytes[i];
        }
        else
        {
          // Data is too large, needs to be appended.
          toBeAppended.Add(valuesAsBytes);

          // It's going to be written starting-from the next available byte in the file.
          byte[] writeToLoc = BitConverter.GetBytes((ulong)nextDataWriteStartLocation);
          for (int i = 0; i < writeToLoc.Length; i++)
            tagSpace[tagStartIdx + 12 + i] = writeToLoc[i];

          nextDataWriteStartLocation += valuesAsBytes.Length;
        }

        tagStartIdx += BytesPerTag;
      }


      // Jump to the end to append our directory.
      DirectoryOffset = fs.Length;
      fs.Seek(0, SeekOrigin.End);
      byte[] tagCtBytes = BitConverter.GetBytes((ulong)TagCount);
      fs.Write(tagCtBytes, 0, tagCtBytes.Length);

      fs.Write(tagSpace, 0, tagSpace.Length);
      var nextDirOffsetLoc = fs.Length; // This is where the caller should write our next-directory-index.

      byte[] zeroBytes = BitConverter.GetBytes((ulong)0);
      fs.Write(zeroBytes, 0, zeroBytes.Length); // Write a 0, no next-dir by default. (Can be overwritten by caller - see return-value)

      // Write the extra buffers that couldn't fit in the [8] byte chunks
      foreach(var arr in toBeAppended)
        fs.Write(arr, 0, arr.Length);
      
      return nextDirOffsetLoc;
    }

    #region Parsing Directory Headers
    // http://bigtiff.org/#FILE_FORMAT
    void ImportTags(BinaryReader br)
    {
      //  https://partners.adobe.com/public/developer/en/tiff/TIFF6.pdf with http://bigtiff.org/#FILE_FORMAT
      //
      //              ----- BIGTIFF -----
      //     2            2           8           8
      // [Tag Index] [Data Type] [Data Count] [Location]
      // 2 bytes are the tag index.
      // 2 bytes specify the field type
      // 8 bytes specify count of [field type]
      // 8 bytes specify either:
      //    a) the data, if it takes <= 8 bytes. Or,
      //    b) the file offset to the location of the data.
      //
      // Note: If data is less than 8 bytes, it's left-justified. 
      // (I.E. Take first 2 bytes of those 8 to read a short)


      //           ----- REGULAR TIFF -----       
      //     2           2            4           4
      // [Tag Index] [Data Type] [Data Count] [Location]
      // 2 bytes are the tag index.
      // 2 bytes specify the field type
      // 4 bytes specify count of [field type]
      // 4 bytes specify either:
      //    a) the data, if it takes <= 4 bytes. Or,
      //    b) the file offset to the location of the data.
      for (int i = 0; i < _tagBytes.Length; i += BytesPerTag)
      {
        byte[] tagDef = new byte[BytesPerTag];
        for (int x = 0; x < BytesPerTag; x++)
        {
          tagDef[x] = _tagBytes[x + i];
        }

        ushort tagIdx = BitConverter.ToUInt16(tagDef, 0);
        ushort dtID = BitConverter.ToUInt16(tagDef, 2);

        // Only real difference: 4 bytes for reg-tiff, 8 for bigtiff
        ulong dataCount = 0;
        if (_isBigTiff)
          dataCount = BitConverter.ToUInt64(tagDef, 4);
        else
          dataCount = BitConverter.ToUInt32(tagDef, 4);

        // Convert integer to a known data-type
        DataType dt = DataTypes.Get(dtID);

        // Find this tag in our list of well-known tags.
        TagType knownTagType = TagTypes.TagList.FirstOrDefault(tt => tt.ID == tagIdx);

        // Eventually store in a list of unknown tags
        if (knownTagType == null)
        {
          Debug.WriteLine("Tag index " + tagIdx.ToString() + " unknown, skipping.");
          continue;
        }

        // Violation of spec? Should we try to survive in certain cases? 
        // If ImageLength is written as a signed int, but the value is positive... failing seems pedantic.
        if (!knownTagType.DataTypesSupported.Contains(dt))
        {
          Debug.WriteLine("Tag index " + tagIdx.ToString() + " shouldn't have data-type " + dt.Description + ", skipping.");
          continue;
        }

        if (dataCount > knownTagType.MaxAllowableValues)
        {
          Debug.WriteLine("Tag index " + tagIdx.ToString() + " can't support " + dataCount.ToString() + " values, skipping.");
          continue;
        }

        // Read values, create the tag.
        Tag tag = knownTagType.CreateTag(dt, ReadData((uint)dataCount, dt, tagDef, br, _isBigTiff));
        _tags.Add(tag);
      }
    }
    void ProcessTagMetadata()
    {
      // Mandatory tags
      var imgWdTag = FindTag(TagTypes.ImageWidth);
      var imgLnTag = FindTag(TagTypes.ImageLength);
      var bpsTag = FindTag(TagTypes.BitsPerSample);
      var sppTag = FindTag(TagTypes.SamplesPerPixel);
      var frmtTag = FindTag(TagTypes.SampleFormat);

      // Unsure if mandatory tags
      var fillTag = FindTag(TagTypes.FillOrder);
      var compTag = FindTag(TagTypes.Compression);
      var orienTag = FindTag(TagTypes.Orientation);
      var subFTag = FindTag(TagTypes.SubfileType);
      var configTag = FindTag(TagTypes.PlanarConfig);

      // Optional tags
      var tileLTag = FindTag(TagTypes.TileLength);
      var tileWTag = FindTag(TagTypes.TileWidth);
      var tileOffTag = FindTag(TagTypes.TileByteOffsets);
      var tileByteCtTag = FindTag(TagTypes.TileByteCounts);
      var nodataTag = FindTag(TagTypes.GDALNoData);

      // Fatal error conditions
      {
        string err = "";
        Action<TagType> AddMissingError = (tt) => err += "Missing " + tt.Name + "; ";
        if (imgWdTag == null)
          AddMissingError(TagTypes.ImageWidth);
        if (imgLnTag == null)
          AddMissingError(TagTypes.ImageLength);
        if (bpsTag == null)
          AddMissingError(TagTypes.BitsPerSample);
        if (sppTag == null)
          AddMissingError(TagTypes.SamplesPerPixel);
        if (frmtTag == null)
          AddMissingError(TagTypes.SampleFormat);

        // Further validation?
        if (err != "")
        {
          CanReadData = false;
          ErrorStatus = err;
          return;
        }
      }


      ImageWidth = imgWdTag.ValueAs<long>();
      ImageHeight = imgLnTag.ValueAs<long>();
      BitsPerSample = bpsTag.ValueAs<int>();
      SamplesPerPixel = sppTag.ValueAs<int>();
      SampleFormat = (SampleFormats)frmtTag.ValueAs<ushort>();


      // Fill-Order
      if (fillTag == null)
        FillOrder = FillOrders.MSBFirst; // Fairly safe assumption
      else
        FillOrder = (FillOrders)fillTag.ValueAs<ushort>();


      // Subfile type
      if (subFTag == null)
        SubfileType = SubfileTypes.None;
      else
        SubfileType = (SubfileTypes)subFTag.ValueAs<ushort>();


      // Compression
      if (compTag == null)
        CompressionType = CompressionTypes.Uncompressed;
      else
        CompressionType = (CompressionTypes)compTag.ValueAs<ushort>();

      if (CompressionType == CompressionTypes.Uncompressed)
        IsCompressed = false;
      else
        IsCompressed = true;


      // Orientation
      if (orienTag == null)
        Orientation = Orientations.TopLeft;
      else
        Orientation = (Orientations)orienTag.ValueAs<ushort>();


      // PlanarConfig
      if (configTag == null)
        PlanarConfiguration = PlanarConfigs.Contiguous; // Pretty good guess
      else
        PlanarConfiguration = (PlanarConfigs)configTag.ValueAs<ushort>();


      // NoData
      if (nodataTag != null)
        NoData = nodataTag.ValueAsString();


      // Tiling
      if (tileLTag != null)
        TileHeight = tileLTag.ValueAs<int>();
      if (tileWTag != null)
        TileWidth = tileWTag.ValueAs<int>();
      if (tileOffTag != null)
        TileByteOffsets = tileOffTag.ValuesAs<long>();
      if (tileByteCtTag != null)
        TileByteCounts = tileByteCtTag.ValuesAs<long>();

      if (tileLTag != null && tileWTag != null && tileOffTag != null && tileByteCtTag != null)
        IsTiled = true;
      else
        IsTiled = false;

      ValidateCanRead();
    }
    
    private bool ValidateCanRead()
    {
      // Soft error conditions - we can't currently read this data.
      string err = "";
      Action<string> AddCantReadErr = (msg) => err += "Can't read: " + msg + "; ";
      Action<string> AddMalformedErr = (msg) => err += "Malformed file: " + msg + "; ";

      // Fatal errors
      if (ImageHeight <= long.MinValue)
        AddCantReadErr("ImageHeight not set");
      if (ImageWidth <= 0)
        AddCantReadErr("ImageWidth not set");
      if (BitsPerSample <= 0)
        AddCantReadErr("BitsPerSample not set");
      if (SamplesPerPixel <= 0)
        AddCantReadErr("Samples per pixel not set");

      // Nonfatal errors - we just can't read at this time.
      if (CompressionType != CompressionTypes.Deflate)
        AddCantReadErr("Compression is not 'Deflate'");
      if (FillOrder != FillOrders.MSBFirst)
        AddCantReadErr("FillOrder is not 'MSBFirst'");
      if (SampleFormat != SampleFormats.IEEEFP)   // Eventually read ints...
        AddCantReadErr("Format is not 'IEEEFP'");
      if (IsTiled == false)
        AddCantReadErr("Data is not tiled"); // Should we try to fuss with untiled data?
      if (IsTiled && TileByteOffsets.Length != TileByteCounts.Length)
        AddMalformedErr("TileByteOffsets size is not equal to TileByteCounts size.");
      if (Orientation != Orientations.TopLeft)
        AddCantReadErr("Data is not top-left oriented.");
      if (SamplesPerPixel != 1)
        AddCantReadErr("Can only read 1 sample-per-pixel."); // Fix for RGB data?
      if (SamplesPerPixel != 1 && PlanarConfiguration != PlanarConfigs.Contiguous) // Tag only matters if SPP > 1
        AddCantReadErr("Can only read Contiguous configurations, e.g. RGB.RGB.RGB.");
      if (BitsPerSample != 32) // Eventually read doubles...
        AddCantReadErr("Can only read 32-bit data at this time.");

      // Further validation?
      if (err != "")
      {
        CanReadData = false;
        ErrorStatus = err;
      }
      else
        CanReadData = true;

      return CanReadData;
    }

    private static object[] ReadData(uint dataCount, DataType dt, byte[] tagData, BinaryReader br, bool isBigTiff)
    {
      if (isBigTiff)
      {
        const int DataOrOffset = 12;
        int byteCount = (int)(dataCount * dt.ByteCount);
        if (byteCount <= 8)
        {
          // Interpret values directly
          return dt.Parse(tagData, DataOrOffset, dataCount);
        }
        else
        {
          // Use this as an offset
          long offset = (long)BitConverter.ToUInt64(tagData, DataOrOffset);
          br.BaseStream.Position = offset;
          byte[] buf = new byte[byteCount];
          br.Read(buf, 0, byteCount);

          // Offset is 0 for the byte buffer we're returning
          return dt.Parse(buf, 0, dataCount);
        }
      }
      else
      {
        const int DataOrOffset = 8;
        int byteCount = (int)(dataCount * dt.ByteCount);
        if (byteCount <= 4)
        {
          // I think this is correct to spec, but not sure if any implementation does this:
          // Technically, a 2-char ascii string should be written to the tagData portion
          return dt.Parse(tagData, DataOrOffset, dataCount);
        }
        else
        {
          // Dont use tagData or tagOffset
          uint offset = BitConverter.ToUInt32(tagData, DataOrOffset);
          br.BaseStream.Position = offset;
          byte[] buf = new byte[byteCount];
          br.Read(buf, 0, byteCount);

          // Offset is 0 for the byte buffer we're returning
          return dt.Parse(buf, 0, dataCount);
        }
      }

    }
    private static string ReadDataPretty(uint dataCount, DataType dt, byte[] tagData, BinaryReader br, bool isBigTiff)
    {
      object[] values = ReadData(dataCount, dt, tagData, br, isBigTiff);
      //
      var sb = new StringBuilder();
      for (int i = 0; i < values.Length; i++)
      {
        sb.Append(values[i].ToString());
        if (i != values.Length - 1)
          sb.Append(", ");
      }
      return sb.ToString();
    }
    
    // Helper function to search tags
    public Tag FindTag(TagType type) => Tags.FirstOrDefault(t => t.TagType == type);
    #endregion

  }
  
}
