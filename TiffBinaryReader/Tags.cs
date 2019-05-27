using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Numerics;

namespace TiffBinaryReader
{
  /// <summary>
  /// Static class to control/own all TagType(s). Any new TagTypes created will be registered to its global list.
  /// </summary>
  public static class TagTypes
  {
    private const string NO_DESC = "No description available.";

    // EWWWWW. Have to depend on C# class declaration order for this to be initialized and register-able.
    // Classes are parsed top-to-bottom.
    private static List<TagType> _tagList = new List<TagType>();

    // Does this work? Declaration-order dependent. Outside callers can't clear, etc.
    public static ReadOnlyCollection<TagType> TagList = new ReadOnlyCollection<TagType>(_tagList);


    // Enumerate common tag-types.

    // Data

    // For some reason, these have to be Long8, not ULong8 -_-
    public static TagType ImageWidth = TagType.CreateNew(nameof(ImageWidth), TagValues.IMAGEWIDTH, DataTypes.AllIntegralTypes, "Width of the raster, in pixels", 1);
    public static TagType ImageLength = TagType.CreateNew(nameof(ImageLength), TagValues.IMAGELENGTH, DataTypes.AllIntegralTypes, "Height of the raster, in pixels", 1);

    public static TagType BitsPerSample = TagType.CreateNew(nameof(BitsPerSample), TagValues.BITSPERSAMPLE, DataTypes.AllUnsignedIntegralTypes, "Number of bits (Normally 8/16/32/64) to represent each value.", 1);
    public static TagType SubfileType = TagType.CreateNew(nameof(SubfileType), TagValues.SUBFILETYPE, DataTypes.AllUnsignedIntegralTypes, "Indicates the type of data in this page - overview, mask, etc.", 1);
    public static TagType Compression = TagType.CreateNew(nameof(Compression), TagValues.COMPRESSION, DataTypes.AllUnsignedIntegralTypes, "Defines the compression type used for this page.", 1);
    public static TagType PhotometricInterpID = TagType.CreateNew(nameof(PhotometricInterpID), TagValues.PHOTOMETRIC, DataTypes.AllUnsignedIntegralTypes, "Photometric/Visual interpretation of the data.", 1);
    public static TagType FillOrder = TagType.CreateNew(nameof(FillOrder), TagValues.FILLORDER, DataTypes.AllUnsignedIntegralTypes, "Defines how the data is stored within a byte: MSB first or LSB first.", 1);
    public static TagType Orientation = TagType.CreateNew(nameof(Orientation), TagValues.ORIENTATION, DataTypes.AllUnsignedIntegralTypes, "Defines how the rows and columns are to be interpreted spatially, e.g. is the 0-row down or up?", 1);
    public static TagType SamplesPerPixel = TagType.CreateNew(nameof(SamplesPerPixel), TagValues.SAMPLESPERPIXEL, DataTypes.AllUnsignedIntegralTypes, "How many channels of data does the raster have? (Elevs = 1, RGB Image = 3, etc.)", 1);
    public static TagType PlanarConfig = TagType.CreateNew(nameof(PlanarConfig), TagValues.PLANARCONFIG, DataTypes.AllUnsignedIntegralTypes, "How is multi-channel data handled? RGBRGBRGB vs RRRGGGBBB", 1);
    public static TagType SampleFormat = TagType.CreateNew(nameof(SampleFormat), TagValues.SAMPLEFORMAT, DataTypes.AllUnsignedIntegralTypes, "How should we interpret each data sample? Int, Float, etc.", 1);
    public static TagType Predictor = TagType.CreateNew(nameof(Predictor), TagValues.PREDICTOR, DataTypes.AllUnsignedIntegralTypes, "Compression predictor hints.", 1);

