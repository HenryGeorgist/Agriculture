using Agriculture.Interfaces;

namespace Agriculture.Crops
{
    public class CropWithFixedCost : BaseCrop
    {
        public CropWithFixedCost(string name, int id, double pricePerUnit, double outputPerAcre, ICropSchedule schedule, Agriculture.ProductionFunctions.AgProductionFunctionWithFixedCosts productionFunction, ICropDamageFunction damageFunction) : base(name, id, pricePerUnit, outputPerAcre, schedule, productionFunction, damageFunction)
        {
        }
        protected override void ComputeNotPlantedDollarDamages(IAgriculturalFloodEvent floodEvent, ref ICropResult result)
        {
            Agriculture.ProductionFunctions.AgProductionFunctionWithFixedCosts pf = ProductionFunction as ProductionFunctions.AgProductionFunctionWithFixedCosts;
            result.Damage = pf.MonthlyFixedCosts*12;
        }
    }
}
