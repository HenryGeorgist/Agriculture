using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.Interfaces
{
    public interface ICrop : IRelateCrops
    {
        ICropSchedule Schedule { get; }
        double ValuePerOutputUnit { get; set; }//possibly a distribution
        double OutputUnitsPerAcre { get; set; }//possibly a distribution
        double TotalAnnualValue();
        ICropDamageFunction DamageFunction { get; }
        ICropProductionFunction ProductionFunction { get; }
        ICropResult ComputeDamages(IAgriculturalFloodEvent floodEvent);

    }
}
