using Agriculture.Interfaces;

namespace Agriculture.Crops
{
    class SubstitutableCrop: BaseCrop, ISubstitutableCrop
    {
        private ICrop _SubstituteCrop;
        public ICrop SubstituteCrop
        {
            get
            {
                return _SubstituteCrop;
            }
        }
        public SubstitutableCrop(string name, int id,double pricePerUnit, double outputPerAcre, ICropSchedule schedule, ICropProductionFunction productionFunction, ICropDamageFunction damageFunction, ICrop substituteCrop) : base(name, id, pricePerUnit, outputPerAcre, schedule, productionFunction, damageFunction)
        {
            _SubstituteCrop = substituteCrop;
        }
        //protected override void ComputeFloodedDollarDamages(IAgriculturalFloodEvent floodEvent, ref IAgricultureComputeResult result)
        //{
        //    base.ComputeFloodedDollarDamages(floodEvent, ref result);
        //    result.DamageCase = result.DamageCase | CropDamageCaseEnum.SubstituteCrop;
        //}
        //protected override void ComputeNotFloodedDollarDamages(IAgriculturalFloodEvent floodEvent, ref IAgricultureComputeResult result)
        //{
        //    base.ComputeNotFloodedDollarDamages(floodEvent, ref result);
        //    result.DamageCase = result.DamageCase | CropDamageCaseEnum.SubstituteCrop;
        //}
        //protected override void ComputePlantingDelayedDamages(IAgriculturalFloodEvent floodEvent, ref IAgricultureComputeResult result)
        //{
        //    base.ComputePlantingDelayedDamages(floodEvent, ref result);
        //    result.DamageCase = result.DamageCase | CropDamageCaseEnum.SubstituteCrop;
        //}
        protected override void ComputeNotPlantedDollarDamages(IAgriculturalFloodEvent floodEvent, ref ICropResult result)
        {
            double totalValue = TotalAnnualValue();
            double totalValueLessHarvest = totalValue - ProductionFunction.HarvestCost;
            ICropResult subResult = SubstituteCrop.ComputeDamages(floodEvent);
            double newDamage =(totalValueLessHarvest) - ((SubstituteCrop.TotalAnnualValue() - SubstituteCrop.ProductionFunction.HarvestCost) - subResult.Damage);
            subResult.Damage = newDamage;
            result = subResult;
            result.DamageCase = result.DamageCase | CropDamageCaseEnum.SubstituteCrop;
            //result.Damage =  newDamage;
        }
    }
}
