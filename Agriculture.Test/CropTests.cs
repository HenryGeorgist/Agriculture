using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Agriculture.CropSchedules;
using Agriculture.ProductionFunctions;
using Agriculture.Crops;
namespace Agriculture.Test
{
    [TestClass]
    public class CropTests
    {
        private Crop _Crop;

        [TestInitialize]
        public void Initilize()
        {
            Interfaces.ICropSchedule cs = new SingleYearCropSchedule(new DateTime(1984, 2, 4), 10, 200);
            System.Collections.Generic.List<double> monthlyCosts = new System.Collections.Generic.List<double>() { 0.0d, 10.0d, 10.0d, 10.0d, 10.0d, 10.0d, 10.0d, 2.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            Interfaces.ICropProductionFunction cpf = new AgProductionFunction(monthlyCosts, cs,.1,10);

            Dictionary<double, List<double>> damFun = new Dictionary<double, List<double>>();
            List<double> oneDay = new List<double>() { 0.0d, .5d, .75d, .8d, .9d, .1d, .1d, .1d, 0.0d, 0.0d, 0.0d, 0.0d };
            List<double> twoDay = new List<double>() { 0.0d, 5.0d, 7.5d, 8.0d, 9.0d, 10.0d, 10.0d, 10.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            List<double> threeDay = new List<double>() { 0.0d, 50.0d, 75.0d, 80.0d, 90.0d, 100.0d, 100.0d, 100.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            damFun.Add(24.0d, oneDay);
            damFun.Add(48.0d, twoDay);
            damFun.Add(72.0d, threeDay);
            DamageFunctions.DurationDamageFunction DamageFunction = new Agriculture.DamageFunctions.DurationDamageFunction(damFun);
            _Crop = new Crop("Test", 0, 10, 10, cs, cpf, DamageFunction);
        }
        [TestMethod]
        public void LatePlantingDamage()
        {
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 48.25;
            e.StartDate = new DateTime(1984, 2, 3);
            double loss = _Crop.ComputeDamages(e).Damage;
            Assert.IsTrue(loss == 1);
        }
        [TestMethod]
        public void NoPlantingDamage()
        {
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 448.25;
            e.StartDate = new DateTime(1984, 1, 30);
            Assert.IsTrue(_Crop.ComputeDamages(e).Damage == 0);
        }
        [TestMethod]
        public void FloodedDamage()
        {
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 48.0d;
            e.StartDate = new DateTime(1984, 6, 15);
            double loss = _Crop.ComputeDamages(e).Damage;
            Assert.IsTrue(loss == 5.8064516129032256);
        }
        [TestMethod]
        public void FloodedDamage_LongEvent()
        {
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 72.0d;
            e.StartDate = new DateTime(1984, 6, 15);
            double loss = _Crop.ComputeDamages(e).Damage;
            Assert.IsTrue(loss == 58.064516129032256);
        }
        [TestMethod]
        public void NotFloodedDamage()
        {
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 48.25;
            e.StartDate = new DateTime(1984, 1, 22);
            
            Assert.IsTrue(_Crop.ComputeDamages(e).Damage == 0);
        }
    }
}
