using System;
using Agriculture.CropSchedules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agriculture.Test
{
    [TestClass]
    public class SingleYearCropScheduleTest
    {
        [TestMethod]
        public void FloodedBeforePlanting()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984,2,4),2,200);
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 48.25;
            e.StartDate = new DateTime(1984, 1, 22);
            Agriculture.Interfaces.CropDamageCaseEnum outcome = apf.ComputeOutcome(e);
            Assert.IsTrue(outcome == Interfaces.CropDamageCaseEnum.NotFloodedDuringSeason);
        }
        [TestMethod]
        public void FloodedAfterHarvest()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 2, 4), 2, 200);
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 48.25;
            e.StartDate = new DateTime(1984, 08, 24);
            Agriculture.Interfaces.CropDamageCaseEnum outcome = apf.ComputeOutcome(e);
            Assert.IsTrue(outcome == Interfaces.CropDamageCaseEnum.NotFloodedDuringSeason);
        }
        [TestMethod]
        public void NotPlanted()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 2, 4), 2, 200);
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 448.25;
            e.StartDate = new DateTime(1984, 1, 30);
            Agriculture.Interfaces.CropDamageCaseEnum outcome = apf.ComputeOutcome(e);
            Assert.IsTrue(outcome == Interfaces.CropDamageCaseEnum.NotPlanted);
        }
        [TestMethod]
        public void PlantingDelayed()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 2, 4), 2, 200);
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 24.25;
            e.StartDate = new DateTime(1984, 2, 4);
            Agriculture.Interfaces.CropDamageCaseEnum outcome = apf.ComputeOutcome(e);
            Assert.IsTrue(outcome == Interfaces.CropDamageCaseEnum.PlantingDelayed);
        }
        [TestMethod]
        public void FloodedAfterPlanting_SpanningAYear()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 12, 16), 2, 200);
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 48.25;
            e.StartDate = new DateTime(1984, 1, 22);
            Agriculture.Interfaces.CropDamageCaseEnum outcome = apf.ComputeOutcome(e);
            Assert.IsTrue(outcome == Interfaces.CropDamageCaseEnum.Flooded);
        }
        [TestMethod]
        public void NotFloodedAfterPlanting_SpanningAYear()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 12, 16), 2, 200);
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 48.25;
            e.StartDate = new DateTime(1984, 12, 2);
            Agriculture.Interfaces.CropDamageCaseEnum outcome = apf.ComputeOutcome(e);
            Assert.IsTrue(outcome == Interfaces.CropDamageCaseEnum.NotFloodedDuringSeason);
        }
        [TestMethod]
        public void NotPlantedAfterPlanting_SpanningAYear()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 12, 16), 2, 200);
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 448.25;
            e.StartDate = new DateTime(1984, 12, 10);
            Agriculture.Interfaces.CropDamageCaseEnum outcome = apf.ComputeOutcome(e);
            Assert.IsTrue(outcome == Interfaces.CropDamageCaseEnum.NotPlanted);
        }
        [TestMethod]
        public void PlantingDelayedAfterPlanting_SpanningAYear()
        {
            Interfaces.ICropSchedule apf = new SingleYearCropSchedule(new DateTime(1984, 12, 16), 2, 200);
            Agriculture.EventProviders.AgricultureEvent e = new EventProviders.AgricultureEvent();
            e.DurationInDecimalHours = 24.25;
            e.StartDate = new DateTime(1983, 12, 16);
            Agriculture.Interfaces.CropDamageCaseEnum outcome = apf.ComputeOutcome(e);
            Assert.IsTrue(outcome == Interfaces.CropDamageCaseEnum.PlantingDelayed);
        }
    }
}
