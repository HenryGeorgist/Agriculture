using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModelBase.Events;

namespace Agriculture.ViewModel.Hydraulics
{
    public class HydraulicEventEditor : ViewModelBase.BaseViewModel, ViewModelBase.Interfaces.ICanClose
    {
        public event RequestCloseHandler Close;

        private ViewModelBase.NamedAction _CloseAction;
        private ViewModelBase.NamedAction _OKAction;
        private string _Name;
        private string _ArrivalTimePath;
        private string _DurationPath;
        private double _DryoutPeriodInHours;
        private bool _UseDryoutPeriod;
        private bool _Closed = true;
        private DateTime _StartDate = DateTime.Today;
        public string Name { get { return _Name; } set { _Name = value; NotifyPropertyChanged(); } }
        public string ArrivalTimePath { get { return _ArrivalTimePath; } set { _ArrivalTimePath = value; NotifyPropertyChanged(); } }
        public string DurationPath { get { return _DurationPath; } set { _DurationPath = value; NotifyPropertyChanged(); } }
        public DateTime StartDate { get { return _StartDate; } set { _StartDate = value; NotifyPropertyChanged(); } }
        public double DryOutPeriodInHours { get { return _DryoutPeriodInHours; } set { _DryoutPeriodInHours = value; NotifyPropertyChanged(); } }
        public bool UseDryoutPeriod { get { return _UseDryoutPeriod; } set { _UseDryoutPeriod = value;  NotifyPropertyChanged(); } }
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
        public HydraulicEventEditor()
        {
            CloseAction = new ViewModelBase.NamedAction();
            CloseAction.Name = "Close";
            CloseAction.Action = (ob, ev) => { _Closed = true; Close?.Invoke(this, new RequestCloseEventArgs()); };

            OKAction = new ViewModelBase.NamedAction();
            OKAction.Name = "OK";
            OKAction.Action = (ob, ev) => { _Closed = false; Close?.Invoke(this, new RequestCloseEventArgs()); };

            AddSinglePropertyRule(nameof(Name), new ViewModelBase.Rule(() => { return Name != ""; }, "Name cannot be blank"));
            AddSinglePropertyRule(nameof(DurationPath), new ViewModelBase.Rule(() => { return DurationPath != ""; }, "Duration grid path cannot be blank"));
            AddSinglePropertyRule(nameof(ArrivalTimePath), new ViewModelBase.Rule(() => { return ArrivalTimePath != ""; }, "Arrival time grid path cannot be blank"));
            AddSinglePropertyRule(nameof(DryOutPeriodInHours), new ViewModelBase.Rule(DryoutRule, "Dryout period cannot be less than or equal to zero"));

            Name = "";
            ArrivalTimePath = "";
            DurationPath = "";
            ModelBase.Logging.Logger.Instance.LogMessage(new ModelBase.Logging.ErrorMessage("Initalizing Hydraulic Event from VM", ModelBase.Logging.ErrorMessageEnum.Info));
        }

        private bool DryoutRule()
        {
            if (UseDryoutPeriod)
            {
                return DryOutPeriodInHours <= 0.0d;
            }else
            {
                return false;
            }
        }

        public void RequestClose(object sender, RequestCloseEventArgs e)
        {
            Close?.Invoke(sender, e);
        }
    }
}
