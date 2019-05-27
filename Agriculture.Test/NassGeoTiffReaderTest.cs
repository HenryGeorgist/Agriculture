using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Agriculture.Test
{
    [TestClass]
    public class NassGeoTiffReaderTest
    {
        [TestMethod]
        public void ReadTiff()
        {
            Agriculture.Nass.BaseTiffFilter filter = new Nass.BaseTiffFilter();
            Agriculture.Nass.NassGeoTiffReader reader = new Nass.NassGeoTiffReader(@"C:\Temp\NASS\Test.tif");
            IList<Agriculture.Interfaces.ICropLocation> locs = reader.FilteredCrops(filter);
        }
    }
}