    // Tiles
    public static TagType TileWidth = TagType.CreateNew(nameof(TileWidth), TagValues.TILEWIDTH, DataTypes.AllIntegralTypes, "Width of each tile, in pixels", 1);
    public static TagType TileLength = TagType.CreateNew(nameof(TileLength), TagValues.TILELENGTH, DataTypes.AllIntegralTypes, "Height of each tile, in pixels", 1);
    public static TagType TileByteOffsets = TagType.CreateNew(nameof(TileByteOffsets), TagValues.TILEOFFSETS, DataTypes.AllIntegralTypes, "Byte offsets to each data tile on disk.", ulong.MaxValue);
    public static TagType TileByteCounts = TagType.CreateNew(nameof(TileByteCounts), TagValues.TILEBYTECOUNTS, DataTypes.AllUnsignedIntegralTypes, "Byte lengths for each (potentially compressed) data tile on disk.", ulong.MaxValue);

    // Geotiff
    public static TagType ModelPixelScale = TagType.CreateNew(nameof(ModelPixelScale), TagValues.GEOTIFF_MODELPIXELSCALETAG, DataTypes.AllFloatingTypes, "Geotiff tag for affine transformations. (ScaleX, ScaleY, ScaleZ)", 3);
    public static TagType ModelTiePoint = TagType.CreateNew(nameof(ModelTiePoint), TagValues.GEOTIFF_MODELTIEPOINTTAG, DataTypes.AllFloatingTypes, "Geotiff tag for raster->model tiepoint pairs", ulong.MaxValue);
    public static TagType ModelTransformation = TagType.CreateNew(nameof(ModelTransformation), TagValues.GEOTIFF_MODELTRANSFORMATIONTAG, DataTypes.AllFloatingTypes, "Geotiff tag (optional) for further defining affine transformations.", 16);
    public static TagType GeoKeyDirectory = TagType.CreateNew(nameof(GeoKeyDirectory), TagValues.GEOTIFF_GEOKEYDIRECTORYTAG, DataTypes.AllUnsignedIntegralTypes, "Geotiff tag that helps store projection information.", ulong.MaxValue);
    public static TagType GeoDoubleParams = TagType.CreateNew(nameof(GeoDoubleParams), TagValues.GEOTIFF_GEODOUBLEPARAMSTAG, DataTypes.Double, "Geotiff tag that helps store all of the DOUBLE valued GeoKeys, referenced by the GeoKeyDirectoryTag.", ulong.MaxValue);
    public static TagType GeoAsciiParams = TagType.CreateNew(nameof(GeoAsciiParams), TagValues.GEOTIFF_GEOASCIIPARAMSTAG, DataTypes.Ascii, "Geotiff tag that helps store projection information.", ulong.MaxValue);
    public static TagType GDALMetadata = TagType.CreateNew(nameof(GDALMetadata), TagValues.GEOTIFF_GDALMETADATATAG, DataTypes.Ascii, "GDAL Tag for stats and other metadata", ulong.MaxValue);
    public static TagType GDALNoData = TagType.CreateNew(nameof(GDALNoData), TagValues.GEOTIFF_GDALNODATATAG, DataTypes.Ascii, "GDAL Tag storing no-data (redundant).", ulong.MaxValue);

    // Arbitrary metadata
    public static TagType ImageDescription = TagType.CreateNew(nameof(ImageDescription), TagValues.IMAGEDESCRIPTION, DataTypes.Ascii, "Text description of the tiff file contents.", ulong.MaxValue);
    public static TagType Artist = TagType.CreateNew(nameof(Artist), TagValues.ARTIST, DataTypes.Ascii, "Text description of the person/company/software that created this file.", ulong.MaxValue);

    // Can we easily do an 'unknown' tag?

    public static void Register(TagType t)
    {
      // Can't match another tag's ID - must be singletons. 
      // Extreme rare case of race condition if (in the future) users can create new TagTypes in parallel.
      if (_tagList.FirstOrDefault(tt => tt.ID == t.ID) == null)
        _tagList.Add(t);
      else
        throw new Exception("Tag has already been registered.");
    }


