using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Agriculture.Nass;
using System.Threading.Tasks;

namespace Agriculture.Test
{
    [TestClass]
    public class NassTiffServiceTest
    {
        [TestMethod]
        public void NassGetTiff()
        {
            awaitableStatistics().Wait();
        }
        private async Task awaitableStatistics()
        {
            GeoTiff s = new GeoTiff();
            await s.RequestFileForBoundingBox(@"C:\Temp\NASS\Test.tif","2011", 130783.786503,2203171.19972,153923.584713,2217961.586205);
        }
    }
}
