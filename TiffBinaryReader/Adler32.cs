using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffBinaryReader
{
  // Copied from Adler32.cs in the LibTiff.NET project
  internal class Adler32
  {

    // largest prime smaller than 65536
    private const int BASE = 65521;
    // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
    private const int NMAX = 5552;
    
    internal static byte[] ComputeAdler32(byte[] buf, int index, int len)
    {
      // guess, based on the wiki (LOOKS RIGHT - compared two different tiles with LibTiff.Net, and the checksums matched.
      long adlerL = ComputePartialAdler32(1L, buf, index, len);
      
      // Taken from Deflate.cs, function deflate(ZStream strm, int flush)
      int adlerToInt = (int)URShift(adlerL, 16);

      byte[] firstTwo = PutShortMSB(adlerToInt);
      byte[] nextTwo = PutShortMSB((int)(adlerL & 0xffff));

      byte[] all4 = new byte[4];
      all4[0] = firstTwo[0];
      all4[1] = firstTwo[1];
      all4[2] = nextTwo[0];
      all4[3] = nextTwo[1];

      return all4;
    }

    // I *think* this is "building" an adler, given several input byte buffers? 
    // Input the previous adler, and it returns the built-so-far adler value?
    // backed up by bottom of : https://tools.ietf.org/html/rfc1950
    internal static long ComputePartialAdler32(long adler, byte[] buf, int index, int len)
    {
      if (buf == null)
      {
        return 1L;
      }

      // https://en.wikipedia.org/wiki/Adler-32
      // s1 = a, s2 = b from the wiki article.

      long s1 = adler & 0xffff;
      long s2 = (adler >> 16) & 0xffff;
      int k;

      while (len > 0)
      {
        k = len < NMAX ? len : NMAX;
        len -= k;
        while (k >= 16)
        {
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          s1 += (buf[index++] & 0xff); s2 += s1;
          k -= 16;
        }
        if (k != 0)
        {
          do
          {
            s1 += (buf[index++] & 0xff); s2 += s1;
          }
          while (--k != 0);
        }
        s1 %= BASE;
        s2 %= BASE;
      }
      return (s2 << 16) | s1;
    }

    // Taken from zlib ... how do we long->int???
    private static long URShift(long number, int bits)
    {
      if (number >= 0)
        return number >> bits;
      else
        return (number >> bits) + (2L << ~bits);
    }
    private static byte[] PutShortMSB(int b)
    {
      // Adapted from their 'putByte' call.
      return new byte[] { (byte)(b >> 8), (byte)(b) };
    }

  }
}
