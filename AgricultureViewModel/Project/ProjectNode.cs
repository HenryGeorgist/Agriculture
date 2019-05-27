namespace Agriculture.ViewModel.Project
{
    public class ProjectNode: ViewModelBase.HierarchicalViewModel
    {
        public ProjectNode()
        {
            Name = "Project";
            Expanded = true;
            Ag.AgricultureInventories node = new Ag.AgricultureInventories();
            AddChild(node);

            Hydraulics.HydraulicConfigurations hnode = new Hydraulics.HydraulicConfigurations();
            AddChild(hnode);

            Simulation.Simulations snode = new Simulation.Simulations();
            AddChild(snode);
            ModelBase.Logging.Logger.Instance.LogMessage(new ModelBase.Logging.ErrorMessage("Initalizing Project Node from VM", ModelBase.Logging.ErrorMessageEnum.Info));
        }
    }
}