    #region Tag-Specific functions
    public static string GetSubfileTypeDescription(ushort id)
    {
      // Defined as a 1-hot: http://www.awaresystems.be/imaging/tiff/tifftags/newsubfiletype.html
      // FILETYPE_REDUCEDIMAGE = 1; FILETYPE_PAGE = 2; FILETYPE_MASK = 4;
      // Is this ever *actually* used in combinations?
      if ((id & 1) == 1)
        return "Reduced Image";
      else if ((id & 2) == 2)
        return "Page";
      else if ((id & 4) == 4)
        return "Mask";
      else
        return "Unknown";
    }

    public static string GetFillOrderDescription(ushort id)
    {
      if (id == (ushort)(TagTypeEnums.FillOrders.MSBFirst))
        return "MSB First";
      else if (id == (ushort)(TagTypeEnums.FillOrders.LSBFirst))
        return "LSB First";
      else
        return "Unknown";
    }

    public static string GetPhotometricDescription(ushort id)
    {
      // Appendex A, page 117 Tiff 6.0 spec
      switch (id)
      {
        case (ushort)TagTypeEnums.PhotometricInterpretations.WhiteIsZero: return "WhiteIsZero";
        case (ushort)TagTypeEnums.PhotometricInterpretations.BlackIsZero: return "BlackIsZero";
        case (ushort)TagTypeEnums.PhotometricInterpretations.RGB: return "RGB";
        case (ushort)TagTypeEnums.PhotometricInterpretations.RGBPalette: return "RGB Palette";
        case (ushort)TagTypeEnums.PhotometricInterpretations.TransparencyMask: return "Transparency Mask";
        case (ushort)TagTypeEnums.PhotometricInterpretations.CMYK: return "CMYK";
        case (ushort)TagTypeEnums.PhotometricInterpretations.YCbCr: return "YCbCr";
        case (ushort)TagTypeEnums.PhotometricInterpretations.CIELab: return "CIELab";
        default: return "Unknown";
      }
    }

    // For other IDs, see: http://www.awaresystems.be/imaging/tiff/tifftags/compression.html
    // Site might be down, check google cached version
    public static string GetCompressionDescription(ushort id)
    {
      // Appendex A, page 117 Tiff 6.0 spec
      switch (id)
      {
        case (ushort)TagTypeEnums.CompressionTypes.Uncompressed: return "Uncompressed";
        case (ushort)TagTypeEnums.CompressionTypes.CCITT_1D: return "CCITT 1D";
        case (ushort)TagTypeEnums.CompressionTypes.Group_3_Fax: return "Group 3 Fax";
        case (ushort)TagTypeEnums.CompressionTypes.Group_4_Fax: return "Group 4 Fax";
        case (ushort)TagTypeEnums.CompressionTypes.LZW: return "LZW";
        case (ushort)TagTypeEnums.CompressionTypes.JPEG_Old: return "Old JPEG"; // old style
        case (ushort)TagTypeEnums.CompressionTypes.JPEG_New: return "New JPEG"; // new style
        case (ushort)TagTypeEnums.CompressionTypes.Deflate: return "Deflate"; // Adobe (RAS uses this)
        case (ushort)TagTypeEnums.CompressionTypes.PackBits: return "PackBits";
        default: return "Unknown";
      }
    }
    #endregion

  }

  /// <summary>
  /// <para>This class defines a particular type of tag, e.g. "ImageWidth", and some properties about it.
  /// TagType instances must be singletons - they will be registered to a global list. </para>
  /// Users should only have to create new TagType instances if they do not already exist.
  /// </summary>
  [DebuggerDisplay("{Name}")]
  public class TagType
  {
    public string Name { get; private set; }
    public ushort ID { get; private set; }
    public List<DataType> DataTypesSupported { get; private set; }
    public string Description { get; private set; }
    public ulong MaxAllowableValues { get; private set; }

