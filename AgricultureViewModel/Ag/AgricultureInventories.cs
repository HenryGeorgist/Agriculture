using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.ViewModel.Ag
{
    public class AgricultureInventories: ViewModelBase.HierarchicalViewModel
    {
        public AgricultureInventories()
        {
            Name = "Agriculture Inventories";
            ViewModelBase.NamedAction ImportAction = new ViewModelBase.NamedAction();
            ImportAction.Action = Import;
            ImportAction.Name = "Import from NASS";
            Actions = new List<ViewModelBase.NamedAction>();
            Actions.Add(ImportAction);
        }
        private void Import(object arg1, EventArgs arg2)
        {
            List<ViewModelBase.BaseViewModel> vms = new List<ViewModelBase.BaseViewModel>();
            List<string> titles = new List<string>();
            titles.Add(@"C: \Users\Q0HECWPL\Documents\WAT\stpaul\StPaul_Deterministic\fia\maps\StPaulStudyArea.shp");


            NassImporter importers = new NassImporter(@"C: \Users\Q0HECWPL\Documents\WAT\stpaul\StPaul_Deterministic\fia\maps\", titles.ToArray());
            //Hydraulics.HydraulicEventEditor hee = new Hydraulics.HydraulicEventEditor();
            //Simulation.SimulationEditor se = new Simulation.SimulationEditor(null, null);
            //vms.Add(importers);
            //vms.Add(hee);
            //vms.Add(se);

            Navigate(this, new ViewModelBase.Events.RequestNavigationEventArgs(importers,ViewModelBase.Enumerations.NavigationEnum.NewScalableDialog,"Import"));

            //NassImporter importer = new NassImporter(@"C:\Temp\Nass\Data",new string[] { @"C:\Temp\NASS\NWK_LP_Data\LPT_SA_reproj.shp", @"C:\Temp\NASS\SLR_Data\Maps\SLR_StudyArea.shp" });
            //Navigate(this,new ViewModelBase.Events.RequestNavigationEventArgs(importer,ViewModelBase.Enumerations.NavigationEnum.AsNewWindow | ViewModelBase.Enumerations.NavigationEnum.Scalable,"Import Nass Data"));
            ////determine if the window was closed or if the user actually imported an inventory.
            //if (importer.Success)
            //{
            //    List<Crops.BaseCrop> userSelectedCrops = new List<Crops.BaseCrop>();
            //    CropSchedules.SingleYearCropSchedule baseSchedule = new CropSchedules.SingleYearCropSchedule(DateTime.Today, 10, 100);

            //    System.Collections.Generic.List<double> monthlyCosts = new System.Collections.Generic.List<double>() { 0.0d, 10.0d, 10.0d, 10.0d, 10.0d, 10.0d, 10.0d, 2.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            //    Interfaces.ICropProductionFunction cpf = new ProductionFunctions.AgProductionFunction(monthlyCosts, baseSchedule, .1, 10);

            //    Dictionary<double, List<double>> damFun = new Dictionary<double, List<double>>();
            //    List<double> oneDay = new List<double>() { 0.0d, .5d, .75d, .8d, .9d, .1d, .1d, .1d, 0.0d, 0.0d, 0.0d, 0.0d };
            //    List<double> twoDay = new List<double>() { 0.0d, 5.0d, 7.5d, 8.0d, 9.0d, 10.0d, 10.0d, 10.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            //    List<double> threeDay = new List<double>() { 0.0d, 50.0d, 75.0d, 80.0d, 90.0d, 100.0d, 100.0d, 100.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            //    damFun.Add(24.0d, oneDay);
            //    damFun.Add(48.0d, twoDay);
            //    damFun.Add(72.0d, threeDay);
            //    DamageFunctions.DurationDamageFunction baseDamageFunction = new DamageFunctions.DurationDamageFunction(damFun);

            //    foreach (Agriculture.ViewModel.Ag.NassStatistics n in importer.StatisticsResult)
            //    {
            //        if (n.ImportCrop)
            //        {
            //            userSelectedCrops.Add(new Crops.Crop(n.CropName,n.CropID, 0, 0, baseSchedule, cpf, baseDamageFunction));
            //        }
            //    }

            //    AgricultureInventory inv = new AgricultureInventory(@"C:\Temp\Nass\Data", importer.Name, userSelectedCrops);
            //    AddChild(inv);
            //}
            
            

        }
    }
}
