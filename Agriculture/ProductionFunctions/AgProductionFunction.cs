using System.Collections.Generic;
using Agriculture.Interfaces;

namespace Agriculture.ProductionFunctions
{
    //public delegate void DelayedPlantingHandler(int daysDelayed, int plantingDuration);
    public class AgProductionFunction : BaseProductionFunction
    {
        public AgProductionFunction(List<double> monthlyCosts, ICropSchedule schedule, double lossFromLatePlant, double harvestCost):base(monthlyCosts,schedule,lossFromLatePlant,harvestCost)
        {

        }
    }
}
