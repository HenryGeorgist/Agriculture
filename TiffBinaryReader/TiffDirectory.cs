using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using TiffBinaryReader.TagTypeEnums;

namespace TiffBinaryReader
{
  [DebuggerDisplay("Page {DirectoryIndex}")]
  public class TiffDirectory : IDisposable
  {
    #region Public Properties

    //
    public TiffFile Tiff { get; private set; }

    // Directory/Tag header metadata.
    public long DirectoryIndex { get; private set; }    // Page 0/1/2 ...
    public TagMetadata Metadata { get; private set; }   // Contains processed tag metadata

    // The matriarch. We can only read an extremely specific subset of the tiff spec.
    public bool CanReadData { get { return Metadata.CanReadData; } }

    // If CanReadData = false, Why? Missing x/y/z, etc.
    public string ErrorStatus { get { return Metadata.ErrorStatus; } }
   

    #endregion

    #region Private Local Variables

    // Tile-reading threadlocal helpers
    private ThreadLocal<byte[]> TLCompressedTileByteBuffers;
    private ThreadLocal<byte[]> TLUncompressedTileByteBuffers;
    
    private byte[] _noDataCompressedTileBuffer;
    private byte[] _noDataTileBuffer;

    private bool IsBigTiff { get { return Tiff.IsBigTiff; } } // Just a simple accessor
    #endregion


    // Private constructor, chained from the create-all-directories static call.
    private TiffDirectory(TiffFile parent, BinaryReader br, long dirIndex, long dirStartPosition)
    {
      Tiff = parent;
      DirectoryIndex = dirIndex;

      // Process metadata
      Metadata = TagMetadata.Read(br, dirStartPosition, IsBigTiff);

      if(Metadata.IsTiled && Metadata.CanReadData)
      {
        // Set up the tile-reading buffers
        int maxCompressedTileSize = (int)Metadata.TileByteCounts.Max();
        TLCompressedTileByteBuffers = new ThreadLocal<byte[]>(() => new byte[maxCompressedTileSize]);
        TLUncompressedTileByteBuffers = new ThreadLocal<byte[]>(() => new byte[Metadata.NumberCellsPerTile * (Metadata.BitsPerSample / 8) * Metadata.SamplesPerPixel]);

        // Set up the nodata buffer. (Returns null on failure)
        TryGetCompressedNodataTile(ref _noDataCompressedTileBuffer, ref _noDataTileBuffer);        
      }
    }

    
    internal static List<TiffDirectory> Parse(TiffFile parent, BinaryReader br)
    {
      var retn = new List<TiffDirectory>();

      // Reset the stream position to 0
      br.BaseStream.Position = 0;

      long nextDirOffset;
      if(parent.IsBigTiff)
      {
        // 16 byte header for big-tiff
        byte[] startupBytes = br.ReadBytes(16);

        // How big are the offsets? (Always 8)
        short bytesize = BitConverter.ToInt16(startupBytes, 4);
        if (bytesize != 8)
          throw new Exception("Can't read bigtiff with non 8-byte offsets."); // Totally non-standard

        short constant0 = BitConverter.ToInt16(startupBytes, 6);
        if (constant0 != 0)
          throw new Exception("Magic number '0' not present as 6th byte in file, doesn't follow the BigTIFF specifications.");

        // Offset into the file where tags begin
        nextDirOffset = (long)BitConverter.ToUInt64(startupBytes, 8);
      }
      else
      {
        // 8 byte header for reg tiff
        byte[] startupBytes = br.ReadBytes(8);

        // Offset into the file where tags begin
        nextDirOffset = (long)BitConverter.ToUInt32(startupBytes, 4);
      }

      long dirCount = 0;
      while (nextDirOffset != 0)
      {
        retn.Add(new TiffDirectory(parent, br, dirCount++, nextDirOffset));
        nextDirOffset = retn.Last().Metadata.NextDirectoryOffset;
      }

      return retn;
    }
    
