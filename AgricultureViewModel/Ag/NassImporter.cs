using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModelBase.Events;
using ViewModelBase.Interfaces;

namespace Agriculture.ViewModel.Ag
{
    public class NassImporter : ViewModelBase.BaseViewModel, ViewModelBase.Interfaces.INavigate, ViewModelBase.Interfaces.ICanClose
    {
        private ViewModelBase.NamedAction _Statistics;
        private ViewModelBase.NamedAction _Apply;
        private ViewModelBase.NamedAction _Close;
        private bool _success = false;
        private System.Collections.ObjectModel.ObservableCollection<NassStatistics> _StatisticsResult;
        private string _Year;
        private string _OutputDirectory;
        private string _Name;
        private IFileNameAndPath _InputFile;
        private System.Collections.ObjectModel.ObservableCollection<ViewModelBase.Interfaces.IFileNameAndPath> _InputFilePaths;

        public event RequestNavigationHandler RequestNavigation;
        public event RequestCloseHandler Close;
        public ViewModelBase.NamedAction Statistics
        {
            get { return _Statistics; }
            set { _Statistics = value; NotifyPropertyChanged(); }
        }
        public ViewModelBase.NamedAction ApplyAction
        {
            get { return _Apply; }
            set { _Apply = value; NotifyPropertyChanged(); }
        }
        public ViewModelBase.NamedAction CloseAction
        {
            get { return _Close; }
            set { _Close = value; NotifyPropertyChanged(); }
        }
        public System.Collections.ObjectModel.ObservableCollection<NassStatistics> StatisticsResult
        {
            get { return _StatisticsResult; }
            set { _StatisticsResult = value; NotifyPropertyChanged(); }
        }
        public string Year
        {
            get { return _Year; }
            set { _Year = value; NotifyPropertyChanged(); }
        }
        public IFileNameAndPath InputFile
        {
            get { return _InputFile; }
            set { _InputFile.FilePath = value.FilePath; NotifyPropertyChanged(); }
        }
        public System.Collections.ObjectModel.ObservableCollection<IFileNameAndPath> InputFilePaths
        {
            get { return _InputFilePaths; }
            set { _InputFilePaths = value; NotifyPropertyChanged(); }
        }
        public bool Success { get { return _success; } }
        public string Name { get { return _Name; } set { _Name = value; NotifyPropertyChanged(); } }
        public NassImporter(string outputDirectory, string[] polygonShapefilePaths)
        {
            _OutputDirectory = outputDirectory;
            _InputFile = new ViewModelBase.FileNameAndPath();
            StatisticsResult = new System.Collections.ObjectModel.ObservableCollection<ViewModel.Ag.NassStatistics>();
            InputFilePaths = new System.Collections.ObjectModel.ObservableCollection<IFileNameAndPath>();
            foreach(string s in polygonShapefilePaths)
            {
                ViewModelBase.FileNameAndPath FNAP = new ViewModelBase.FileNameAndPath();
                FNAP.FilePath = s;
                InputFilePaths.Add(FNAP);
            }
            InputFile = InputFilePaths.FirstOrDefault();

            Statistics = new ViewModelBase.NamedAction();
            Statistics.Name = "Get Statistics";
            Statistics.Action = NassStatistics;

            ApplyAction = new ViewModelBase.NamedAction();
            ApplyAction.Name = "Download GeoTiff";
            ApplyAction.Action = ApplyActionHandler;

            CloseAction = new ViewModelBase.NamedAction();
            CloseAction.Name = "Close";
            CloseAction.Action = CloseActionHandler;

            
            _InputFile.AddSinglePropertyRule(nameof(ViewModelBase.FileNameAndPath.FilePath), new ViewModelBase.Rule(ShapeExtensionRule, "File must have a shape extension"));
            _InputFile.AddSinglePropertyRule(nameof(ViewModelBase.FileNameAndPath.FilePath), new ViewModelBase.Rule(PolygonShapeRule, "File must be a polygon shapefile"));

            AddSinglePropertyRule(nameof(Year), new ViewModelBase.Rule(IntegerRule, "Year must be an integer."));
            AddSinglePropertyRule(nameof(Year), new ViewModelBase.Rule(NassYearRule, "Year must be available (between 2008 and 2016) on the NASS website."));
            AddSinglePropertyRule(nameof(Name), new ViewModelBase.Rule(NameRule, "Name cannot be blank."));


            //initalize with validation errors triggered.
            Name = "";
            Year = "";
            ModelBase.Logging.Logger.Instance.LogMessage(new ModelBase.Logging.ErrorMessage("Initalizing Nass Importer from VM", ModelBase.Logging.ErrorMessageEnum.Info));
        }
        private bool NameRule()
        {
            return Name != "";
        }
        private bool NassYearRule()
        {
            int year = 0;
            if (Int32.TryParse(Year, out year))
            {
                return (2008<=year&&year<=2016);
            }
            return false;
        }
        private bool IntegerRule()
        {
            int year = 0;
            return Int32.TryParse(Year,out year);
        }
        private bool PolygonShapeRule()
        {

            //enusre it is a polygon shapefile.
            if (_InputFile.FilePath == null) return false;
            if (_InputFile.FilePath == "") return false;
            LifeSimGIS.ShapefileReader s = new LifeSimGIS.ShapefileReader(_InputFile.FilePath);
            if (s.ShapeType == LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Polygon) return true;
            if (s.ShapeType == LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonM) return true;
            if (s.ShapeType == LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonZM) return true;
            return false;
        }
        private bool ShapeExtensionRule()
        {
            if (_InputFile.FilePath == null) return false;
            if (_InputFile.FilePath == "") return false;
            string ext = _InputFile.FilePath.Substring(_InputFile.FilePath.Count() - 3, 3).ToLower();
            return ext == "shp";
        }
        private void CloseActionHandler(object arg1, EventArgs arg2)
        {
            Close(this, new RequestCloseEventArgs());
        }
        private async void ApplyActionHandler(object arg1, EventArgs arg2)
        {
            //requests must be made in albers equal area meters; EPSG 6703
            Agriculture.Nass.GeoTiff g = new Agriculture.Nass.GeoTiff();
            bool success;
            LifeSimGIS.Extent ext = CheckFileAndGetExtent(out success);
            if (success)
            {
                var result = await g.RequestFileForBoundingBox(_OutputDirectory + "\\" + Name + ".tif", Year, ext.MinX, ext.MinY, ext.MaxX, ext.MaxY);

                //filter by selected crops to import?
                //improve the ouptut of the requestfileforboundingbox to produce an error result that is inteligalble.
                if (!result)
                {
                    ViewModelBase.MessageBox mbox = new ViewModelBase.MessageBox("Something bad happened", ViewModelBase.Enumerations.MessageBoxOptionsEnum.Ok);
                    Navigate(this, new RequestNavigationEventArgs(mbox, ViewModelBase.Enumerations.NavigationEnum.NewScalable, "Error Message"));
                }
                _success = true;
                Close(this, new RequestCloseEventArgs());
            }
        }
        private async void NassStatistics(object arg1, EventArgs arg2)
        {
            Agriculture.Nass.Statistics s = new Agriculture.Nass.Statistics();
            //get bounding box. 
            bool success;
            StatisticsResult = new System.Collections.ObjectModel.ObservableCollection<ViewModel.Ag.NassStatistics>(); 
            LifeSimGIS.Extent ext = CheckFileAndGetExtent(out success);
            if (success)
            {
                var result = await s.GetStatistics(Year,ext.MinX,ext.MinY,ext.MaxX,ext.MaxY);
                if (result.success)
                {
                foreach(Agriculture.Nass.Json.StatisticsRow r in s.Result.rows)
                {
                    StatisticsResult.Add(new ViewModel.Ag.NassStatistics(r));
                }
                }else
                {
                    ViewModelBase.MessageBox mbox = new ViewModelBase.MessageBox(result.errorMessage, ViewModelBase.Enumerations.MessageBoxOptionsEnum.Ok);
                    Navigate(this, new RequestNavigationEventArgs(mbox, ViewModelBase.Enumerations.NavigationEnum.NewScalable, "Error Message"));
                }

            }
        }
        private LifeSimGIS.Extent CheckFileAndGetExtent(out bool Success)
        {
            LifeSimGIS.Extent ext = new LifeSimGIS.Extent();
            Success = true;
            if (_InputFile.FilePath == null)
            {
                Success = false;
                ViewModelBase.MessageBox mbox = new ViewModelBase.MessageBox("No Path was defined", ViewModelBase.Enumerations.MessageBoxOptionsEnum.Ok);
                Navigate(this, new RequestNavigationEventArgs(mbox, ViewModelBase.Enumerations.NavigationEnum.NewScalable, "Error Message"));
                return ext;
            }
            if (_InputFile.FilePath == "")
            {
                Success = false;
                ViewModelBase.MessageBox mbox = new ViewModelBase.MessageBox("No Path was defined", ViewModelBase.Enumerations.MessageBoxOptionsEnum.Ok);
                Navigate(this, new RequestNavigationEventArgs(mbox, ViewModelBase.Enumerations.NavigationEnum.NewScalable, "Error Message"));
                return ext;
            }
            if (!_InputFile.HasErrors)
            {
                //requests must be made in albers equal area meters;
                LifeSimGIS.ShapefileReader sr = new LifeSimGIS.ShapefileReader(_InputFile.FilePath);
                ext = sr.Extent.BaseExtent;

                if (System.IO.File.Exists(System.IO.Path.ChangeExtension(_InputFile.FilePath, ".prj")))
                {
                    //GDALAssist.EPSGProjection epsg = new GDALAssist.EPSGProjection(6703);
                    GDALAssist.Proj4Projection proj4 = new GDALAssist.Proj4Projection("+proj=aea +lat_1=29.5 +lat_2=45.5 +lat_0=23 +lon_0=-96 +x_0=0 +y_0=0 +datum=NAD83 +units=m +no_defs ");
                    GDALAssist.WKTProjection wkt = new GDALAssist.WKTProjection(System.IO.Path.ChangeExtension(_InputFile.FilePath, ".prj"));
                    if (!wkt.Equals(proj4))
                    {
                        //reproject extent or reproject feature?
                        LifeSimGIS.VectorFeatures pg = sr.ToFeatures();
                        string errors = null;
                        pg.Reproject(proj4, ref errors);
                        ext = pg.Extent.BaseExtent;
                        if (errors != null)
                        {
                            ViewModelBase.MessageBox mbox = new ViewModelBase.MessageBox(errors, ViewModelBase.Enumerations.MessageBoxOptionsEnum.Ok);
                            Navigate(this, new RequestNavigationEventArgs(mbox, ViewModelBase.Enumerations.NavigationEnum.NewScalable, "Error Message"));
                            Success = false;
                        }
                        if((ext.MaxX - ext.MinX) < 60)
                        {
                            ViewModelBase.MessageBox mbox = new ViewModelBase.MessageBox("Height is less than 60m", ViewModelBase.Enumerations.MessageBoxOptionsEnum.Ok);
                            Navigate(this, new RequestNavigationEventArgs(mbox, ViewModelBase.Enumerations.NavigationEnum.NewScalable, "Error Message"));
                            Success = false;
                        }
                        if ((ext.MaxY - ext.MinY) < 60)
                        {
                            ViewModelBase.MessageBox mbox = new ViewModelBase.MessageBox("Width is less than 60m", ViewModelBase.Enumerations.MessageBoxOptionsEnum.Ok);
                            Navigate(this, new RequestNavigationEventArgs(mbox, ViewModelBase.Enumerations.NavigationEnum.NewScalable, "Error Message"));
                            Success = false;
                        }
                    }
                }
                else
                {
                    //punt.
                    ViewModelBase.MessageBox mbox = new ViewModelBase.MessageBox("Shapefile does not have a projection", ViewModelBase.Enumerations.MessageBoxOptionsEnum.Ok);
                    Navigate(this, new RequestNavigationEventArgs(mbox, ViewModelBase.Enumerations.NavigationEnum.NewScalable, "Error Message"));
                    Success = false;
                }
            }
            else
            {
                ViewModelBase.MessageBox mbox = new ViewModelBase.MessageBox(_InputFile.Error, ViewModelBase.Enumerations.MessageBoxOptionsEnum.Ok);
                Navigate(this, new RequestNavigationEventArgs(mbox, ViewModelBase.Enumerations.NavigationEnum.NewScalable, "Error Message"));
                Success = false;
            }
            return ext;
        }
        public void Navigate(object sender, RequestNavigationEventArgs e)
        {
            RequestNavigation?.Invoke(sender, e);
        }

        public void RequestClose(object sender, RequestCloseEventArgs e)
        {
            Close?.Invoke(this, e);
        }
    }
}
