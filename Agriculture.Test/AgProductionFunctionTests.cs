using System;
using Agriculture.CropSchedules;
using Agriculture.ProductionFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agriculture.Test
{
    [TestClass]
    public class AgProductionFunctionTests
    {
        [TestMethod]
        public void Cumulation_WithinAYear()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 2, 4), 2, 200);
            System.Collections.Generic.List<double> monthlyCosts = new System.Collections.Generic.List<double>(){ 0.0d,10.0d,10.0d,10.0d,10.0d,10.0d,10.0d,2.0d,0.0d,0.0d,0.0d,0.0d};
            Interfaces.ICropProductionFunction cpf = new AgProductionFunction(monthlyCosts, apf,.1,10);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[0] == 0);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[1] == 10);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[2] == 20);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[3] == 30);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[4] == 40);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[5] == 50);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[6] == 60);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[7] == 62);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[8] == 0);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[9] == 0);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[10] == 0);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[11] == 0);
        }
        [TestMethod]
        public void Cumulation_SpanningTwoYears()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 10, 4), 2, 200);
            System.Collections.Generic.List<double> monthlyCosts = new System.Collections.Generic.List<double>() { 10.0d, 10.0d, 10.0d, 2.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 10.0d, 10.0d, 10.0d };
            Interfaces.ICropProductionFunction cpf = new AgProductionFunction(monthlyCosts, apf,.1,10);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[0] == 40);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[1] == 50);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[2] == 60);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[3] == 62);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[4] == 0);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[5] == 0);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[6] == 0);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[7] == 0);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[8] == 0);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[9] == 10);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[10] == 20);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[11] == 30);
        }
    }
}