    // Static functions that chain directly to the constructor. I just want the function to indicate that it's being registered - you can't create identical TagTypes.
    public static TagType CreateNew(string name, TagValues tv, DataType dataType, string desc, ulong maxAllowedVals) => new TagType(name, tv, dataType, desc, maxAllowedVals);
    public static TagType CreateNew(string name, TagValues tv, IList<DataType> dataTypes, string desc, ulong maxAllowedVals) => new TagType(name, tv, dataTypes, desc, maxAllowedVals);
    protected TagType(string name, TagValues tv, IList<DataType> dataTypes, string desc, ulong maxAllowedVals)
    {
      Name = name;
      ID = Convert.ToUInt16(tv);
      DataTypesSupported = dataTypes.ToList();
      Description = desc;
      MaxAllowableValues = maxAllowedVals;
      TagTypes.Register(this);
    }
    protected TagType(string name, TagValues tv, DataType dataType, string desc, ulong maxAllowedVals) : this(name, tv, new DataType[] { dataType }, desc, maxAllowedVals)
    { }

    // Converts this tag-type (singleton) to the 
    public Tag CreateTag(DataType actualDT, object[] values) => new Tag(this, actualDT, values);

  }


  /// <summary>
  /// <para>This is an instance of a <see cref="TiffBinaryReader.TagType"/>. It was read-from, or will be written-to, a particular file.
  /// Where a <see cref="TiffBinaryReader.TagType"/> can have a list of possible <see cref="DataType"/>, 
  /// a <see cref="Tag"/> can only have a single 'actual' <see cref="DataType"/>. </para> 
  /// Values are boxed, because who wants to fight a type system for no real performance gain.
  /// </summary>
  [DebuggerDisplay("{TagType.Name}")]
  public class Tag
  {
    public TagType TagType;
    public DataType ActualDataType;

    // Is there a way to do this with generics?
    public object[] Values;

    public Tag(TagType type, DataType actualDT, object[] values)
    {
      TagType = type; ActualDataType = actualDT; Values = values;
    }

    public T ValueAs<T>(int index = 0) where T : struct
    {
      // Many tags contain only one value - return first as T.
      object val = Values[index];
      return ValueConverter.To<T>(val);    
    }
    public string ValueAsString()
    {
      return Values[0].ToString();
    }

    public T[] ValuesAs<T>() where T : struct
    {
      T[] retn = new T[Values.Length];
      for (int i = 0; i < retn.Length; i++)
        retn[i] = ValueAs<T>(i);
      return retn;
    }

  }

  public static class ValueConverter
  {
    static Type SByteType = typeof(sbyte); // Naming is reversed here.
    static Type ByteType = typeof(byte);
    static Type ShortType = typeof(short);
    static Type UShortType = typeof(ushort);
    static Type IntType = typeof(int);
    static Type UIntType = typeof(uint);
    static Type LongType = typeof(long);
    static Type ULongType = typeof(ulong);
    static List<Type> IntegerTypes = new List<Type>()
    { SByteType, ByteType, ShortType, UShortType, IntType, UIntType, LongType, ULongType };

    // NOTE --- subtle error - fails if we're using a ulong > long.maxvalue!!!!!!
    static Dictionary<Type, BigInteger> IntegerMaxValues = new Dictionary<Type, BigInteger>()
    {
      { SByteType, new BigInteger(sbyte.MaxValue) },
      { ByteType, new BigInteger(byte.MaxValue) },
      { ShortType, new BigInteger(short.MaxValue) },
      { UShortType, new BigInteger(ushort.MaxValue) },
      { IntType, new BigInteger(int.MaxValue) },
      { UIntType, new BigInteger(uint.MaxValue) },
      { LongType, new BigInteger(long.MaxValue) },
      { ULongType, new BigInteger(ulong.MaxValue) },
    };
    static Dictionary<Type, BigInteger> IntegerMinValues = new Dictionary<Type, BigInteger>()
    {
      { SByteType, new BigInteger(sbyte.MinValue) },
      { ByteType, new BigInteger(byte.MinValue) },
      { ShortType, new BigInteger(short.MinValue) },
      { UShortType, new BigInteger(ushort.MinValue) },
      { IntType, new BigInteger(int.MinValue) },
      { UIntType, new BigInteger(uint.MinValue) },
      { LongType, new BigInteger(long.MinValue) },
      { ULongType, new BigInteger(ulong.MinValue)},
    };


    static Type SingleType = typeof(float);
    static Type DoubleType = typeof(double);
    static List<Type> FloatingTypes = new List<Type>() { SingleType, DoubleType };
    