    public float TileNoDataCompressionPotential()
    {
      const float FAIL_RETN = 1f;
      string nodataStr = Metadata.NoData;
      // No compression potential
      if (nodataStr == "")
        return FAIL_RETN;

      float nd;
      bool success = float.TryParse(nodataStr, out nd);
      if (!success)
        return FAIL_RETN;

      long minByteCt = Metadata.TileByteCounts.Min();

      // How do we continue trying?
      int tryIdx = Metadata.TileByteCounts.ToList().IndexOf(minByteCt);
      float[] tile = GetTile(tryIdx);

      bool allND = true;
      for(int i = 0; i < tile.Length; i++)
      {
        if(tile[i] != nd)
        {
          allND = false;
          break;
        }
      }

      if (allND == false)
        return FAIL_RETN;

      byte[] ndCompressedTile = GetCompressedTile(tryIdx);

      int duplicateTiles = 0;
      for(int i = 0; i < Metadata.TileCount; i++)
      {
        if (i == tryIdx)
          continue;

        byte[] compressedTile = GetCompressedTile(i);
        if (compressedTile.Length != ndCompressedTile.Length)
          continue;

        bool match = true;
        for (int j = 0; j < compressedTile.Length; j++)
        {
          if (compressedTile[j] != ndCompressedTile[j])
          {
            match = false;
            break;
          }
        }

        if (match)
          duplicateTiles++;
      }

      // How many duplicate tiles?
      float savings = (float)duplicateTiles / (float)Metadata.TileCount;
      return 1 - savings;
    }

    #region Reading Tiles
    public float[] GetTile(int tileIdx)
    {
      float[] retn = new float[Metadata.NumberCellsPerTile];
      GetTile(tileIdx, ref retn);
      return retn;
    }

    public static bool TryNoDataOpt = false;
    public int TileReadsSaved = 0;
    public void GetTile(int tileIdx, ref float[] buf)
    {
      if (CanReadData == false)
      {
        buf = null;
        return;
      }

      if (tileIdx >= Metadata.TileCount)
        throw new ArgumentOutOfRangeException(nameof(tileIdx));

      long tileStartIdx = Metadata.TileByteOffsets[tileIdx];  // Compressed tile start index (in file)
      int tileByteCt = (int)Metadata.TileByteCounts[tileIdx]; // Compressed tile byte count

      FileStream fs = Tiff.GetReadonlyFilestream();
      fs.Position = tileStartIdx;

      // Read the compressed bytes from disk. Can be pre-allocated to Max(_tileByteCounts) size.
      byte[] arr = TLCompressedTileByteBuffers.Value; // This will be longer than necessary!!! It's preallocated to max-size.
      fs.Read(arr, 0, tileByteCt);

      // Try to compare to our nodata tile - short-circuit test.
      if(TryNoDataOpt && _noDataCompressedTileBuffer != null && _noDataTileBuffer != null)
      {

        if(tileByteCt == _noDataCompressedTileBuffer.Length)
        {
          // 99% likely to be a nodata tile - validate compressed bytes.
          bool isSame = true;

          // Check through the compressed ~1301 bytes for differences.
          for (int i = 0; i < tileByteCt; i++)
          {
            if (arr[i] != _noDataCompressedTileBuffer[i])
            {
              isSame = false;
              break;
            }
          }

          if (isSame)
          {
            Interlocked.Increment(ref TileReadsSaved);

            // Just block-copy our uncompressed tile buf to a float array and return
            int nCells = Metadata.NumberCellsPerTile;
            if (buf == null || buf.Length != nCells)
              buf = new float[nCells];
            Buffer.BlockCopy(_noDataTileBuffer, 0, buf, 0, _noDataTileBuffer.Length);
            return;
          }
        }
      }


      /* I think this is sufficient? http://stackoverflow.com/questions/9050260/what-does-a-zlib-header-look-like
        Skips the zlib-deflate header. Are there other common deflate implementations?
        .Net one - 0x58 0x85 header? Do we have to remove that?
        Note - https://blogs.msdn.microsoft.com/bclteam/2007/05/16/system-io-compression-capabilities-kim-hamilton/
        Might actually be a 6-byte header? But this deflate implementation doesnt support dictionaries anyway.
        Check for that bit in the future ....
      */
      int skipFirstBytes = 0;
      if (arr[0] == 0x78) // Default zlib header implementation... Can this change? How do we ID a generic zlib header?
        skipFirstBytes = 2;
      else if (arr[0] == 0x58 && arr[1] == 0x85) // See our WriteTile function - these are *our* 'zlib header' bytes.
        skipFirstBytes = 2;

      // Important: First two bytes are part of zlib-deflate, *not* the generic deflate spec.
      // http://george.chiramattel.com/blog/2007/09/deflatestream-block-length-does-not-match.html
      using (MemoryStream readFrom = new MemoryStream(arr, skipFirstBytes, tileByteCt - skipFirstBytes)) // This actually uses the arr[] buffer, doesnt copy.
      {
        DecompressTile(readFrom, ref buf);
      }
    }

