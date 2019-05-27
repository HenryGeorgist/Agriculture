
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.ViewModel.Hydraulics
{
    public class HydraulicConfiguration: ViewModelBase.HierarchicalViewModel
    {
        private string _ArrivalTimePath;
        private string _DurationPath;
        private double _DryoutPeriodInHours;
        private bool _UseDryoutPeriod;
        private DateTime _StartDate = DateTime.Today;
        public string ArrivalTimePath { get { return _ArrivalTimePath; } set { _ArrivalTimePath = value; NotifyPropertyChanged(); } }
        public string DurationPath { get { return _DurationPath; } set { _DurationPath = value; NotifyPropertyChanged(); } }
        public DateTime StartDate { get { return _StartDate; } set { _StartDate = value; NotifyPropertyChanged(); } }
        public double DryOutPeriodInHours { get { return _DryoutPeriodInHours; } set { _DryoutPeriodInHours = value; NotifyPropertyChanged(); } }
        public bool UseDryoutPeriod { get { return _UseDryoutPeriod; } set { _UseDryoutPeriod = value; NotifyPropertyChanged(); } }
        public HydraulicConfiguration(HydraulicEventEditor h)
        {
            ArrivalTimePath = h.ArrivalTimePath;
            Name = h.Name;
            StartDate = h.StartDate;
            DurationPath = h.DurationPath;
            UseDryoutPeriod = h.UseDryoutPeriod;
            DryOutPeriodInHours = h.DryOutPeriodInHours;

            ViewModelBase.NamedAction Edit = new ViewModelBase.NamedAction();
            Edit.Name = "Edit Event";
            Edit.Action = EditAction;
            Actions.Add(Edit);

            ViewModelBase.NamedAction Delete = new ViewModelBase.NamedAction();
            Delete.Name = "Delete Event";
            Delete.Action = DeleteAction;
            Actions.Add(Delete);
        }

        private void DeleteAction(object arg1, EventArgs arg2)
        {
            Parent.RemoveChild(this);
        }

        private void EditAction(object arg1, EventArgs arg2)
        {
            HydraulicEventEditor h = new HydraulicEventEditor();
            h.Name = Name;
            h.ArrivalTimePath = ArrivalTimePath;
            h.DurationPath = DurationPath;
            h.StartDate = StartDate;
            h.UseDryoutPeriod = UseDryoutPeriod;
            h.DryOutPeriodInHours = DryOutPeriodInHours;
            Navigate(this, new ViewModelBase.Events.RequestNavigationEventArgs(h, ViewModelBase.Enumerations.NavigationEnum.NewScalableDialog, "Hydraulic Event"));
            if (!h.WasClosed)
            {
                ArrivalTimePath = h.ArrivalTimePath;
                Name = h.Name;
                StartDate = h.StartDate;
                DurationPath = h.DurationPath;
                UseDryoutPeriod = h.UseDryoutPeriod;
                DryOutPeriodInHours = h.DryOutPeriodInHours;
            }
        }
    }
}
