using Agriculture.Interfaces;

namespace Agriculture.Crops
{
    public abstract class BaseCrop : ICrop
    {
        private int _ID;
        private string _CropName;
        //private int _DryOutPeriodInDays;
        private double _Yeild; //as a distribution?
        private double _PricePerUnit; //as a distribution?
        private ICropProductionFunction _ProductionFunction;
        private ICropDamageFunction _DamageFunction;
        private ICropSchedule _Schedule;
        public double ValuePerOutputUnit
        {
            get
            {
                return _PricePerUnit;
            }

            set
            {
                _PricePerUnit = value;
            }
        }
        public double OutputUnitsPerAcre
        {
            get
            {
                return _Yeild;
            }

            set
            {
                _Yeild = value;
            }
        }
        public int CropID
        {
            get
            {
                return _ID;
            }

            set
            {
                _ID = value;
            }
        }
        public string CropName
        {
            get
            {
                return _CropName;
            }

            set
            {
                _CropName = value;
            }
        }
        public ICropDamageFunction DamageFunction
        {
            get
            {
                return _DamageFunction;
            }
        }
        public ICropProductionFunction ProductionFunction
        {
            get
            {
                return _ProductionFunction;
            }
        }
        public ICropSchedule Schedule
        {
            get
            {
                return _Schedule;
            }
        }
        public BaseCrop(string name, int id, double pricePerUnit, double outputPerAcre, ICropSchedule schedule, ICropProductionFunction productionFunction, ICropDamageFunction damageFunction)
        {
            CropName = name;
            CropID = id;
            ValuePerOutputUnit = pricePerUnit;
            OutputUnitsPerAcre = outputPerAcre;
            _Schedule = schedule;
            _ProductionFunction = productionFunction;
            _DamageFunction = damageFunction;
        }
        public ICropResult ComputeDamages(IAgriculturalFloodEvent floodEvent)
        {
            double totalValue = TotalAnnualValue();
            double totalValueLessHarvest = totalValue - ProductionFunction.HarvestCost;
            Interfaces.CropDamageCaseEnum outcome = Schedule.ComputeOutcome(floodEvent);
            ICropResult result = new Agriculture.Compute.Results.CropResult();
            result.CropID = CropID;
            result.CropName = CropName;
            result.X = floodEvent.X;
            result.Y = floodEvent.Y;
            result.DamageCase = outcome;
            switch (outcome)
            {
                case CropDamageCaseEnum.NotFloodedDuringSeason:
                    ComputeNotFloodedDollarDamages(floodEvent, ref result);
                    return result;
                case CropDamageCaseEnum.Flooded:
                    ComputeFloodedDollarDamages(floodEvent, ref result);
                    return result;
                case CropDamageCaseEnum.NotPlanted:
                    ComputeNotPlantedDollarDamages(floodEvent, ref result);
                    return result;
                case CropDamageCaseEnum.PlantingDelayed:
                    ComputePlantingDelayedDamages(floodEvent, ref result);
                    return result;
                default:
                    result.Damage = 0.0d;
                    return result;
            }
        }
        protected virtual void ComputeNotFloodedDollarDamages(IAgriculturalFloodEvent floodEvent, ref ICropResult result)
        {
            result.Damage =  0.0d;
        }
        protected virtual void ComputeFloodedDollarDamages(IAgriculturalFloodEvent floodEvent, ref ICropResult result)
        {
            double totalValue = TotalAnnualValue();
            double totalValueLessHarvest = totalValue - ProductionFunction.HarvestCost;
            double exposedvalue = ProductionFunction.ComputeExposedValue(floodEvent);
            double damagePercent = DamageFunction.ComputeDamagePercent(floodEvent);
            double totalCost = ProductionFunction.CumulativeCostsLessHarvest;
            double proRatedValue = exposedvalue / totalCost;
            result.Damage = damagePercent * proRatedValue * totalValueLessHarvest;
        }
        protected virtual void ComputeNotPlantedDollarDamages(IAgriculturalFloodEvent floodEvent, ref ICropResult result)
        {
            result.Damage = 0;
        }
        protected virtual void ComputePlantingDelayedDamages(IAgriculturalFloodEvent floodEvent, ref ICropResult result)
        {
            double totalValue = TotalAnnualValue();
            int plantingWindow = Schedule.LastPlantDayOfYear - Schedule.StartPlantDayOfYear;
            System.DateTime floodEndDate = floodEvent.StartDate.AddHours(floodEvent.DurationInDecimalHours);
            int ActualPlantDayOfYear = floodEndDate.DayOfYear;
            int daysLate = ActualPlantDayOfYear - Schedule.StartPlantDayOfYear;
            double factor = (double)daysLate / (double)plantingWindow;
            double actualValue = totalValue * (1 - (ProductionFunction.OutputLossDueToLatePlant * factor));//gradient based on actual planting date?
            result.Damage =  totalValue - actualValue;
        }
        public double TotalAnnualValue()
        {
            return (ValuePerOutputUnit * OutputUnitsPerAcre);
        }
    }
}
