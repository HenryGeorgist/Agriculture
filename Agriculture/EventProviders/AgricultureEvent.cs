using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.EventProviders
{
    public class AgricultureEvent : Agriculture.Interfaces.IAgriculturalFloodEvent
    {
        private double _Duration;
        private DateTime _StartDate;
        public int CropID { get; set; }
        public string CropName { get; set; }
        public double DurationInDecimalHours
        {
            get
            {
                return _Duration;
            }

            set
            {
                _Duration = value;
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
        public double X { get; set; }
        public double Y { get; set; }
    }
}
