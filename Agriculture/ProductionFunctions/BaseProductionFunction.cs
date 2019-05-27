using System;
using System.Collections.Generic;
using System.Linq;
using Agriculture.Interfaces;

namespace Agriculture.ProductionFunctions
{
    public class BaseProductionFunction: Interfaces.ICropProductionFunction
    {
        private double _HarvestCost;
        private readonly List<double> _MonthlyCosts;
        private readonly List<double> _CumulativeMonthlyCosts;
        protected double _TotalCostLessHarvest;
        private double _lossFromLatePlant;
        public virtual int CropID { get; set; }
        public virtual string CropName { get; set; }
        public double HarvestCost
        {
            get
            {
                return _HarvestCost;
            }

            set
            {
                _HarvestCost = value;
            }
        }
        public double OutputLossDueToLatePlant
        {
            get
            {
                return _lossFromLatePlant;
            }

            set
            {
                _lossFromLatePlant = value;
            }
        }
        public double MonthlyFixedCosts { get; set; }
        public IList<double> MonthlyVariableCosts
        {
            get
            {
                return _MonthlyCosts;
            }
        }
        public IList<double> CumulativeMonthlyCosts
        {
            get
            {
                return _CumulativeMonthlyCosts;
            }
        }
        public double CumulativeCostsLessHarvest
        {
            get
            {
                return _TotalCostLessHarvest;
            }
        }
        public BaseProductionFunction(List<double> monthlyCosts, ICropSchedule schedule, double lossFromLatePlant, double harvestCost)
        {
            _MonthlyCosts = monthlyCosts;
            _CumulativeMonthlyCosts = new List<double>();
            OutputLossDueToLatePlant = lossFromLatePlant;
            HarvestCost = harvestCost;
            //cumulate based on the schedule
            CumulateMonthlyCost(schedule);
            _TotalCostLessHarvest = CumulativeMonthlyCosts.Max();
        }
        public virtual double ComputeExposedValue(IAgriculturalFloodEvent floodEvent)
        {
            //Assume this is being called when we know the crop is flooded.
            double factor = floodEvent.StartDate.Day / DateTime.DaysInMonth(floodEvent.StartDate.Year, floodEvent.StartDate.Month);//ensure this is a double.
            if (floodEvent.StartDate.Month == 1)
            {
                return CumulativeMonthlyCosts[12] + (MonthlyVariableCosts[0] * factor);
            }
            else
            {
                return CumulativeMonthlyCosts[floodEvent.StartDate.Month - 2] + (MonthlyVariableCosts[floodEvent.StartDate.Month - 1] * factor);
            }
        }
        protected virtual void CumulateMonthlyCost(ICropSchedule schedule)
        {
            if (schedule.StartPlantDayOfYear + schedule.NumberOfDaysToMaturity > 365)
            {
                //spanning a year
                int daysinyear = 365;
                if (DateTime.IsLeapYear(schedule.StartDate.Year))
                {
                    daysinyear += 1;
                }
                int daysremaining = daysinyear - schedule.StartPlantDayOfYear;
                int daysinnextYear = schedule.NumberOfDaysToMaturity - daysremaining;
                int startmonthidx = schedule.StartDate.Month - 1;
                for (int i = 0; i < startmonthidx; i++)
                {
                    _CumulativeMonthlyCosts.Add(0);//initalize with zeros.
                }
                int counter = 0;
                double sum = 0;
                do
                {
                    sum += _MonthlyCosts[startmonthidx + counter];
                    _CumulativeMonthlyCosts.Add(sum);
                    daysremaining -= DateTime.DaysInMonth(schedule.StartDate.Year, schedule.StartDate.Month + counter);
                    counter++;
                } while (daysremaining > 0);
                counter = 0;
                do
                {
                    sum += _MonthlyCosts[counter];
                    _CumulativeMonthlyCosts[counter] = sum;
                    daysinnextYear -= DateTime.DaysInMonth(schedule.StartDate.Year + 1, counter + 1);
                    counter++;
                } while (daysinnextYear > 0);
            }
            else
            {
                //within a year.
                int daysremaining = schedule.NumberOfDaysToMaturity;
                int startmonthidx = schedule.StartDate.Month - 1;
                for (int i = 0; i < startmonthidx; i++)
                {
                    _CumulativeMonthlyCosts.Add(0);
                }
                int counter = 0;
                double sum = 0;
                do
                {
                    sum += _MonthlyCosts[startmonthidx + counter];
                    _CumulativeMonthlyCosts.Add(sum);
                    daysremaining -= DateTime.DaysInMonth(schedule.StartDate.Year, schedule.StartDate.Month);
                    counter++;
                } while (daysremaining > 0);
                if (startmonthidx + counter < 12)
                {
                    for (int i = startmonthidx + counter; i < 12; i++)
                    {
                        _CumulativeMonthlyCosts.Add(0);
                    }
                }
            }
        }
    }
}
