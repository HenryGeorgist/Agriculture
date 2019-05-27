using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.ViewModel.Simulation
{
    public class Simulations: ViewModelBase.HierarchicalViewModel
    {
        public Simulations()
        {
            Name = "Simulations";

            //add new simulation.
            Actions = new List<ViewModelBase.NamedAction>();

            ViewModelBase.NamedAction addSimulation = new ViewModelBase.NamedAction();
            addSimulation.Name = "Create New";
            addSimulation.Action = AddSimulation;
            Actions.Add(addSimulation);

            ViewModelBase.NamedAction addLog = new ViewModelBase.NamedAction();
            addLog.Name = "Log";
            addLog.Action = AddLog;
            Actions.Add(addLog);

        }

        private void AddLog(object arg1, EventArgs arg2)
        {
            ModelBase.Logging.Logger.Instance.LogMessage(new ModelBase.Logging.ErrorMessage("Logging from Simulations VM", ModelBase.Logging.ErrorMessageEnum.Info));
            ModelBase.Logging.Logger.Instance.Flush();
        }

        private void AddSimulation(object arg1, EventArgs arg2)
        {
            SimulationEditor s = new SimulationEditor(GetRelativesOfType<Hydraulics.HydraulicConfiguration>(),GetRelativesOfType<Ag.AgricultureInventory>());

            Navigate(this, new ViewModelBase.Events.RequestNavigationEventArgs(s, ViewModelBase.Enumerations.NavigationEnum.NewScalableDialog, "AgricultureSimulation"));
            if (!s.WasClosed)
            {
                Simulation sim = new Simulation(s);

                AddChild(sim);
            }
        }
    }
}
