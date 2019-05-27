using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.Interfaces
{
    public interface ICropSchedule:IRelateCrops
    {
        int StartPlantDayOfYear { get; }//possibly a distribution
        int LastPlantDayOfYear { get;}//if a distribution for start plant, is this necessary? what about late planting issues?
        int NumberOfDaysToMaturity { get; }//possibly a distribution
        DateTime StartDate { get; set; }
        CropDamageCaseEnum ComputeOutcome(IAgriculturalFloodEvent floodEvent);
    }
}
