using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.ViewModel.Simulation
{
    public class Simulation: ViewModelBase.HierarchicalViewModel
    {
        private Ag.AgricultureInventory _Inventory;
        private Hydraulics.HydraulicConfiguration _HydraulicEvent;
        private ViewModelBase.NamedAction _viewResults;
        public Simulation(SimulationEditor sim)
        {
            Name = sim.Name;
            _Inventory = sim.Inventory;
            _HydraulicEvent = sim.Configuration;

            Actions = new List<ViewModelBase.NamedAction>();

            ViewModelBase.NamedAction compute = new ViewModelBase.NamedAction();
            compute.Name = "Compute";
            compute.Action = Compute;
            Actions.Add(compute);

            _viewResults = new ViewModelBase.NamedAction();
            _viewResults.Name = "Results";
            _viewResults.Action = Results;
            _viewResults.IsEnabled = false;
            Actions.Add(_viewResults);
        }

        private void Results(object arg1, EventArgs arg2)
        {
            throw new NotImplementedException();
        }

        private void Compute(object arg1, EventArgs arg2)
        {
            Agriculture.Compute.AgricultureComputeBase compute = new Compute.AgricultureComputeBase();
            IList<Interfaces.ICrop> crops = new List<Interfaces.ICrop>();
            foreach(Interfaces.ICrop c in _Inventory.Crops)
            {
                crops.Add(c);
            }
            compute.Crops = crops;

            Agriculture.Nass.BaseTiffFilter filter = new Nass.BaseTiffFilter();
            Agriculture.Nass.NassGeoTiffReader nassReader = new Nass.NassGeoTiffReader(_Inventory.AgricultureGrid);
            Agriculture.EventProviders.MultiThreadedGriddedHydraulicDataProvider gridReader;
            if (_HydraulicEvent.UseDryoutPeriod)
            {
                gridReader = new EventProviders.MultiThreadedGriddedHydraulicDataProvider(_HydraulicEvent.ArrivalTimePath, _HydraulicEvent.DurationPath, _HydraulicEvent.DryOutPeriodInHours);
            }
            else
            {
                gridReader = new EventProviders.MultiThreadedGriddedHydraulicDataProvider(_HydraulicEvent.ArrivalTimePath, _HydraulicEvent.DurationPath);
            }
            

            Agriculture.Compute.Results.AgComputeResult result = (Agriculture.Compute.Results.AgComputeResult)compute.Compute(nassReader, filter, gridReader, _HydraulicEvent.StartDate);
            result.ToShapeFile(@"C:\Temp\Nass\Output\"+ Name + "_ComputeOutput.shp", nassReader.Projection);

            _viewResults.IsEnabled = true;
        }
    }
}
