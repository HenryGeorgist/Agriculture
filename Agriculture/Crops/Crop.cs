using Agriculture.Interfaces;

namespace Agriculture.Crops
{
    public class Crop:BaseCrop
    {
        public Crop(string name, int id, double pricePerUnit, double outputPerAcre, ICropSchedule schedule, ICropProductionFunction productionFunction, ICropDamageFunction damageFunction): base(name,id,pricePerUnit,outputPerAcre,schedule,productionFunction,damageFunction)
        {
        }
    }
}
