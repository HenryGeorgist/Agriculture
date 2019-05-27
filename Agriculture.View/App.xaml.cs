using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Agriculture.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Environment.SetEnvironmentVariable("GDAL_TIFF_OVR_BLOCKSIZE", "256");
                string dir = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                dir = new Uri(dir).LocalPath;
                dir = System.IO.Path.GetDirectoryName(dir);
                string ToolDir = dir + "\\GDAL\\bin";
                string DataDir = dir + "\\GDAL\\data";
                string PluginDir = dir + "\\GDAL\\bin\\gdalplugins";
                string WMSDir = dir + "\\GDAL\\Web Map Services";
                GDALAssist.GDALSetup.Initialize(ToolDir, DataDir, PluginDir, WMSDir);
            }
            catch (Exception ex)
            {
                ViewModelBase.WindowBase wbm = new ViewModelBase.WindowBase(new ViewModelBase.MessageBox("GDAL failed to initalize, " + "\\n" + ex.InnerException, ViewModelBase.Enumerations.MessageBoxOptionsEnum.Close));
                ViewBase.ViewWindow mvw = new ViewBase.ViewWindow(wbm);
                mvw.Show();
            }
            ViewModelBase.WindowBase wb = new ViewModelBase.ScalableWindowBase(new Agriculture.ViewModel.Project.AgricultureProject());
            wb.Title = "Agriculture Flood Damage Software";
            ViewBase.ViewWindow vw = new ViewBase.ViewWindow(wb);
            vw.Show();
        }
    }
}
