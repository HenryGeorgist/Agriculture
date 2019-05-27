namespace TiffBinaryReader.TagTypeEnums
{
  public enum FillOrders : ushort
  { // http://www.awaresystems.be/imaging/tiff/tifftags/fillorder.html
    MSBFirst = 1, // Only baseline requirement.
    LSBFirst = 2
  }

  public enum PhotometricInterpretations : ushort
  {
    WhiteIsZero = 0,
    BlackIsZero = 1,
    RGB = 2,
    RGBPalette = 3,
    TransparencyMask = 4,
    CMYK = 5,
    YCbCr = 6,
    CIELab = 8
  }

  public enum Orientations : ushort
  { // http://www.awaresystems.be/imaging/tiff/tifftags/orientation.html
    TopLeft = 1,  // Only baseline requirement. Rows[0] = Top of image.
    TopRight = 2,
    BotRight = 3,
    BotLeft = 4,
    LeftTop = 5,
    RightTop = 6,
    RightBot = 7,
    LeftBot = 8
  }

  public enum PlanarConfigs : ushort
  { // http://www.awaresystems.be/imaging/tiff/tifftags/planarconfiguration.html
    // Unnecessary if SamplesPerPixel = 1.
    Contiguous = 1, // RGB RGB RGB ... Only baseline requirement
    Separate = 2    // RRRRRR ... GGGGG ... BBBBB (uncommon)
  }

  public enum SampleFormats : ushort
  { // http://www.awaresystems.be/imaging/tiff/tifftags/sampleformat.html
    UInt = 1,
    Int = 2,
    IEEEFP = 3,
    Void = 4,
    ComplexInt = 5,
    ComplexIEEEFP = 6
  }

  public enum Predictors : ushort
  { // http://www.awaresystems.be/imaging/tiff/tifftags/predictor.html
    None = 1,
    Horizontal = 2,
    FloatingPoint = 3
  }

  public enum CompressionTypes : ushort
  { // Appendex A, page 117 Tiff 6.0 spec
    Uncompressed = 1,
    CCITT_1D = 2,
    Group_3_Fax = 3,
    Group_4_Fax = 4,
    LZW = 5,
    JPEG_Old = 6,
    JPEG_New = 7,
    Deflate = 8,
    PackBits = 32773
  }

  [System.Flags]
  public enum SubfileTypes : ushort // Techincally these are 1-hot, but that's a little ambiguous...
  { // http://www.awaresystems.be/imaging/tiff/tifftags/newsubfiletype.html
    None = 0,
    ReducedImage = 1,
    Page = 2,
    Mask = 4
  }
}
