using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.Interfaces
{
    public interface ICropResult: ICropLocation, IAgriculturalFloodEvent
    {
        double Damage { get; set; }
        CropDamageCaseEnum DamageCase { get; set; }
    }
}