    public byte[] GetCompressedTile(int tileIdx)
    {
      byte[] retn = null;
      GetCompressedTile(tileIdx, ref retn);
      return retn;
    }
    public void GetCompressedTile(int tileIdx, ref byte[] buf)
    {
      if (CanReadData == false)
      {
        buf = null;
        return;
      }

      if (tileIdx >= Metadata.TileCount)
        throw new ArgumentOutOfRangeException(nameof(tileIdx));

      long tileStartIdx = Metadata.TileByteOffsets[tileIdx];  // Compressed tile start index (in file)
      int tileByteCt = (int)Metadata.TileByteCounts[tileIdx]; // Compressed tile byte count

      FileStream fs = Tiff.GetReadonlyFilestream();
      fs.Position = tileStartIdx;

      if (buf == null || buf.Length != tileByteCt)
        buf = new byte[tileByteCt];

      fs.Read(buf, 0, tileByteCt);
    }


    private void DecompressTile(Stream compStream, ref float[] buf)
    {
      // Is this safe for partial (right, bottom edge) tiles?
      byte[] uncompBuf = TLUncompressedTileByteBuffers.Value;
      int nCells = Metadata.NumberCellsPerTile;
      if (buf == null || buf.Length != nCells)
        buf = new float[nCells];

      using (DeflateStream decompStream = new DeflateStream(compStream, CompressionMode.Decompress))
      {
        decompStream.Read(uncompBuf, 0, uncompBuf.Length);

        // Try to go unsafe here, and avoid an extra copy?
        Buffer.BlockCopy(uncompBuf, 0, buf, 0, uncompBuf.Length);
      }
    }

    internal float[] DecompressTile(Stream compStream)
    {
      // For internal, testing use.
      using (MemoryStream ms = new MemoryStream())
      using (DeflateStream decompStream = new DeflateStream(compStream, CompressionMode.Decompress))
      {
        decompStream.CopyTo(ms);
        decompStream.Close();
        byte[] buf = ms.GetBuffer();
        float[] arr = new float[buf.Length / 4];
        Buffer.BlockCopy(buf, 0, arr, 0, buf.Length);
        return arr;
      }

    }
    private bool TryGetCompressedNodataTile(ref byte[] compressedBuf, ref byte[] uncompressedBuf)
    {
      // Get the directory's nodata value
      string nodataStr = Metadata.NoData;
      if (string.IsNullOrEmpty(nodataStr))
        return false;

      float nd;
      bool success = float.TryParse(nodataStr, out nd);
      if (!success)
        return false;

      // *If* a pure NoData tile exists, it's extremely likely to be the smallest
      // one, since identical values get amazing compression.
      long minByteCt = Metadata.TileByteCounts.Min();

      /* Just try the first instance of MinByteCount for now? 
       * We don't want to get in a situation where someone gives us a giant tiff
       * with a bunch of 0'd out tiles, and we read through *every* one of them
       * to try to find a ND tile. This seems like the best compromise.
       */
      int tryIdx = Metadata.TileByteCounts.ToList().IndexOf(minByteCt);
      float[] tile = GetTile(tryIdx);

      bool allND = true;
      for (int i = 0; i < tile.Length; i++)
      {
        if (tile[i] != nd)
        {
          allND = false;
          break;
        }
      }

      if (allND == false)
        return false;

      compressedBuf = GetCompressedTile(tryIdx);
      uncompressedBuf = new byte[tile.Length * sizeof(float)];
      Buffer.BlockCopy(tile, 0, uncompressedBuf, 0, uncompressedBuf.Length);
      return true;
    }

    public void GetValidRegion(int tileNumber, out int validWidth, out int validHeight)
    {
      // Find the in-bounds dims of this tile.
      int tileWidth = Metadata.TileWidth;
      int tileHeight = Metadata.TileHeight;
      int tilesWide = Metadata.TilesWide;
      int tilesTall = Metadata.TilesTall;
      int width = (int)Metadata.ImageWidth;
      int height = (int)Metadata.ImageHeight;

      int tileXIdx = tileNumber % tilesWide;
      int tileYIdx = tileNumber / tilesWide;

      int fullW = (tileXIdx + 1) * tileWidth;
      int extraW = fullW - width;
      int fullH = (tileYIdx + 1) * tileHeight;
      int extraH = fullH - height;

      if (extraW > 0)
        validWidth = tileWidth - extraW;
      else
        validWidth = tileWidth;

      if (extraH > 0)
        validHeight = tileHeight - extraH;
      else
        validHeight = tileHeight;
    }
    #endregion

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // Drop internal buffers
          TLCompressedTileByteBuffers = null;
          TLUncompressedTileByteBuffers = null;
          Metadata = null;
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~TiffDirectory() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}