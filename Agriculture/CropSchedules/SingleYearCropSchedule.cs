using System;
using Agriculture.Interfaces;

namespace Agriculture.CropSchedules
{
    public class SingleYearCropSchedule : ICropSchedule
    {
        private readonly int _NumDaysToMaturity;
        private readonly int _StartDayOfYear;
        private DateTime _StartDate;
        private readonly int _LastPlantDayOfYear;
        public int NumberOfDaysToMaturity
        {
            get
            {
                return _NumDaysToMaturity;
            }
        }
        public int StartPlantDayOfYear
        {
            get
            {
                return _StartDayOfYear;
            }
        }
        public int LastPlantDayOfYear
        {
            get
            {
                return _LastPlantDayOfYear;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return _StartDate;
            }
            set
            {
                _StartDate = value;
            }
        }

        public virtual int CropID { get; set; }

        public virtual string CropName { get; set; }

        public SingleYearCropSchedule(DateTime startDate, int plantingWindowInDays, int daysToMaturity)
        {
            StartDate = startDate;
            _StartDayOfYear = startDate.DayOfYear;
            _LastPlantDayOfYear = StartPlantDayOfYear + plantingWindowInDays;
            _NumDaysToMaturity = daysToMaturity;
        }
        public CropDamageCaseEnum ComputeOutcome(IAgriculturalFloodEvent floodEvent)
        {
            System.DateTime firstOfYear = new DateTime(floodEvent.StartDate.Year, 1, 1);
            System.TimeSpan TimSpan = floodEvent.StartDate.Subtract(firstOfYear);
            Agriculture.Interfaces.CropDamageCaseEnum outcome = CropDamageCaseEnum.Unassigned;
            int harvestDayOfYear = StartPlantDayOfYear + NumberOfDaysToMaturity;
            int daysFromFirstofYear = TimSpan.Days + 1;//TotalDays provides more accuracy by putting the event at an actual time rather than midnight on the day of.
            if (daysFromFirstofYear <= StartPlantDayOfYear)
            {
                //flooded before planting occured in this year - check duration of flooding against start day of year.
                //and check if flooding occured before harvest of last year's crop?
                if (daysFromFirstofYear + (floodEvent.DurationInDecimalHours / 24) < StartPlantDayOfYear)
                {
                    //flooded well before plant date?
                    outcome = CropDamageCaseEnum.NotFloodedDuringSeason;
                }
                else
                {
                    //last plant day of year.
                    if (daysFromFirstofYear + (floodEvent.DurationInDecimalHours / 24) < LastPlantDayOfYear)
                    {
                        outcome = CropDamageCaseEnum.PlantingDelayed;
                    }
                    else
                    {
                        outcome = CropDamageCaseEnum.NotPlanted;
                    }

                }
                if (harvestDayOfYear > 365)
                {
                    if (harvestDayOfYear - 365 > daysFromFirstofYear)
                    {
                        //flooded before harvest.
                        outcome = CropDamageCaseEnum.Flooded;
                    }
                    else
                    {
                        //flooded between plantings
                    }
                }
                else
                {
                    //flooded between plantings

                }
            }
            else
            {
                //flooded after planting, check if flooded after harvest also.
                if (harvestDayOfYear < daysFromFirstofYear)
                {
                    //flooded after harvest (betweenplantings)
                    outcome = CropDamageCaseEnum.NotFloodedDuringSeason;
                }
                else
                {
                    //flooded before harvest.
                    outcome = CropDamageCaseEnum.Flooded;
                }
            }
            return outcome;
        }
    }
}
