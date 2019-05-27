using System;
using System.Collections.Generic;

namespace Agriculture.ViewModel.Ag
{
    public class AgricultureInventory : ViewModelBase.HierarchicalViewModel, ViewModelBase.Interfaces.INavigate
    {
        private string _OutputDirectory;
        private List<Crops.BaseCrop> _Crops;
        public string AgricultureGrid
        {
            get { return _OutputDirectory + "\\" + Name + ".tif"; }
        }
        public List<Agriculture.Crops.BaseCrop> Crops
        {
            get { return _Crops; }
            set { _Crops = value; NotifyPropertyChanged(); }
        }
        public AgricultureInventory(string outputDirectory, string name, List<Crops.BaseCrop> userSelectedCrops)
        {
            _OutputDirectory = outputDirectory;
            Name = name;
            Crops = userSelectedCrops;

            //Add crop budget
            ViewModelBase.NamedAction AddCropBudgetAction = new ViewModelBase.NamedAction();
            AddCropBudgetAction.Name = "Add Crop info";
            AddCropBudgetAction.Action = AddCropBudget;
            Actions.Add(AddCropBudgetAction);
            //remove crop budget
            ViewModelBase.NamedAction RemoveCropBudgetAction = new ViewModelBase.NamedAction();
            RemoveCropBudgetAction.Name = "Remove Crop info";
            RemoveCropBudgetAction.Action = RemoveCropBudget;
            Actions.Add(RemoveCropBudgetAction);

            //edit crop budget
            ViewModelBase.NamedAction EditCropBudgetsAction = new ViewModelBase.NamedAction();
            EditCropBudgetsAction.Name = "Edit Crop info";
            EditCropBudgetsAction.Action = EditCropBudgets;
            Actions.Add(EditCropBudgetsAction);

            //delete inventory
            ViewModelBase.NamedAction DeleteInventoryAction = new ViewModelBase.NamedAction();
            DeleteInventoryAction.Name = "Delete Inventory";
            DeleteInventoryAction.Action = DeleteInventory;
            Actions.Add(DeleteInventoryAction);

            //edit AgGridFile...
        }
        private void AddCropBudget(object arg1, EventArgs arg2)
        {
            //throw new NotImplementedException();
            List<ViewModelBase.BaseViewModel> vms = new List<ViewModelBase.BaseViewModel>();
            List<string> titles = new List<string>();
            titles.Add("VM1");
            titles.Add("VM2");
            titles.Add("VM3");

            NassImporter importer = new NassImporter("ASDF", titles.ToArray());
            Hydraulics.HydraulicEventEditor hee = new Hydraulics.HydraulicEventEditor();
            Simulation.SimulationEditor se = new Simulation.SimulationEditor(null, null);
            vms.Add(importer);
            vms.Add(hee);
            vms.Add(se);

            Navigate(this, new ViewModelBase.Events.RequestNavigationEventArgs(vms, titles, "This is a test"));

        }
        private void RemoveCropBudget(object arg1, EventArgs arg2)
        {
            throw new NotImplementedException();
        }
        private void EditCropBudgets(object arg1, EventArgs arg2)
        {
            List<CropEditor> editors = new List<CropEditor>();
            List<CropEditor> subs = new List<CropEditor>();
            foreach(Agriculture.Crops.BaseCrop b in Crops)
            {
                if(Array.Exists(b.GetType().GetInterfaces(),i => i == typeof(Interfaces.ISubstitutableCrop))){
                    subs.Add(new CropEditor(b,subs));
                }
                editors.Add(new CropEditor(b,subs));
            }
            Navigate(this, new ViewModelBase.Events.RequestNavigationEventArgs(new CropsEditor(editors),ViewModelBase.Enumerations.NavigationEnum.NewScalableDialog,"Crop Editor"));
        }
        private void DeleteInventory(object arg1, EventArgs arg2)
        {
            //system.io.
            System.IO.File.Delete(AgricultureGrid);
            Parent.RemoveChild(this);
            //Parent.Children.Remove(this);//does this remove all handlers?
        }
    }
}
