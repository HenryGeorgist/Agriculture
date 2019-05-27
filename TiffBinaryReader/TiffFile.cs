using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TiffBinaryReader
{
  [DebuggerDisplay("{System.IO.Path.GetFileName(Filename)}")]
  public class TiffFile : IDisposable
  {
    #region Static File/Reader Management
    /*
      Writing? Dictionary of filename -> list of open readers? So when we open a writer, we can ensure no other readers are opened?
      Because of the cached tile-locations, we can't have it simultaneously open for read/write.
      
      Write automatically with NoData tile compression?

      Only allow a single writer per filename? Seems best.

      Should we have the write calls on the tiff itself? That seems poor practice, 80% of the time it just clutters intellisense.

      BETTER IDEA - for now, ONLY ALLOW CREATE-NEW!!!
    */

    private static object FileListLock = new object();
    private static HashSet<TiffFile> OpenFileReaders = new HashSet<TiffFile>();

    private static void AddReader(TiffFile tf)
    {
      lock (FileListLock)
      {
        OpenFileReaders.Add(tf);
      }
    }
    private static void ReaderDisposed(TiffFile tf)
    {
      lock (FileListLock)
      {
        OpenFileReaders.Remove(tf);
      }
    }
    private static bool ContainsReaderFor(string filename)
    {
      return GetOpenReadersFor(filename).Count > 0;
    }
    private static List<TiffFile> GetOpenReadersFor(string filename, bool isAlreadyLocked = false)
    {
      // This isn't foolproof.
      // http://stackoverflow.com/questions/410705/best-way-to-determine-if-two-path-reference-to-same-file-in-c-sharp
      
      var retn = new List<TiffFile>();
      Action Run = () =>
      {
        var fnFixed = Path.GetFullPath(filename).ToLower();
        foreach (TiffFile tf in OpenFileReaders)
        {
          var fnQuery = Path.GetFullPath(tf.Filename).ToLower();
          if (fnFixed == fnQuery)
            retn.Add(tf);
        }
      };

      // Avoid a deadlock if the filereader list is already locked.
      if(isAlreadyLocked)
      {
        Run();
      }
      else
      {
        lock (FileListLock)
        {
          Run();
        }
      }

      return retn;
    }

    /// <summary>
    /// Loop through any open readers for a particular file, and suspend them. This allows us to open a writer for that file,
    /// which would otherwise invalidate cached data. Call <see cref="RefreshReadersFor(string)"/> to re-open these readers.
    /// </summary>
    /// <param name="filename"></param>
    public static void SuspendReadersFor(string filename)
    {
      var files = GetOpenReadersFor(filename);
      foreach (var tf in files)
        tf.ClearAllDirectories();
    }

    /// <summary>
    /// Inverts the <see cref="ContainsReaderFor(string)"/> call. Refreshes all directories on all readers for this filename,
    /// and reopens them for tile reading.
    /// </summary>
    /// <param name="filename"></param>
    public static void RefreshReadersFor(string filename)
    {
      var files = GetOpenReadersFor(filename);
      foreach (var tf in files)
        tf.RefreshAllDirectories();

    }



    #endregion


    #region Constants
    public const ushort REGTIFF_ID = 42;
    public const ushort BIGTIFF_ID = 43;
    #endregion
    public string ErrorMessage { get; private set; }
    public string Filename { get; private set; }
    public bool LittleEndian { get; private set; }
    public bool IsBigTiff { get; private set; }


    private List<TiffDirectory> _directories;
    public ReadOnlyCollection<TiffDirectory> Directories { get { return new ReadOnlyCollection<TiffDirectory>(_directories); } }

    private ThreadLocal<FileStream> TLFilestreams;
    internal FileStream GetReadonlyFilestream() => TLFilestreams.Value;

    /* Couple of different ways we could handle this. 
       We could have TiffFile be IDisposable, and always keep a binary-reader open. We have to deal with parallelization a bit oddly -
       choose to use the open BR, or spawn new ones. If we use this like our TiffAssist object, we have to strongly back-down from
       the current method of 'load all tags up front'.
       
       We could have TiffFile be a create-once type of object, and just keep the metadata state (like tile locations) in-memory. 
       Then, we could open BRs on-demand whenever a user wants a tile. 

       Are we going to support arbitrary tiff writing? We can track file-times well enough, I guess? That becomes a PITA to check 
       validation *everywhere*, and easy to let bugs in. Maybe the binary-readers can have a 'no-write' lock, so we only have to 
       check file times when there are zero open BRs?

       Should we support lazy-loading tags? Seems bad/useless for most of our cases. We're not really trying to support 100MBs of tag data...
       Then again, if we're constantly opening/closing the TiffFile object, we don't want to spend that much time loading tags.

       How should we deal with different data types? TiffDirectory<float>, which can only belong to a TiffFile<float> ? Technically 
       a tiff-file can have many types of directories! Are we looking to support these more-arbitrary cases?

    */
    
    public TiffFile(string filename)
    {
      Filename = filename;
      _directories = new List<TiffDirectory>();
      
      TLFilestreams = new ThreadLocal<FileStream>(() => new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read), true);

      FileStream fs = TLFilestreams.Value; // Don't dispose these, we're keeping it around.
      BinaryReader br = new BinaryReader(fs);
      
      var startupBytes = br.ReadBytes(16);
          
      // Little Endian vs. Big Endian
      if (startupBytes[0] == 73 && startupBytes[1] == 73)
      {
        LittleEndian = true;
      }
      else
      {
        LittleEndian = false;
        ErrorMessage = "Big Endian files currently unsupported.";
        return;
      }
          
      ushort identifier = BitConverter.ToUInt16(startupBytes, 2);
      if (identifier == REGTIFF_ID)
        IsBigTiff = false;
      else if (identifier == BIGTIFF_ID)
        IsBigTiff = true;
      else
      {
        ErrorMessage = "File is not a well-formatted tiff: The magic number does not correspond to either a regular TIFF or BigTIFF.";
        return;
      }

      _directories.AddRange(TiffDirectory.Parse(this, br));
    }

    internal void ClearAllDirectories()
    {
      // Used in the static Suspend/Refresh readers calls.
      foreach (TiffDirectory dir in _directories)
        dir.Dispose();

      _directories.Clear();
    }
    internal void RefreshAllDirectories()
    {
      // Used in the static Refresh reader call.
      FileStream fs = TLFilestreams.Value;
      BinaryReader br = new BinaryReader(fs);
      _directories.AddRange(TiffDirectory.Parse(this, br));
    }

        
    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // Dispose the filestreams from the different thread accessors.
          foreach (FileStream fs in TLFilestreams.Values)
            fs.Dispose();

          // Dispose the children
          foreach (TiffDirectory dir in _directories)
            dir.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~TiffFile() {
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


    // General help
    //https://bitmiracle.github.io/libtiff.net/html/abdb7196-7bdc-4dee-88c7-fe25459575df.htm
    
  }
}
