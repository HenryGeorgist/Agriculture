using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TiffBinaryReader
{
  public class TiffFileWriter : IDisposable
  {
    private List<TagMetadata> _dirDefs;
    // Bitarray isnt threadsafe. Implement and interlocked-or-threadsafe one? (currently just locking)
    private List<BitArray> _tilesWritten;
    private object _tilesWrittenLock = new object();

    private List<long[]> _tileByteLocations;
    private List<long[]> _tileByteCounts;

    private FileStream _fs;
    private string _filename;

    // Only 256x256 for now, or we need a diff buffer per directory potentially.
    private ThreadLocal<byte[]> TLUncompressedBuffer;
    private ConcurrentDictionary<Tuple<int, int>, byte[]> _queuedData;
    

    /// <summary>
    /// How many tiles should we accumulate in memory before flushing to disk?
    /// </summary>    
    public int MaxTileQueueDepth { get; set; } = 50;

    /// <summary>
    /// If set to true, directories and tiles will be written expressly in-order. 
    /// This can violate the MaxTileQueueDepth - if our most recently written tile is #16,
    /// then we will queue compressed tiles in memory until we find #17.
    /// </summary>
    private bool _enforceTilesWriteInOrder = false;
    private int _waitingForDir = -1;
    private int _waitingForTile = -1;
    private object _writeLock = new object();

    public TiffFileWriter(string filename, TagMetadata directoryToCreate, bool enforceTilesWriteInOrder = false) : 
      this(filename, new List<TagMetadata>() { directoryToCreate }, enforceTilesWriteInOrder) { }
    public TiffFileWriter(string filename, List<TagMetadata> directoriesToCreate, bool enforceTilesWriteInOrder = false)
    {
      // Don't support appending files yet.
      if (File.Exists(filename))
        throw new ArgumentException("File " + Path.GetFileName(filename) + " already exists, cannot overwrite.");

      if (directoriesToCreate == null || directoriesToCreate.Count == 0)
        throw new ArgumentException("No directories to create.", nameof(directoriesToCreate));

      _dirDefs = directoriesToCreate.ToList();
      _tileByteLocations = new List<long[]>(_dirDefs.Count);
      _tileByteCounts = new List<long[]>(_dirDefs.Count);
      _enforceTilesWriteInOrder = enforceTilesWriteInOrder;
      if(_enforceTilesWriteInOrder)
      {
        _waitingForDir = 0;
        _waitingForTile = 0;
      }
      else
      {
        _waitingForDir = -1;
        _waitingForTile = -1;
      }


      TLUncompressedBuffer = new ThreadLocal<byte[]>(() => new byte[256 * 256 * sizeof(float)]);
      _queuedData = new ConcurrentDictionary<Tuple<int, int>, byte[]>();

      // All tiles must be written!!!
      _tilesWritten = new List<BitArray>();
      for(int i = 0; i < _dirDefs.Count; i++)
      {
        var dir = _dirDefs[i];
        int nTiles = (int)dir.TileCount; // 2bn tile limit
        _tilesWritten.Add(new BitArray(nTiles));

        var byteLocs = new long[nTiles];
        _tileByteLocations.Add(byteLocs);

        var byteCounts = new long[nTiles];
        _tileByteCounts.Add(byteCounts);

        if (dir.NumberCellsPerTile != 256 * 256)
          throw new ArgumentException("Directory " + i.ToString() + " does not have 256x256 tiles. Current implementation only allows for tiles of this size.");
        if (dir.BitsPerSample != 32)
          throw new ArgumentException("Directory " + i.ToString() + " needs 32-bit data for now.");
      }

      _filename = filename;
      _fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write, FileShare.None);
      
      
      // Write the tiff header. (See TiffFile constructor for more info)
      byte[] startup = new byte[16];

      // Little-Endian
      startup[0] = 73;
      startup[1] = 73;

      // Big-tiff file, not supporting writing reg-tiffs yet.
      byte[] bigID = BitConverter.GetBytes(TiffFile.BIGTIFF_ID);
      startup[2] = bigID[0];
      startup[3] = bigID[1];

      // 8-byte offsets for big-tiff. Really no reason to ever change this.
      byte[] offsetSize = BitConverter.GetBytes((ushort)8);
      startup[4] = offsetSize[0];
      startup[5] = offsetSize[1];

      // Constant - 0
      startup[6] = 0;
      startup[7] = 0;

      // Next 8-bytes are the first-directory-offset. Unknown yet, write on disposal.
      _fs.Write(startup, 0, startup.Length);
    }


    /* With a bit of work, this can be done in parallel.
       It would help with the CPU intensive compression. Basically, threadsafe filestreams that
       have FileShare.Write permissions. Pre-allocate enough space for the file, then write to
       those byte locations. Global lock that allocates another XXX bytes to the file as-needed.
       1300 bytes per tile minimum? Min(10kB, 1300 * tilesRemaining)? Accelerated allocation scheme?

    */
    public void WriteTileSerial(int dirIdx, int tileIdx, float[] data)
    {
      if (dirIdx < 0 || dirIdx >= _dirDefs.Count)
        throw new ArgumentOutOfRangeException(nameof(dirIdx));

      var metadata = _dirDefs[dirIdx];

      if (tileIdx < 0 || tileIdx >= metadata.TileCount)
        throw new ArgumentOutOfRangeException(nameof(tileIdx));

      if (_tilesWritten[dirIdx][tileIdx] == true)
        throw new Exception("This tile has already been written.");

      if (data == null || data.Length != metadata.NumberCellsPerTile)
        throw new ArgumentException("Data should be " + metadata.NumberCellsPerTile.ToString() + " elements.");

      byte[] uncompData = new byte[data.Length * sizeof(float)];
      Buffer.BlockCopy(data, 0, uncompData, 0, uncompData.Length);

      byte[] compData = null;
      int compDataValidLen = 0;
      using (MemoryStream decompMS = new MemoryStream(uncompData))
      {
        CompressTile(decompMS, ref compData, ref compDataValidLen);
      }
      _tileByteLocations[dirIdx][tileIdx] = _fs.Length;

      /* ZLIB References
         http://stackoverflow.com/questions/9050260/what-does-a-zlib-header-look-like
         https://blogs.msdn.microsoft.com/bclteam/2007/05/16/system-io-compression-capabilities-kim-hamilton/
         "With our deflate implementation, those bytes are 0x58 and 0x85."
      */
      _tileByteCounts[dirIdx][tileIdx] = compDataValidLen + 2 + 4; // zlib header (2 bytes) + adler-32 checksum (4 bytes)
        
      // Within a byte, the 4/4 bit-split is inverted. (Hence, 0x58 is first, since the 'first 4 bits' are 1000 = 8.)
      _fs.WriteByte((byte)0x58); 
      _fs.WriteByte((byte)0x85); //

      // Write the deflated tile and checksum
      _fs.Write(compData, 0, compDataValidLen);

      // adler-32 checksum    
      byte[] adler32 = Adler32.ComputeAdler32(uncompData, 0, uncompData.Length);
    
      // Tasking these was categorically worse - not enough work to justify this granularity.
      //Task a = Task.Run(() => _fs.Write(compData, 0, compDataValidLen));
      //Task b = Task.Run(() => adler32 = Adler32.ComputeAdler32(uncompData, 0, uncompData.Length));
      
      _fs.WriteByte(adler32[0]);
      _fs.WriteByte(adler32[1]);
      _fs.WriteByte(adler32[2]);
      _fs.WriteByte(adler32[3]);
      
      // Mark as written
      _tilesWritten[dirIdx][tileIdx] = true;
    }
    
    public void WriteTileParallel(int dirIdx, int tileIdx, float[] data)
    {
      if (dirIdx < 0 || dirIdx >= _dirDefs.Count)
        throw new ArgumentOutOfRangeException(nameof(dirIdx));

      var metadata = _dirDefs[dirIdx];

      if (tileIdx < 0 || tileIdx >= metadata.TileCount)
        throw new ArgumentOutOfRangeException(nameof(tileIdx));

      // Yes, this has a *tiny* race condition if the users screws up and tries to write the tile twice simultaneously. NBD
      if (_tilesWritten[dirIdx][tileIdx] == true)
        throw new Exception("This tile has already been written.");

      if (data == null || data.Length != metadata.NumberCellsPerTile)
        throw new ArgumentException("Data should have " + metadata.NumberCellsPerTile.ToString() + " elements.");

      byte[] uncompData = TLUncompressedBuffer.Value;
      Buffer.BlockCopy(data, 0, uncompData, 0, uncompData.Length);

      // Do we have a theoretical max-size on the compressed data? Can we use a threadsafe<byte[]> to avoid the extra alloc?
      // ZLIB does, but I'm not sure the .net stuff would adhere to that. http://stackoverflow.com/questions/8902924/zlib-deflate-how-much-memory-to-allocate
      // If we just take a *reasonable* max, MemoryStream can realloc if need-be? Would be a rare case, might be more efficient that way?
      byte[] compData = null;
      int compDataValidLen = 0;
      using (MemoryStream decompMS = new MemoryStream(uncompData))
      {
        CompressTile(decompMS, ref compData, ref compDataValidLen);
      }

      /* ZLIB References
         http://stackoverflow.com/questions/9050260/what-does-a-zlib-header-look-like
         https://blogs.msdn.microsoft.com/bclteam/2007/05/16/system-io-compression-capabilities-kim-hamilton/
         "With our deflate implementation, those bytes are 0x58 and 0x85."
      */
      
      QueueWrite(dirIdx, tileIdx, 0x58, 0x85, compData, compDataValidLen, Adler32.ComputeAdler32(uncompData, 0, uncompData.Length));
    }
    
    private void QueueWrite(int dirIdx, int tileIdx, byte headerByte1, byte headerByte2, byte[] deflatedData, int validDeflatedDataLen, byte[] adler32 )
    {
      // Copy the deflated data to a new buffer
      int fullTileLen = 2 + validDeflatedDataLen + 4; // zlib header (2 bytes) + adler-32 checksum (4 bytes)
      byte[] queueBuf = new byte[fullTileLen];
      queueBuf[0] = headerByte1;
      queueBuf[1] = headerByte2;
      Buffer.BlockCopy(deflatedData, 0, queueBuf, 2, validDeflatedDataLen);
      int a32 = queueBuf.Length - 4;
      queueBuf[a32] = adler32[0];
      queueBuf[a32 + 1] = adler32[1];
      queueBuf[a32 + 2] = adler32[2];
      queueBuf[a32 + 3] = adler32[3];


      _queuedData[new Tuple<int, int>(dirIdx, tileIdx)] = queueBuf;

      // Track the total count of queued tiles
      Interlocked.Increment(ref _totalQueuedTiles);

      // Should we mark when they're queued, or written?

      // Note - just locking on the write, none of the reads. They don't have to be very threadsafe.
      // E.G. "ShouldFlush" is more of a guideline anyway.
      lock(_tilesWrittenLock)
        _tilesWritten[dirIdx][tileIdx] = true;

      if (ShouldFlush())
        Flush();
    }

    private int _totalQueuedTiles = 0;

    private bool ShouldFlush()
    {
      // Not threadsafe, doesnt matter. (This number is more of a guideline)
      int curSize = _queuedData.Count; 
      if (curSize == 0)
        return false;

      if (_enforceTilesWriteInOrder)
      {
        // Use size *and* [next tile is available]
        // Can we get away without a lock here? We need [waitingForDir] and [waitingForTile] to be changed/read atomically.
        lock (_waitingForNextLock)
        {
          if (_tilesWritten[_waitingForDir][_waitingForTile] && curSize > MaxTileQueueDepth)
            return true;
          else
            return false;
        }
      }
      else
      {
        // No order enforcement, just use the size.
        return (curSize > MaxTileQueueDepth);          
      }
    }

    private object _waitingForNextLock = new object();

    private bool _someoneElseIsWriting = false;
    private void Flush()
    {
      // Issue - 5 different threads detect we should *flush* because there are 50 tiles, then they all wait here on the lock.
      // Threads 2/3/4/5 each wait, and write 2-3 tiles each.
      // Solution: detect that someone else is writing, then walk away.
      if (_someoneElseIsWriting)
        return;

      lock(_writeLock)
      {
        _someoneElseIsWriting = true;

        // Get a collection of all tiles available to be written.
        // ToList crashed on me, I'm guessing a race condition. It's not implemented natively, but ToArray() is.
        // ToList is just an extension method. 
        // See http://referencesource.microsoft.com/#mscorlib/system/Collections/Concurrent/ConcurrentDictionary.cs
        var writables = _queuedData.ToArray(); // Save a point-in-time state? Necessary?
        if (writables.Length == 0)
        {
          _someoneElseIsWriting = false;
          return;
        }

        //System.Diagnostics.Debug.Print("Start: " + _totalQueuedTiles.ToString() + " tiles queued.");    
        
        if(_enforceTilesWriteInOrder)
        {
          var orderedKVPs = writables.OrderBy(t => t.Key.Item1).ThenBy(t => t.Key.Item2);
          var firstKVP = orderedKVPs.First();
          //System.Diagnostics.Debug.Print("Up for writing: " + firstKVP.Key.Item1.ToString() + ", " + firstKVP.Key.Item2.ToString());

          // Loop them in order <dir><tile>. 
          foreach (var kvp in orderedKVPs)
          {
            int dirIdx = kvp.Key.Item1;
            int tileIdx = kvp.Key.Item2;
            byte[] write = kvp.Value;

            // Can't write, not the right directory
            if (dirIdx != _waitingForDir)
              break;

            // Not the right tile index
            if (tileIdx != _waitingForTile)
              break;

            // Yay!
            _tileByteCounts[dirIdx][tileIdx] = write.Length;
            _tileByteLocations[dirIdx][tileIdx] = _fs.Length;
            _fs.Write(write, 0, write.Length);

            // Remove from dictionary
            byte[] ignore = null;
            _queuedData.TryRemove(kvp.Key, out ignore); 



            // Mark as written
            Interlocked.Decrement(ref _totalQueuedTiles);

            // Increment the tile to be written next.
            // Can we do this without locks????? 
            // _waitingForDir can be incremented before _waitingForTile is reset...
            // See usage in "SeeFlush" - dangerous to ask for dictionary[dir][tile]...
            lock (_waitingForNextLock)
            {
               if (_waitingForTile != _dirDefs[dirIdx].TileCount - 1)
                _waitingForTile += 1;
              else
              {
                // I think this is ok, even on the last tile? It will increment these OOB, 
                // but nobody should be reading this after the very last tile is written anyway?
                _waitingForDir += 1;
                _waitingForTile = 0;
              }
            }
          }
        }
        else
        {
          // Just write each tile, ignoring position
          foreach(var kvp in writables)
          {
            int dirIdx = kvp.Key.Item1;
            int tileIdx = kvp.Key.Item2;
            byte[] write = kvp.Value;

            // Write the data
            _tileByteCounts[dirIdx][tileIdx] = write.Length;
            _tileByteLocations[dirIdx][tileIdx] = _fs.Length;
            _fs.Write(write, 0, write.Length);

            // Remove from dictionary
            byte[] ignore = null;
            _queuedData.TryRemove(kvp.Key, out ignore);

            // Mark as written
            Interlocked.Decrement(ref _totalQueuedTiles);
          }
        }

        //System.Diagnostics.Debug.Print("End: " + _totalQueuedTiles.ToString() + " tiles queued.");

        _someoneElseIsWriting = false;
      }
    }

    private static void CompressTile(Stream dataStream, ref byte[] buf, ref int bufLen)
    {
      // Not obvious to me, but usage... http://stackoverflow.com/questions/10599596/compress-and-decompress-a-stream-with-compression-deflatestream
      using (MemoryStream compressStream = new MemoryStream())
      using (DeflateStream compressor = new DeflateStream(compressStream, CompressionMode.Compress, true)) // LeaveOpen = true, so we can do a true 'GetBuffer' call.
      {
        dataStream.CopyTo(compressor);
        compressor.Close();
        bufLen = (int)compressStream.Length;
        buf = compressStream.GetBuffer();
      }
    }


    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // Flush the rest of the tiles
          Flush();

          // Write all directory metadata, appended to file
          TLUncompressedBuffer.Dispose();
          TLUncompressedBuffer = null;

          // Validate first
          bool allWritten = true;
          for (int dirIdx = 0; dirIdx < _dirDefs.Count; dirIdx++)
          {
            var written = _tilesWritten[dirIdx];
            for(int i = 0; i < written.Length; i++)
              if(written[i] == false)
              {
                allWritten = false;
                break;
              }

            if (allWritten == false)
              break;
          }

          if(allWritten == false)
          {
            // Exception? ErrorStatus?
            _fs.Dispose();
            try { File.Delete(_filename); } catch { }            
            throw new Exception("All tiles need to be written before disposing the " + nameof(TiffFileWriter) + " for " + Path.GetFileName(_filename));
          }


          for (int dirIdx = 0; dirIdx < _dirDefs.Count; dirIdx++)
          {
            var meta = _dirDefs[dirIdx];
            long writeNextDirLoc = meta.AppendDirectory(_fs, null, _tileByteLocations[dirIdx], _tileByteCounts[dirIdx]);

            // On the very last iteration, we have to leave the 'next-directory' tag empty.
            if (dirIdx == _dirDefs.Count - 1)
              break;

            // Filestream has been advanced to next available directory location - 
            long nextDirLoc = _fs.Length;
            _fs.Seek(writeNextDirLoc, SeekOrigin.Begin);

            byte[] writeBackBytes = BitConverter.GetBytes(nextDirLoc);
            _fs.Write(writeBackBytes, 0, writeBackBytes.Length);

            // Jump back to the end, so the next directory can append successfully.
            _fs.Seek(0, SeekOrigin.End);
          }

          // Write the location of the very first directory definition.
          _fs.Position = 8;
          byte[] firstDirOffset = BitConverter.GetBytes((ulong)_dirDefs.First().DirectoryOffset);
          _fs.Write(firstDirOffset, 0, firstDirOffset.Length);

          // Done!
          _fs.Dispose();
          _dirDefs = null;
          _tilesWritten = null;
          _tileByteCounts = null;
          _tileByteLocations = null;
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~TiffFileWriter() {
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
