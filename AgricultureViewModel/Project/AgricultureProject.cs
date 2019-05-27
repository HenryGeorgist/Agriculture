using ViewModelBase.Events;

namespace Agriculture.ViewModel.Project
{
    public class AgricultureProject : ViewModelBase.BaseViewModel, ViewModelBase.Interfaces.INavigate, ViewModelBase.Interfaces.ICanClose
    {
        private System.Collections.ObjectModel.ObservableCollection<ViewModelBase.HierarchicalViewModel> _Items;

        public event RequestCloseHandler Close;
        public event RequestNavigationHandler RequestNavigation;

        public System.Collections.ObjectModel.ObservableCollection<ViewModelBase.HierarchicalViewModel> Items
        {
            get { return _Items; }
            set { _Items = value; NotifyPropertyChanged(); }
        }
        public AgricultureProject()
        {
            Items = new System.Collections.ObjectModel.ObservableCollection<ViewModelBase.HierarchicalViewModel>();

           Project.ProjectNode node = new Project.ProjectNode();
            node.RequestNavigation += Navigate;
            node.Close += RequestClose;
            Items.Add(node);
            ModelBase.Logging.Logger.Instance.LogMessage(new ModelBase.Logging.ErrorMessage("Initalizing Project from VM", ModelBase.Logging.ErrorMessageEnum.Info));
            //Agriculture.ViewModel.AgricultureInventories node = new AgricultureInventories();
            //node.RequestNavigation += Navigate;
            //node.Close += RequestClose;
            //Items.Add(node);

            //HydraulicConfigurations hnode = new HydraulicConfigurations();
            //hnode.RequestNavigation += Navigate;
            //hnode.Close += RequestClose;
            //Items.Add(hnode);
        }
        public void Navigate(object sender, RequestNavigationEventArgs e)
        {
            RequestNavigation?.Invoke(sender,e);
        }
        public void RequestClose(object sender, RequestCloseEventArgs e)
        {
            Close?.Invoke(sender, e);
        }
    }
}
