using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.ViewModel
{
    public static class ExtensionMethods
    {
        public static System.Collections.ObjectModel.ObservableCollection<object> ToItems(this Agriculture.Interfaces.ICropProductionFunction productionFunction)
        {
            System.Collections.ObjectModel.ObservableCollection<object> output = new System.Collections.ObjectModel.ObservableCollection<object>();
            Ag.ProductionFunctionRowItem monthlyVariableCosts = new Ag.ProductionFunctionRowItem(output);//need late plant variable costs.
            monthlyVariableCosts.January = productionFunction.MonthlyVariableCosts[0];
            monthlyVariableCosts.February = productionFunction.MonthlyVariableCosts[1];
            monthlyVariableCosts.March = productionFunction.MonthlyVariableCosts[2];
            monthlyVariableCosts.April = productionFunction.MonthlyVariableCosts[3];
            monthlyVariableCosts.May = productionFunction.MonthlyVariableCosts[4];
            monthlyVariableCosts.June = productionFunction.MonthlyVariableCosts[5];
            monthlyVariableCosts.July = productionFunction.MonthlyVariableCosts[6];
            monthlyVariableCosts.August = productionFunction.MonthlyVariableCosts[7];
            monthlyVariableCosts.September = productionFunction.MonthlyVariableCosts[8];
            monthlyVariableCosts.October = productionFunction.MonthlyVariableCosts[9];
            monthlyVariableCosts.November = productionFunction.MonthlyVariableCosts[10];
            monthlyVariableCosts.December = productionFunction.MonthlyVariableCosts[11];
            output.Add(monthlyVariableCosts);
            return output;
        }
        public static System.Collections.ObjectModel.ObservableCollection<object> ToItems(this Agriculture.Interfaces.IDurationDamageFunction damageFunction)
        {
            System.Collections.ObjectModel.ObservableCollection<object> output = new System.Collections.ObjectModel.ObservableCollection<object>();
            Ag.DurationDamageRowItem durationRow = new Ag.DurationDamageRowItem(output);//need late plant variable costs.
            foreach(KeyValuePair<double,List<double>> pair in damageFunction.DurationDamageFunction)
            {
                durationRow.DurationInDays = pair.Key;
                durationRow.January = pair.Value[0];
                durationRow.February = pair.Value[1];
                durationRow.March = pair.Value[2];
                durationRow.April = pair.Value[3];
                durationRow.May = pair.Value[4];
                durationRow.June = pair.Value[5];
                durationRow.July = pair.Value[6];
                durationRow.August = pair.Value[7];
                durationRow.September = pair.Value[8];
                durationRow.October = pair.Value[9];
                durationRow.November = pair.Value[10];
                durationRow.December = pair.Value[11];
                output.Add(durationRow);
            }

            return output;
        }
    }
}
