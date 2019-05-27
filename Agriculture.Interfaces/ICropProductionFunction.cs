using System.Collections.Generic;
namespace Agriculture.Interfaces
{
    public interface ICropProductionFunction: IRelateCrops
    {
        IList<double> MonthlyVariableCosts { get; }
        double MonthlyFixedCosts { get; set; }

        IList<double> CumulativeMonthlyCosts { get; }
        double CumulativeCostsLessHarvest { get; }
        double OutputLossDueToLatePlant { get; set; }
        double HarvestCost { get; set; }//possibly a distribution
        double ComputeExposedValue(IAgriculturalFloodEvent floodEvent);
    }
}