using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TiffBinaryReader
{
  // Page 15
  // Types: 1 = UByte  - 1 byte
  //        2 = Ascii  - 1 byte per char
  //        3 = UShort - 2 byte uint
  //        4 = ULong  - 4 byte uint
  //        5 = Rational - (unused?)
  //        6 = SByte  - 1 byte, signed
  //        7 = Undef  - 1 byte undef
  //        8 = SShort - 2 byte int
  //        9 = SLong  - 4 byte int
  //       10 = SRational - (unused?)
  //       11 = Float  - 4 byte single
  //       12 = Double - 8 byte double
  public class DataTypes
  {
    public static DataType UByte = new DataType(1, nameof(UByte), typeof(byte), sizeof(byte));
    public static DataType Ascii = new DataType(2, nameof(Ascii), typeof(byte), sizeof(byte));
    public static DataType UShort = new DataType(3, nameof(UShort), typeof(ushort), sizeof(ushort));
    public static DataType UInt = new DataType(4, nameof(UInt), typeof(uint), sizeof(uint));
    public static DataType URational = new DataType(5, nameof(URational), null, 4);
    public static DataType Byte = new DataType(6, nameof(Byte), typeof(sbyte), sizeof(sbyte));
    public static DataType Undef = new DataType(7, nameof(Undef), null, 4);
    public static DataType Short = new DataType(8, nameof(Short), typeof(short), sizeof(short));
    public static DataType Int = new DataType(9, nameof(Int), typeof(int), sizeof(int)); // Defined as 'long' in the tiff spec, dont get confused.
    public static DataType Rational = new DataType(10, nameof(Rational), null, 4);
    public static DataType Float = new DataType(11, nameof(Float), typeof(float), sizeof(float));
    public static DataType Double = new DataType(12, nameof(Double), typeof(double), sizeof(double));
    public static DataType IFD = new DataType(13, nameof(IFD), typeof(uint), sizeof(uint));

    // Bigtiff specific
    public static DataType ULong8 = new DataType(16, nameof(ULong8), typeof(ulong), sizeof(ulong));
    public static DataType Long8 = new DataType(17, nameof(Long8), typeof(long), sizeof(long));
    public static DataType IFD8 = new DataType(18, nameof(IFD8), typeof(ulong), sizeof(ulong));


    // Searchable collection
    public static DataType[] AvailableTypes = { UByte, Ascii, UShort, UInt, URational, Byte, Undef, Short, Int, Rational, Float, Double, IFD, ULong8, Long8, IFD8 };

    public static DataType[] AllIntegralTypes = new DataType[] { DataTypes.Byte, DataTypes.UByte, DataTypes.Short, DataTypes.UShort,
      DataTypes.Int, DataTypes.UInt, DataTypes.Long8, DataTypes.ULong8 };
    public static DataType[] AllUnsignedIntegralTypes = new DataType[] { DataTypes.UByte, DataTypes.UShort, DataTypes.UInt, DataTypes.ULong8 };
    public static DataType[] AllFloatingTypes = new DataType[] { DataTypes.Float, DataTypes.Double }; // DataTypes.URational, DataTypes.Rational, 
    public static DataType[] AllAsciiTypes = new DataType[] { DataTypes.Ascii };

    public static DataType Get(ushort typeID)
    { return AvailableTypes.FirstOrDefault((dt) => dt.UID == typeID); }
  }

  [DebuggerDisplay("{Description}")]
  public class DataType
  {
    public ushort UID { get; private set; }
    public string Description { get; private set; }
    public int ByteCount { get; private set; }
    public Type Type { get; private set; }

    public DataType(ushort uid, string desc,Type type, int byteCount )
    { UID = uid; Description = desc; ByteCount = byteCount; Type = type; }
    
    // Might be null, if typeID not found.
    private object BitConvert(byte[] tagData, int offset)
    {
      // There's gotta be a better way...
      if (Type == typeof(byte))
      {
        return ((byte)(BitConverter.ToChar(tagData, offset)));
      }
      else if (Type == typeof(ushort))
      {
        return BitConverter.ToUInt16(tagData, offset);
      }
      else if (Type == typeof(uint))
      {
        return BitConverter.ToUInt32(tagData, offset);
      }
      else if (Type == typeof(sbyte))
      {
        return ((sbyte)(BitConverter.ToChar(tagData, offset)));
      }
      else if(Type == typeof(short))
      {
        return BitConverter.ToInt16(tagData, offset);
      }
      else if(Type == typeof(int))
      {
        return BitConverter.ToInt32(tagData, offset);
      }
      else if (Type == typeof(float))
      {
        return BitConverter.ToSingle(tagData, offset);
      }
      else if (Type == typeof(double))
      {
        return BitConverter.ToDouble(tagData, offset);
      }
      else if (Type == typeof(long))
      {
        return BitConverter.ToInt64(tagData, offset);
      }
      else if (Type == typeof(ulong))
      {
        return BitConverter.ToUInt64(tagData, offset);
      }
      else
        return "";
    }
    private byte[] GetBytesInternal(object[] values)
    {
      var retn = new List<byte>();
      
      // There's gotta be a better way...
      if (Type == typeof(byte))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((byte)values[i]));
      }
      else if (Type == typeof(ushort))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((ushort)values[i]));
      }
      else if (Type == typeof(uint))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((uint)values[i]));
      }
      else if (Type == typeof(sbyte))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((sbyte)values[i]));
      }
      else if (Type == typeof(short))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((short)values[i]));
      }
      else if (Type == typeof(int))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((int)values[i]));
      }
      else if (Type == typeof(float))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((float)values[i]));
      }
      else if (Type == typeof(double))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((double)values[i]));
      }
      else if (Type == typeof(long))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((long)values[i]));
      }
      else if (Type == typeof(ulong))
      {
        for (int i = 0; i < values.Length; i++)
          retn.AddRange(BitConverter.GetBytes((ulong)values[i]));
      }

      // Hip-check
      if (retn.Count / ByteCount != values.Length)
        throw new Exception("Data type issue in GetBytes");

      return retn.ToArray();
    }
    public string StringConvert(byte[] tagData, int offset, uint count)
    {
      if (this == DataTypes.Ascii)
      {
        // subtract one for the null-term character??
        return System.Text.ASCIIEncoding.ASCII.GetString(tagData, offset, (int)count - 1);
      }
      else
      {
        var sb = new StringBuilder();
        for (int i = 0; i < count; i ++)
        {
          int subOffset = i * ByteCount;

          sb.Append(BitConvert(tagData, offset + subOffset).ToString());
          if (i != count - 1)
            sb.Append(", ");
        }
        return sb.ToString();
      }
    }

    public object[] Parse(byte[] tagData, int offset, uint count)
    {
      if (this == DataTypes.Ascii)
      {
        var retn = new object[1];
        // subtract one for the null-term character??
        retn[0] = ASCIIEncoding.ASCII.GetString(tagData, offset, (int)count - 1);
        return retn;
      }
      else
      {
        var retn = new object[count];
        for (int i = 0; i < count; i++)
        {
          int subOffset = i * ByteCount;
          retn[i] = BitConvert(tagData, offset + subOffset);
        }
        return retn;
      }
    }

    public byte[] GetBytes(object[] values)
    {
      if (this == DataTypes.Ascii)
      {
        byte[] str = ASCIIEncoding.ASCII.GetBytes((string)(values.First()));
        
        byte[] retn = new byte[str.Length + 1]; // Add the null-terminator char?
        Buffer.BlockCopy(str, 0, retn, 0, str.Length);
        return retn;
      }
      else
      {
        return GetBytesInternal(values);
      }
    }
  }
}
