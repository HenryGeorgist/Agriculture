using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.ViewModel.Ag
{
    public class DurationDamageRowItem : Consequences_Assist.DataGridRowItem
    {
        private double _DurationInDays;
        private double _JanDamage;
        private double _FebDamage;
        private double _MarDamage;
        private double _AprDamage;
        private double _MayDamage;
        private double _JunDamage;
        private double _JulDamage;
        private double _AugDamage;
        private double _SeptDamage;
        private double _OctDamage;
        private double _NovDamage;
        private double _DecDamage;
        public double DurationInDays
        {
            get { return _DurationInDays; }
            set { _DurationInDays = value; NotifyPropertyChanged(); }
        }
        public double January
        {
            get { return _JanDamage; }
            set { _JanDamage = value; NotifyPropertyChanged(); }
        }
        public double February
        {
            get { return _FebDamage; }
            set { _FebDamage = value; NotifyPropertyChanged(); }
        }
        public double March
        {
            get { return _MarDamage; }
            set { _MarDamage = value; NotifyPropertyChanged(); }
        }
        public double April
        {
            get { return _AprDamage; }
            set { _AprDamage = value; NotifyPropertyChanged(); }
        }
        public double May
        {
            get { return _MayDamage; }
            set { _MayDamage = value; NotifyPropertyChanged(); }
        }
        public double June
        {
            get { return _JunDamage; }
            set { _JunDamage = value; NotifyPropertyChanged(); }
        }
        public double July
        {
            get { return _JulDamage; }
            set { _JulDamage = value; NotifyPropertyChanged(); }
        }
        public double August
        {
            get { return _AugDamage; }
            set { _AugDamage = value; NotifyPropertyChanged(); }
        }
        public double September
        {
            get { return _SeptDamage; }
            set { _SeptDamage = value; NotifyPropertyChanged(); }
        }
        public double October
        {
            get { return _OctDamage; }
            set { _OctDamage = value; NotifyPropertyChanged(); }
        }
        public double November
        {
            get { return _NovDamage; }
            set { _NovDamage = value; NotifyPropertyChanged(); }
        }
        public double December
        {
            get { return _DecDamage; }
            set { _DecDamage = value; NotifyPropertyChanged(); }
        }
        public DurationDamageRowItem(System.Collections.ObjectModel.ObservableCollection<object> owner):base(owner)
        {
            //AddSinglePropertyRule(nameof(January), new ViewModelBase.Rule(() => LessThan100(January), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(February), new ViewModelBase.Rule(() => LessThan100(February), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(March), new ViewModelBase.Rule(() => LessThan100(March), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(April), new ViewModelBase.Rule(() => LessThan100(April), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(May), new ViewModelBase.Rule(() => LessThan100(May), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(June), new ViewModelBase.Rule(() => LessThan100(June), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(July), new ViewModelBase.Rule(() => LessThan100(July), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(August), new ViewModelBase.Rule(() => LessThan100(August), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(September), new ViewModelBase.Rule(() => LessThan100(September), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(October), new ViewModelBase.Rule(() => LessThan100(October), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(November), new ViewModelBase.Rule(() => LessThan100(November), "Damage must be less than or equal to 100%"));
            //AddSinglePropertyRule(nameof(December), new ViewModelBase.Rule(() => LessThan100(December), "Damage must be less than or equal to 100%"));

            //AddSinglePropertyRule(nameof(January), new ViewModelBase.Rule(() => GreaterThanZero(January), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(February), new ViewModelBase.Rule(() => GreaterThanZero(February), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(March), new ViewModelBase.Rule(() => GreaterThanZero(March), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(April), new ViewModelBase.Rule(() => GreaterThanZero(April), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(May), new ViewModelBase.Rule(() => GreaterThanZero(May), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(June), new ViewModelBase.Rule(() => GreaterThanZero(June), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(July), new ViewModelBase.Rule(() => GreaterThanZero(July), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(August), new ViewModelBase.Rule(() => GreaterThanZero(August), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(September), new ViewModelBase.Rule(() => GreaterThanZero(September), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(October), new ViewModelBase.Rule(() => GreaterThanZero(October), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(November), new ViewModelBase.Rule(() => GreaterThanZero(November), "Damage must be greater than 0%"));
            //AddSinglePropertyRule(nameof(December), new ViewModelBase.Rule(() => GreaterThanZero(December), "Damage must be greater than 0%"));

            //AddSinglePropertyRule(nameof(DurationInDays), new ViewModelBase.Rule(() => GreaterThanZero(DurationInDays), "Damage must be less than or equal to 100%"));
        }
        private bool LessThan100(double value) 
        {
            return (value > 100.0d);
        }
        private bool GreaterThanZero(double value)
        {
            return (value < 0.0d);
        }
        public override void AddValidationRules()
        {
            AddRule(nameof(January), () => LessThan100(January), "Damage must be less than or equal to 100%");
            AddRule(nameof(February), () => LessThan100(February), "Damage must be less than or equal to 100%");
            AddRule(nameof(March), () => LessThan100(March), "Damage must be less than or equal to 100%");
            AddRule(nameof(April), () => LessThan100(April), "Damage must be less than or equal to 100%");
            AddRule(nameof(May), () => LessThan100(May), "Damage must be less than or equal to 100%");
            AddRule(nameof(June), () => LessThan100(June), "Damage must be less than or equal to 100%");
            AddRule(nameof(July), () => LessThan100(July), "Damage must be less than or equal to 100%");
            AddRule(nameof(August), () => LessThan100(August), "Damage must be less than or equal to 100%");
            AddRule(nameof(September), () => LessThan100(September), "Damage must be less than or equal to 100%");
            AddRule(nameof(October), () => LessThan100(October), "Damage must be less than or equal to 100%");
            AddRule(nameof(November), () => LessThan100(November), "Damage must be less than or equal to 100%");
            AddRule(nameof(December), () => LessThan100(December), "Damage must be less than or equal to 100%");

            AddRule(nameof(January), () => GreaterThanZero(January), "Damage must be greater than 0%");
            AddRule(nameof(February), () => GreaterThanZero(February), "Damage must be greater than 0%");
            AddRule(nameof(March), () => GreaterThanZero(March), "Damage must be greater than 0%");
            AddRule(nameof(April), () => GreaterThanZero(April), "Damage must be greater than 0%");
            AddRule(nameof(May), () => GreaterThanZero(May), "Damage must be greater than 0%");
            AddRule(nameof(June), () => GreaterThanZero(June), "Damage must be greater than 0%");
            AddRule(nameof(July), () => GreaterThanZero(July), "Damage must be greater than 0%");
            AddRule(nameof(August), () => GreaterThanZero(August), "Damage must be greater than 0%");
            AddRule(nameof(September), () => GreaterThanZero(September), "Damage must be greater than 0%");
            AddRule(nameof(October), () => GreaterThanZero(October), "Damage must be greater than 0%");
            AddRule(nameof(November), () => GreaterThanZero(November), "Damage must be greater than 0%");
            AddRule(nameof(December), () => GreaterThanZero(December), "Damage must be greater than 0%");

            AddRule(nameof(DurationInDays),() => GreaterThanZero(DurationInDays), "Duration must be greater than 0 days");

        }

        public override string PropertyDisplayName(string propertyName)
        {
            if(propertyName==nameof(DurationInDays)) return "Duration in Days" ;
            return propertyName;
        }

        public override bool IsGridDisplayable(string propertyName)
        {
            return true;
        }
    }
}