    static List<Type> NumericTypes;
    static ValueConverter()
    {
      NumericTypes = new List<Type>();
      NumericTypes.AddRange(IntegerTypes);
      NumericTypes.AddRange(FloatingTypes);
    }

    private static BigInteger AsBigInt(object v)
    {
      Type vType = v.GetType();
      if (vType == SByteType)
        return new BigInteger((sbyte)v);
      else if (vType == ByteType)
        return new BigInteger((byte)v);
      else if (vType == ShortType)
        return new BigInteger((short)v);
      else if (vType == UShortType)
        return new BigInteger((ushort)v);
      else if (vType == IntType)
        return new BigInteger((int)v);
      else if (vType == UIntType)
        return new BigInteger((uint)v);
      else if (vType == LongType)
        return new BigInteger((long)v);
      else if (vType == ULongType)
        return new BigInteger((ulong)v);
      else
        throw new InvalidCastException();
    }
    private static T AsT<T>(BigInteger val)
    {
      Type tType = typeof(T);
      if (tType == SByteType)
        return (T)(object)(sbyte)val;
      else if (tType == ByteType)
        return (T)(object)(byte)val;
      else if (tType == ShortType)
        return (T)(object)(short)val;
      else if (tType == UShortType)
        return (T)(object)(ushort)val;
      else if (tType == IntType)
        return (T)(object)(int)val;
      else if (tType == UIntType)
        return (T)(object)(uint)val;
      else if (tType == LongType)
        return (T)(object)(long)val;
      else if (tType == ULongType)
        return (T)(object)(ulong)val;
      else
        throw new InvalidCastException();
    }
    public static T To<T>(object v) where T : struct
    {
      Type vType = v.GetType();
      Type tType = typeof(T);

      // Ezpz - check this first.
      if (vType == tType)
        return (T)v;

      if (NumericTypes.Contains(tType) == false)
        throw new InvalidCastException();
      if (NumericTypes.Contains(vType) == false)
        throw new InvalidCastException();

      if (IntegerTypes.Contains(tType) && IntegerTypes.Contains(vType))
      {
        // Only failure if value v is > tType.Max or < tType.Min
        BigInteger maxAllowed = IntegerMaxValues[tType];
        BigInteger minAllowed = IntegerMinValues[tType];
        BigInteger input = AsBigInt(v);

        if (input > maxAllowed || input < minAllowed)
          throw new InvalidCastException();

        return AsT<T>(input);
      }
      else if (FloatingTypes.Contains(tType) && FloatingTypes.Contains(vType))
      {
        // Only failure if value v is > tType.Max or < tType.Min

        // Because it missed the first check, we know that one of the types is double and the other is float.
        if (tType == SingleType && vType == DoubleType)
        {
          // Casting double -> single, potential casting error.
          double value = (double)v;
          if (value > float.MaxValue || value < float.MinValue)
            throw new InvalidCastException();
          else
          {
            float newValue = (float)value;
            return (T)(object)(newValue); // Ugh. Intermediate cast to object necessary?
          }
        }
        else if (tType == DoubleType && vType == SingleType)
        {
          // Casting single->double, should be golden.
          return (T)(object)(double)(float)v; // hahahahahahahahaha.... I'm sad.
        }
        else
          throw new InvalidCastException(); // No other supported IEEEFP yet.
      }
      else
      {
        // Not trying float <-> integer atm.
        throw new InvalidCastException();
      }
    }
  }


  // Should we do *some* custom classes like this to help with enums?
  public class PhotometricTagType : TagType
  {
    public PhotometricTagType(string name, TagValues tv, IList<DataType> dataTypes, string desc, ulong maxAllowedVals) : 
      base(name, tv, dataTypes, desc, maxAllowedVals)
    { }
    public PhotometricTagType(string name, TagValues tv, DataType dataType, string desc, ulong maxAllowedVals) : 
      base(name, tv, new DataType[] { dataType }, desc, maxAllowedVals)
    { }

  }

}
