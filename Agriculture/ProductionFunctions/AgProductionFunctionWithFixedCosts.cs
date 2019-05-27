using System;
using System.Linq;
using System.Collections.Generic;
using Agriculture.Interfaces;

namespace Agriculture.ProductionFunctions
{
    public class AgProductionFunctionWithFixedCosts : BaseProductionFunction
    {
        private double _MonthlyFixedCosts;
        public new double MonthlyFixedCosts
        {
            get { return _MonthlyFixedCosts; }
            set { _MonthlyFixedCosts = value; }
        }
        public AgProductionFunctionWithFixedCosts(List<double> monthlyVariableCosts, double fixedCost, ICropSchedule schedule, double lossFromLatePlant, double harvestCost) : base(monthlyVariableCosts, schedule, lossFromLatePlant, harvestCost)
        {
            MonthlyFixedCosts = fixedCost;
            AddFixedMonthlyCost(schedule);
            _TotalCostLessHarvest = CumulativeMonthlyCosts.Max();
        }
        private void AddFixedMonthlyCost(ICropSchedule schedule)
        {
            DateTime harvestDate = schedule.StartDate.AddDays((double)schedule.NumberOfDaysToMaturity);
            if (harvestDate.Year != schedule.StartDate.Year)
            {
                harvestDate = harvestDate.AddYears(-1);
            }
            double sum = 0;
            for (int i = harvestDate.Month; i < 12; i++)
            {
                MonthlyVariableCosts[i] += MonthlyFixedCosts;
                sum += MonthlyFixedCosts;
                CumulativeMonthlyCosts[i] += sum;
            }
            for (int i = 0; i < harvestDate.Month; i++)
            {
                MonthlyVariableCosts[i] += MonthlyFixedCosts;
                sum += MonthlyFixedCosts;
                CumulativeMonthlyCosts[i] += sum;
            }
        }
    }
}
