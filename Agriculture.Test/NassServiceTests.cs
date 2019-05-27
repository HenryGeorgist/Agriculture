using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Agriculture.Nass;
using System.Threading.Tasks;

namespace Agriculture.Test
{
    [TestClass]
    public class NassServiceTests
    {
        [TestMethod]
        public void NassStatistics()
        {
            awaitableStatistics().Wait();
        }
        private async Task awaitableStatistics()
        {
            Statistics s = new Statistics();
            await s.GetStatistics("2011", 130783.786503, 2203171.19972, 153923.584713, 2217961.586205);
            Agriculture.Nass.Json.StatisticsResult r = s.Result;
            Assert.IsTrue(r.success);
        }
    }
}
