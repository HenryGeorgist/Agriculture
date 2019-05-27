using System;
using Agriculture.CropSchedules;
using Agriculture.ProductionFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Agriculture.Test
{
    [TestClass]
    public class FixedCostTests
    {
        [TestMethod]
        public void Cumulation_WithinAYear()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 2, 4), 2, 200);
            System.Collections.Generic.List<double> monthlyCosts = new System.Collections.Generic.List<double>() { 0.0d, 10.0d, 10.0d, 10.0d, 10.0d, 10.0d, 10.0d, 2.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            Interfaces.ICropProductionFunction cpf = new AgProductionFunctionWithFixedCosts(monthlyCosts,10, apf, .1, 10);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[0] == 50);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[1] == 70);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[2] == 90);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[3] == 110);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[4] == 130);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[5] == 150);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[6] == 170);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[7] == 182);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[8] == 10);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[9] == 20);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[10] == 30);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[11] == 40);
        }
        [TestMethod]
        public void Cumulation_SpanningTwoYears()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 10, 4), 2, 200);
            System.Collections.Generic.List<double> monthlyCosts = new System.Collections.Generic.List<double>() { 10.0d, 10.0d, 10.0d, 2.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 10.0d, 10.0d, 10.0d };
            Interfaces.ICropProductionFunction cpf = new AgProductionFunctionWithFixedCosts(monthlyCosts,10, apf, .1, 10);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[0] == 130);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[1] == 150);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[2] == 170);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[3] == 182);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[4] == 10);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[5] == 20);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[6] == 30);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[7] == 40);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[8] == 50);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[9] == 70);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[10] == 90);
            Assert.IsTrue(cpf.CumulativeMonthlyCosts[11] == 110);
        }
    }
}
