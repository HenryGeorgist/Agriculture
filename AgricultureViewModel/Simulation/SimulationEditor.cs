using System;
using System.Collections.Generic;
using ViewModelBase;
using ViewModelBase.Events;

namespace Agriculture.ViewModel.Simulation
{
    public class SimulationEditor : ViewModelBase.BaseViewModel, ViewModelBase.Interfaces.ICanClose
    {
        public event RequestCloseHandler Close;

        private ViewModelBase.NamedAction _CloseAction;
        private ViewModelBase.NamedAction _OKAction;
        private bool _Closed = true;
        private string _Name;
        private Ag.AgricultureInventory _Inventory;
        private List<Ag.AgricultureInventory> _Inventories;
        private Hydraulics.HydraulicConfiguration _Configuration;
        private List<Hydraulics.HydraulicConfiguration> _Configurations;
        public string Name
        {
            get { return _Name; }
            set { _Name = value;  NotifyPropertyChanged(); }
        }
        public ViewModelBase.NamedAction CloseAction
        {
            get { return _CloseAction; }
            set { _CloseAction = value; NotifyPropertyChanged(); }
        }
        public ViewModelBase.NamedAction OKAction
        {
            get { return _OKAction; }
            set { _OKAction = value; NotifyPropertyChanged(); }
        }
        public bool WasClosed { get { return _Closed; } }
        public List<Ag.AgricultureInventory> Inventories
        {
            get { return _Inventories; }
            set { _Inventories = value; NotifyPropertyChanged(); }
        }
        public List<Hydraulics.HydraulicConfiguration> Configurations
        {
            get { return _Configurations; }
            set { _Configurations = value; NotifyPropertyChanged(); }
        }
        public Ag.AgricultureInventory Inventory
        {
            get { return _Inventory; }
            set { _Inventory = value; NotifyPropertyChanged(); }
        }
        public Hydraulics.HydraulicConfiguration Configuration
        {
            get { return _Configuration; }
            set { _Configuration = value; NotifyPropertyChanged(); }
        }
        public SimulationEditor(List<Hydraulics.HydraulicConfiguration> configs, List<Ag.AgricultureInventory> invs)
        {
            Configurations = configs;

            Inventories = invs;

            CloseAction = new ViewModelBase.NamedAction();
            CloseAction.Name = "Close";
            CloseAction.Action = (ob, ev) => {Close?.Invoke(this, new RequestCloseEventArgs()); };

            OKAction = new ViewModelBase.NamedAction();
            OKAction.Name = "OK";
            OKAction.Action = (ob, ev) => { _Closed = false; Close?.Invoke(this, new RequestCloseEventArgs()); };

            AddSinglePropertyRule(nameof(Name), new ViewModelBase.Rule(() => { return Name != ""; }, "Name cannot be blank"));

            Name = "";
        }
        public void RequestClose(object sender, RequestCloseEventArgs e)
        {
            Close?.Invoke(this,new RequestCloseEventArgs());
        }
    }
}
