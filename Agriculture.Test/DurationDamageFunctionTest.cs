using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Agriculture.Test
{
    [TestClass]
    public class DurationDamageFunctionTest
    {
        private Interfaces.ICropDamageFunction _DamageFunction;
        [TestInitialize]
        public void Initialize()
        {
            Dictionary<double, List<double>> damFun = new Dictionary<double, List<double>>();
            List<double> oneDay = new List<double>() {0.0d, .5d, .75d, .8d, .9d, .1d, .1d, .1d, 0.0d, 0.0d, 0.0d, 0.0d };
            List<double> twoDay = new List<double>() { 0.0d, 5.0d, 7.5d, 8.0d, 9.0d, 10.0d, 10.0d, 10.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            List<double> threeDay = new List<double>() { 0.0d, 50.0d, 75.0d, 80.0d, 90.0d, 100.0d, 100.0d, 100.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            damFun.Add(24.0d, oneDay);
            damFun.Add(48.0d, twoDay);
            damFun.Add(72.0d, threeDay);
            _DamageFunction = new Agriculture.DamageFunctions.DurationDamageFunction(damFun);
            
        }
        [TestMethod]
        public void ComputeDamageForDurationLongerThanKeys()
        {
            EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 100.0d;
            e.StartDate = new DateTime(1984, 3, 15);
            _DamageFunction.ComputeDamagePercent(e);
            double damage = _DamageFunction.ComputeDamagePercent(e);
            Assert.AreEqual((75.0d)/100.0d, damage);
        }
        [TestMethod]
        public void ComputeDamageForDurationShorterThanKeys()
        {
            EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 12.0d;
            e.StartDate = new DateTime(1984, 3, 15);
            double damage = _DamageFunction.ComputeDamagePercent(e);
            Assert.AreEqual(((.5 * (.75)))/100, damage);
        }
        [TestMethod]
        public void ComputeDamageForDurationEqualToKey()
        {
            EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 48.0d;
            e.StartDate = new DateTime(1984, 3, 15);
            double damage = _DamageFunction.ComputeDamagePercent(e);
            Assert.AreEqual((7.5)/100 , damage);
        }
        [TestMethod]
        public void ComputeDamageForDurationBetweenTwoKeys()
        {
            EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 36.0d;
            e.StartDate = new DateTime(1984, 3, 15);
            double damage = _DamageFunction.ComputeDamagePercent(e);
            Assert.AreEqual((.75+(.5 * (7.5 - .75)))/100, damage);
        }
        [TestMethod]
        public void ComputeDamageForDurationBetweenLastTwoKeys()
        {
            EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 60.0d;
            e.StartDate = new DateTime(1984, 3, 15);
            double damage = _DamageFunction.ComputeDamagePercent(e);
            Assert.AreEqual((7.5 + (.5 * (75 - 7.5)))/100, damage);
        }
    }
}
