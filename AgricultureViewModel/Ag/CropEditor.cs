using System;
using System.Collections.Generic;
using ViewModelBase;
using ViewModelBase.Events;
using ViewModelBase.Interfaces;

namespace Agriculture.ViewModel.Ag
{
    public class CropEditor : ViewModelBase.BaseViewModel, INavigate
    {
        public event RequestNavigationHandler RequestNavigation;
        public event RequestCloseHandler Close;

        private ViewModelBase.NamedAction _PlotBudget;
        private ViewModelBase.NamedAction _PlotDamageFunctions;
        private NamedAction _AddDurationAction;
        private NamedAction _RemoveDurationAction;
        private Agriculture.Interfaces.ICrop _Crop;
        private bool _IsSubstitutableCrop;
        private bool _IncludeFixedCosts;
        private System.Collections.ObjectModel.ObservableCollection<object> _ProductionFunctions;
        private System.Collections.ObjectModel.ObservableCollection<object> _DurationDamageFunction;
        private List<CropEditor> _SubstituteCrops;
        private CropEditor _SubstituteCrop;
        public string CropName
        {
            get { return _Crop.CropName; }
            set { _Crop.CropName = value; NotifyPropertyChanged(); }
        }
        public DateTime StartPlant
        {
            get { return _Crop.Schedule.StartDate; }
            set { _Crop.Schedule.StartDate = value;  NotifyPropertyChanged(); } //need to update production function since schedule is a dependency injection.
        }
        public int PlantingWindow
        {
            get { return _Crop.Schedule.LastPlantDayOfYear - _Crop.Schedule.StartPlantDayOfYear; }//needs refactoring
            set { }
        }
        public double FixedCosts
        {
            get { return _Crop.ProductionFunction.MonthlyFixedCosts; }
            set { _Crop.ProductionFunction.MonthlyFixedCosts = value; NotifyPropertyChanged(); }
        }
        public double PricePerAcre
        {
            get { return _Crop.ValuePerOutputUnit * _Crop.OutputUnitsPerAcre; }
        }
        public double PricePerUnit
        {
            get { return _Crop.ValuePerOutputUnit; }
            set { _Crop.ValuePerOutputUnit = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(PricePerAcre)); }
        }
        public double UnitsPerAcre
        {
            get { return _Crop.OutputUnitsPerAcre; }
            set { _Crop.OutputUnitsPerAcre = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(PricePerAcre)); }
        }
        public double HarvestCost
        {
            get { return _Crop.ProductionFunction.HarvestCost; }
            set { _Crop.ProductionFunction.HarvestCost = value; NotifyPropertyChanged(); }
        }
        public double LatePlantLoss
        {
            get { return _Crop.ProductionFunction.OutputLossDueToLatePlant; }
            set { _Crop.ProductionFunction.OutputLossDueToLatePlant = value;  NotifyPropertyChanged(); }
        }
        public bool IsSubstitutableCrop
        {
            get { return _IsSubstitutableCrop; }
            set { _IsSubstitutableCrop = value; NotifyPropertyChanged(); }
        }
        public bool IncludeFixedCosts
        {
            get { return _IncludeFixedCosts; }
            set { _IncludeFixedCosts = value; NotifyPropertyChanged(); }
        }
        public System.Collections.ObjectModel.ObservableCollection<object> ProductionFunctions
        {
            get { return _ProductionFunctions; }
            set { _ProductionFunctions = value; NotifyPropertyChanged(); }
        }
        public System.Collections.ObjectModel.ObservableCollection<object> DurationDamageFunctions
        {
            get { return _DurationDamageFunction; }
            set { _DurationDamageFunction = value; NotifyPropertyChanged(); }
        }
        public List<CropEditor> SubstituteCrops
        {
            get { return _SubstituteCrops; }
            set { _SubstituteCrops = value; NotifyPropertyChanged(); }
        }
        public CropEditor SubstituteCrop
        {
            get { return _SubstituteCrop; }
            set { _SubstituteCrop = value; NotifyPropertyChanged(); }
        }
        public ViewModelBase.NamedAction PlotBudget
        {
            get { return _PlotBudget; }
            set { _PlotBudget = value; NotifyPropertyChanged(); }
        }
        public ViewModelBase.NamedAction PlotDamageFunctions
        {
            get { return _PlotDamageFunctions; }
            set { _PlotDamageFunctions = value; NotifyPropertyChanged(); }
        }
        public ViewModelBase.NamedAction AddDurationAction
        {
            get { return _AddDurationAction; }
            set { _AddDurationAction = value; NotifyPropertyChanged(); }
        }
        public ViewModelBase.NamedAction RemoveDurationAction
        {
            get { return _RemoveDurationAction; }
            set { _RemoveDurationAction = value; NotifyPropertyChanged(); }
        }
        public CropEditor(List<CropEditor> substitutes)
        {
            InitializeNamedActions();
            Agriculture.CropSchedules.SingleYearCropSchedule cornSchedule = new Agriculture.CropSchedules.SingleYearCropSchedule(new DateTime(2014, 4, 14), 52, 220);

            System.Collections.Generic.List<double> monthlyCosts = new System.Collections.Generic.List<double>() { 0.0d, 0.0d, 10.0d, 73.0d, 260.0d, 41.0d, 21.0d, 14.0d, 27.0d, 10.0d, 0.0d, 0.0d };
            Agriculture.Interfaces.ICropProductionFunction cpf = new Agriculture.ProductionFunctions.AgProductionFunction(monthlyCosts, cornSchedule, .3, 2.5);

            Dictionary<double, List<double>> damFun = new Dictionary<double, List<double>>();
            List<double> zeroDay = new List<double>() { 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            List<double> threeDay = new List<double>() { 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 100.0, 99.2, 75.0, 0.0 };
            List<double> sevenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            List<double> fourteenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            damFun.Add(0.0d, zeroDay);
            damFun.Add(72.0d, threeDay);
            damFun.Add(168.0d, sevenDay);
            damFun.Add(336.0d, fourteenDay);
            Agriculture.DamageFunctions.DurationDamageFunction CornDamageFunction = new Agriculture.DamageFunctions.DurationDamageFunction(damFun);
            _Crop = new Agriculture.Crops.Crop("Corn", 1, 5.2, 194, cornSchedule, cpf, CornDamageFunction);
            FromDurationDamageFunctionToDurationDamageRowItems();
            FromProductionFunctionToProductionFunctionRowItems();
            SubstituteCrops = substitutes;
        }
        public CropEditor(Interfaces.ICrop crop, List<CropEditor> substitutes)
        {
            InitializeNamedActions();
            _Crop = crop;
            FromDurationDamageFunctionToDurationDamageRowItems();
            FromProductionFunctionToProductionFunctionRowItems();
            SubstituteCrops = substitutes;
        }
        private void InitializeNamedActions()
        {
            AddDurationAction = new NamedAction();
            AddDurationAction.Name = "Add Duration";
            AddDurationAction.Action = AddDuration;

            RemoveDurationAction = new NamedAction();
            RemoveDurationAction.Name = "Remove Duration";
            RemoveDurationAction.Action = RemoveDuration;

            PlotDamageFunctions = new NamedAction();
            PlotDamageFunctions.Name = "Plot Damage Functions";
            PlotDamageFunctions.Action = PlotDamageFunctionsAction;

            PlotBudget = new NamedAction();
            PlotBudget.Name = "Plot Production Functions";
            PlotBudget.Action = PlotBudgetFunction;
        }

        private void PlotBudgetFunction(object arg1, EventArgs arg2)
        {
            throw new NotImplementedException();
           
        }

        private void PlotDamageFunctionsAction(object arg1, EventArgs arg2)
        {
            throw new NotImplementedException();
        }

        private void AddDuration(object arg1, EventArgs arg2)
        {
            DurationDamageFunctions.Add(new DurationDamageRowItem(DurationDamageFunctions));
        }
        private void RemoveDuration(object arg1, EventArgs arg2)
        {
            DurationDamageFunctions.RemoveAt(DurationDamageFunctions.Count - 1);//would need to add a selected durationDamageFunction and bind to remove based on selected row.
        }
        private void FromDurationDamageFunctionToDurationDamageRowItems()
        {
            Interfaces.IDurationDamageFunction d = _Crop.DamageFunction as Interfaces.IDurationDamageFunction;
            if (d != null)
            {
                DurationDamageFunctions = d.ToItems();
                DurationDamageFunctions.CollectionChanged += DurationDamageFunctions_CollectionChanged;
            }
        }
        private void DurationDamageFunctions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //capture changes and update _Crop.DamageFunction
        }
        private void FromProductionFunctionToProductionFunctionRowItems()
        {
            ProductionFunctions = _Crop.ProductionFunction.ToItems();
            ProductionFunctions.CollectionChanged += ProductionFunctions_CollectionChanged;
        }
        private void ProductionFunctions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //capture changes and update _Crop.ProductionFunction
        }
        public void Navigate(object sender, RequestNavigationEventArgs e)
        {
            RequestNavigation?.Invoke(sender, e);
        }
        public void ApplyChanges()
        {
            //do stuff;
        }
    }
}
