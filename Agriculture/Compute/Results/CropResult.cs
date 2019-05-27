using System;
using Agriculture.Interfaces;

namespace Agriculture.Compute.Results
{
    public class CropResult : ICropResult
    {
        public int CropID { get; set; }

        public string CropName { get; set; }

        public double Damage { get; set; }

        public CropDamageCaseEnum DamageCase { get; set; }

        public double DurationInDecimalHours { get; set; }

        public DateTime StartDate { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
    }
}
