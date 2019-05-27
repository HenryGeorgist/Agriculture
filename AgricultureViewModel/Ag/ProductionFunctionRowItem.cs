using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.ViewModel.Ag
{
    public class ProductionFunctionRowItem : Consequences_Assist.DataGridRowItem
    {
        //private DateTime _SeasonStart;
        //private int _PlantingWindow;
        private double _JanBudget;
        private double _FebBudget;
        private double _MarBudget;
        private double _AprBudget;
        private double _MayBudget;
        private double _JunBudget;
        private double _JulBudget;
        private double _AugBudget;
        private double _SeptBudget;
        private double _OctBudget;
        private double _NovBudget;
        private double _DecBudget;
        //public DateTime SeasonStart
        //{
        //    get { return _SeasonStart; }
        //    set { _SeasonStart = value; NotifyPropertyChanged(); }
        //}
        //public int PlantingWindow
        //{
        //    get { return _PlantingWindow; }
        //    set { _PlantingWindow = value; NotifyPropertyChanged(); }
        //}
        public double January
        {
            get { return _JanBudget; }
            set { _JanBudget = value; NotifyPropertyChanged(); }
        }
        public double February
        {
            get { return _FebBudget; }
            set { _FebBudget = value; NotifyPropertyChanged(); }
        }
        public double March
        {
            get { return _MarBudget; }
            set { _MarBudget = value; NotifyPropertyChanged(); }
        }
        public double April
        {
            get { return _AprBudget; }
            set { _AprBudget = value; NotifyPropertyChanged(); }
        }
        public double May
        {
            get { return _MayBudget; }
            set { _MayBudget = value; NotifyPropertyChanged(); }
        }
        public double June
        {
            get { return _JunBudget; }
            set { _JunBudget = value; NotifyPropertyChanged(); }
        }
        public double July
        {
            get { return _JulBudget; }
            set { _JulBudget = value; NotifyPropertyChanged(); }
        }
        public double August
        {
            get { return _AugBudget; }
            set { _AugBudget = value; NotifyPropertyChanged(); }
        }
        public double September
        {
            get { return _SeptBudget; }
            set { _SeptBudget = value; NotifyPropertyChanged(); }
        }
        public double October
        {
            get { return _OctBudget; }
            set { _OctBudget = value; NotifyPropertyChanged(); }
        }
        public double November
        {
            get { return _NovBudget; }
            set { _NovBudget = value; NotifyPropertyChanged(); }
        }
        public double December
        {
            get { return _DecBudget; }
            set { _DecBudget = value; NotifyPropertyChanged(); }
        }
        public ProductionFunctionRowItem(System.Collections.ObjectModel.ObservableCollection<object> owner): base(owner)
        {
            //AddSinglePropertyRule(nameof(January), new ViewModelBase.Rule(() => GreaterThanZero(January), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(February), new ViewModelBase.Rule(() => GreaterThanZero(February), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(March), new ViewModelBase.Rule(() => GreaterThanZero(March), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(April), new ViewModelBase.Rule(() => GreaterThanZero(April), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(May), new ViewModelBase.Rule(() => GreaterThanZero(May), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(June), new ViewModelBase.Rule(() => GreaterThanZero(June), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(July), new ViewModelBase.Rule(() => GreaterThanZero(July), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(August), new ViewModelBase.Rule(() => GreaterThanZero(August), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(September), new ViewModelBase.Rule(() => GreaterThanZero(September), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(October), new ViewModelBase.Rule(() => GreaterThanZero(October), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(November), new ViewModelBase.Rule(() => GreaterThanZero(November), "Budget must be greater than 0%"));
            //AddSinglePropertyRule(nameof(December), new ViewModelBase.Rule(() => GreaterThanZero(December), "Budget must be greater than 0%"));
        }

        private bool GreaterThanZero(double value)
        {
            return (value < 0.0d);
        }

        public override void AddValidationRules()
        {
            AddRule(nameof(January), () => GreaterThanZero(January), "Budget must be greater than 0%");
            AddRule(nameof(February), () => GreaterThanZero(February), "Budget must be greater than 0%");
            AddRule(nameof(March), () => GreaterThanZero(March), "Budget must be greater than 0%");
            AddRule(nameof(April), () => GreaterThanZero(April), "Budget must be greater than 0%");
            AddRule(nameof(May), () => GreaterThanZero(May), "Budget must be greater than 0%");
            AddRule(nameof(June), () => GreaterThanZero(June), "Budget must be greater than 0%");
            AddRule(nameof(July), () => GreaterThanZero(July), "Budget must be greater than 0%");
            AddRule(nameof(August), () => GreaterThanZero(August), "Budget must be greater than 0%");
            AddRule(nameof(September), () => GreaterThanZero(September), "Budget must be greater than 0%");
            AddRule(nameof(October), () => GreaterThanZero(October), "Budget must be greater than 0%");
            AddRule(nameof(November), () => GreaterThanZero(November), "Budget must be greater than 0%");
            AddRule(nameof(December), () => GreaterThanZero(December), "Budget must be greater than 0%");
        }

        public override string PropertyDisplayName(string propertyName)
        {
            //if (propertyName == nameof(SeasonStart)) return "Season Start";
            //if (propertyName == nameof(PlantingWindow)) return "Planting Window";
            return propertyName;
        }

        public override bool IsGridDisplayable(string propertyName)
        {
            return true;
        }
    }
}
