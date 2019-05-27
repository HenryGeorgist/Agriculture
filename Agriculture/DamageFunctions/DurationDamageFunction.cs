using System;
using System.Collections.Generic;
using System.Linq;
using Agriculture.Interfaces;

namespace Agriculture.DamageFunctions
{
    public class DurationDamageFunction : Interfaces.IDurationDamageFunction
    {
        private Dictionary<double, List<double>> _DurationDamageFunction;
        public virtual int CropID { get; set; }

        public virtual string CropName { get; set; }
        public Dictionary<double, List<double>> DamageFunction
        {
            get { return _DurationDamageFunction; }
            set { _DurationDamageFunction = value; } //check the keys that they are sorted. check all lists are of lenght 12
        }

        Dictionary<double, List<double>> IDurationDamageFunction.DurationDamageFunction
        {
            get
            {
                return _DurationDamageFunction;
            }
        }

        public DurationDamageFunction(Dictionary<double,List<double>> damFunctions)
        {
            DamageFunction = damFunctions;
        }
        public double ComputeDamagePercent(IAgriculturalFloodEvent floodEvent)
        {
            var durations = _DurationDamageFunction.Keys;
            double[] keys = durations.ToArray();
            int bisearch = Array.BinarySearch(keys, floodEvent.DurationInDecimalHours);
            if (bisearch < 0)
            {
                bisearch = ~bisearch;
                double factor = 0;
                if (bisearch == 0)
                {
                    factor = floodEvent.DurationInDecimalHours / keys[0];
                    return (factor * _DurationDamageFunction[keys[bisearch]][floodEvent.StartDate.Month-1])/100;
                }
                else if(bisearch == keys.Length)
                {
                    return (_DurationDamageFunction[keys.Last()][floodEvent.StartDate.Month - 1])/100;
                }
                else
                {
                    //interpolate
                    factor = (floodEvent.DurationInDecimalHours - keys[bisearch-1])/(keys[bisearch] - keys[bisearch - 1] );
                    double prevDamage = _DurationDamageFunction[keys[bisearch-1]][floodEvent.StartDate.Month - 1];
                    double futureDamage = _DurationDamageFunction[keys[bisearch]][floodEvent.StartDate.Month - 1];
                    return (prevDamage + (futureDamage - prevDamage) * factor)/100;
                }
            }
            else
            {
                //dont interpolate.
                return _DurationDamageFunction[floodEvent.DurationInDecimalHours][floodEvent.StartDate.Month - 1]/100;
            }
        }
        //private double ComputeDamagePercentForListMonthAndDay(List<double> list,int year, int month, int day)
        //{
        //    double sum = 0;
        //    for (int i = 0; i < month-1; i++)//month is not zero based.
        //    {
        //        sum += list[i];
        //    }
        //    int daysInMonth = DateTime.DaysInMonth(year, month);
        //    double factor = day / daysInMonth;//ensure not rounding
        //    return sum + list[month - 1] * factor;
        //}
    }
}
