using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.ViewModel.Hydraulics
{
    public class HydraulicConfigurations: ViewModelBase.HierarchicalViewModel
    {
        public HydraulicConfigurations()
        {
            Name = "Hydraulics";

            ViewModelBase.NamedAction ImportAction = new ViewModelBase.NamedAction();
            ImportAction.Action = Import;
            ImportAction.Name = "Import from Grids";
            Actions = new List<ViewModelBase.NamedAction>();
            Actions.Add(ImportAction);

        }
        private void Import(object arg1, EventArgs arg2)
        {
            HydraulicEventEditor h = new HydraulicEventEditor();
            
            Navigate(this, new ViewModelBase.Events.RequestNavigationEventArgs(h, ViewModelBase.Enumerations.NavigationEnum.NewScalableDialog, "Hydraulic Event"));
            if (!h.WasClosed)
            {
                HydraulicConfiguration hconfig = new HydraulicConfiguration(h);
                
                AddChild(hconfig);
            }
        }
    }
}
