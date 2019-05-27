using System;
using System.Collections.Generic;
using ViewModelBase.Events;

namespace Agriculture.ViewModel.Ag
{
    public class CropsEditor : ViewModelBase.BaseViewModel, ViewModelBase.Interfaces.ICanClose, ViewModelBase.Interfaces.INavigate
    {
        private List<CropEditor> _Crops;
        private CropEditor _SelectedCrop;
        private ViewModelBase.NamedAction _ApplyAction;
        private ViewModelBase.NamedAction _CloseAction;

        public event RequestNavigationHandler RequestNavigation;
        public event RequestCloseHandler Close;

        public List<CropEditor> Crops
        {
            get { return _Crops; }
            set { _Crops = value; NotifyPropertyChanged(); }
        }
        public CropEditor SelectedCrop
        {
            get { return _SelectedCrop; }
            set {
                if (_SelectedCrop !=null && _SelectedCrop.HasErrors)
                {
                    //skip?
                    ViewModelBase.MessageBox mb = new ViewModelBase.MessageBox("Your current crop has validation issues, would you like to contiue?", ViewModelBase.Enumerations.MessageBoxOptionsEnum.YesNo);
                    Navigate(this, new RequestNavigationEventArgs(mb, ViewModelBase.Enumerations.NavigationEnum.NewScalableDialog, "Errors"));
                    if (mb.Result == ViewModelBase.Enumerations.MessageBoxOptionsEnum.No)
                    {
                        return;
                    }
                }
                _SelectedCrop = value;
                NotifyPropertyChanged();

 }
        }
        public ViewModelBase.NamedAction ApplyAction
        {
            get { return _ApplyAction; }
            set { _ApplyAction = value; NotifyPropertyChanged(); }
        }
        public ViewModelBase.NamedAction CloseAction
        {
            get { return _CloseAction; }
            set { _CloseAction = value; NotifyPropertyChanged(); }
        }
        public CropsEditor(List<CropEditor> cropEditors)
        {
            InitializeNamedActions();
            Crops = cropEditors;
        }

        private void InitializeNamedActions()
        {
            ApplyAction = new ViewModelBase.NamedAction();
            ApplyAction.Name = "Apply";
            ApplyAction.Action = Apply;

            CloseAction = new ViewModelBase.NamedAction();
            CloseAction.Name = "Close";
            CloseAction.Action = thisClose;
        }

        private void Apply(object arg1, EventArgs arg2)
        {
            if (_SelectedCrop.HasErrors)
            {
                ViewModelBase.MessageBox mb = new ViewModelBase.MessageBox("Your current crop has validation issues, would you like to contiue?", ViewModelBase.Enumerations.MessageBoxOptionsEnum.YesNo);
                Navigate(this, new RequestNavigationEventArgs(mb, ViewModelBase.Enumerations.NavigationEnum.NewScalableDialog, "Errors"));
                if (mb.Result == ViewModelBase.Enumerations.MessageBoxOptionsEnum.No)
                {
                    return;
                }
            }

            _SelectedCrop.ApplyChanges();
            //check if the substitute crops need to be updated?

        }

        private void thisClose(object arg1, EventArgs arg2)
        {
            if (_SelectedCrop.HasErrors)
            {
                ViewModelBase.MessageBox mb = new ViewModelBase.MessageBox("Your current crop has validation issues, would you like to contiue?", ViewModelBase.Enumerations.MessageBoxOptionsEnum.YesNo);
                Navigate(this, new RequestNavigationEventArgs(mb, ViewModelBase.Enumerations.NavigationEnum.NewScalableDialog, "Errors"));
                if (mb.Result == ViewModelBase.Enumerations.MessageBoxOptionsEnum.No)
                {
                    return;
                }
            }
            Close?.Invoke(arg1, new RequestCloseEventArgs());
        }

        public void Navigate(object sender, RequestNavigationEventArgs e)
        {
            RequestNavigation?.Invoke(sender, e);
        }
        public void RequestClose(object sender, RequestCloseEventArgs e)
        {
            Close?.Invoke(sender, e);
        }
    }
}
