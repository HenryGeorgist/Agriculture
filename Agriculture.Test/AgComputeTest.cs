using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agriculture.Test
{
    [TestClass]
    public class AgComputeTest
    {
        [ClassInitialize]
        static public void InitalizeGDAL(TestContext bla)
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
                throw new ArgumentException("Gdal did not initalize");//Messager.Logger.Instance.ReportMessage(new Messager.ErrorMessage(ex.InnerException.ToString() + "\n Failed to initialize GDAL, check if the GDAL directory is next to the FdaModel.dll", Messager.ErrorMessageEnum.Fatal | Messager.ErrorMessageEnum.Model));
            }
        }
        [TestMethod]
        public void ComputeDamages()
        {
            IList<Interfaces.ICrop> crops = new List<Interfaces.ICrop>();
            CropSchedules.SingleYearCropSchedule cornSchedule = new CropSchedules.SingleYearCropSchedule(new DateTime(2014, 4, 14), 52, 220);

            System.Collections.Generic.List<double> monthlyCosts = new System.Collections.Generic.List<double>() { 0.0d, 0.0d, 10.0d, 73.0d, 260.0d, 41.0d, 21.0d, 14.0d, 27.0d, 10.0d, 0.0d, 0.0d };
            Interfaces.ICropProductionFunction cpf = new ProductionFunctions.AgProductionFunction(monthlyCosts, cornSchedule, .3, 2.5);

            Dictionary<double, List<double>> damFun = new Dictionary<double, List<double>>();
            List<double> zeroDay = new List<double>() { 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            List<double> threeDay = new List<double>() { 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 100.0, 99.2, 75.0, 0.0 };
            List<double> sevenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            List<double> fourteenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            damFun.Add(0.0d, zeroDay);
            damFun.Add(72.0d, threeDay);
            damFun.Add(168.0d, sevenDay);
            damFun.Add(336.0d, fourteenDay);
            DamageFunctions.DurationDamageFunction CornDamageFunction = new Agriculture.DamageFunctions.DurationDamageFunction(damFun);
            Crops.Crop corn = new Crops.Crop("Corn", 1, 5.2, 194, cornSchedule, cpf, CornDamageFunction);
            crops.Add(corn);

            CropSchedules.SingleYearCropSchedule soySchedule = new CropSchedules.SingleYearCropSchedule(new DateTime(2014, 5, 2), 53, 189);

            System.Collections.Generic.List<double> soymonthlyCosts = new System.Collections.Generic.List<double>() { 0.0, 0.0, 5.0, 28.0, 139.0, 23.0, 11.0, 7.0, 14.0, 5.0, 0.0, 0.0 };
            Interfaces.ICropProductionFunction spf = new ProductionFunctions.AgProductionFunction(soymonthlyCosts, soySchedule, .3, 4.29);

            Dictionary<double, List<double>> soydamFun = new Dictionary<double, List<double>>();
            List<double> soyzeroDay = new List<double>() { 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            List<double> soythreeDay = new List<double>() { 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 100.0, 99.2, 75.0, 0.0 };
            List<double> soysevenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            List<double> soyfourteenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            soydamFun.Add(0.0d, soyzeroDay);
            soydamFun.Add(72.0d, soythreeDay);
            soydamFun.Add(168.0d, soysevenDay);
            soydamFun.Add(336.0d, soyfourteenDay);
            DamageFunctions.DurationDamageFunction soyDamageFunction = new Agriculture.DamageFunctions.DurationDamageFunction(soydamFun);
            Crops.Crop soy = new Crops.Crop("Soy Beans", 5, 11.58, 56, soySchedule, spf, soyDamageFunction);
            crops.Add(soy);

            Agriculture.Compute.AgricultureComputeBase compute = new Compute.AgricultureComputeBase();
            compute.Crops = crops;

            Agriculture.Nass.BaseTiffFilter filter = new Nass.BaseTiffFilter();
            Agriculture.Nass.NassGeoTiffReader nassReader = new Nass.NassGeoTiffReader(@"C:\Temp\NASS\NWK_LP_Data\Crops.tif");
            Agriculture.EventProviders.MultiThreadedGriddedHydraulicDataProvider gridReader = new EventProviders.MultiThreadedGriddedHydraulicDataProvider(@"C:\Temp\NASS\NWK_LP_Data\ATG.tif", @"C:\Temp\NASS\NWK_LP_Data\DurG.tif");

            Agriculture.Compute.Results.AgComputeResult result = (Agriculture.Compute.Results.AgComputeResult)compute.Compute(nassReader, filter, gridReader, new DateTime(2014, 7, 4, 0, 0, 0));
            result.ToShapeFile(@"C:\Temp\NASS\NWK_LP_Data\ComputeOutput.shp", nassReader.Projection);
        }
        [TestMethod]
        public void ComputeDamagesSLR()
        {
            IList<Interfaces.ICrop> crops = new List<Interfaces.ICrop>();
            CropSchedules.SingleYearCropSchedule cornSchedule = new CropSchedules.SingleYearCropSchedule(new DateTime(2014, 4, 14), 52, 220);

            System.Collections.Generic.List<double> monthlyCosts = new System.Collections.Generic.List<double>() { 0.0d, 0.0d, 10.0d, 73.0d, 260.0d, 41.0d, 21.0d, 14.0d, 27.0d, 10.0d, 0.0d, 0.0d };
            Interfaces.ICropProductionFunction cpf = new ProductionFunctions.AgProductionFunction(monthlyCosts, cornSchedule, .3, 2.5);

            Dictionary<double, List<double>> damFun = new Dictionary<double, List<double>>();
            List<double> zeroDay = new List<double>() { 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            List<double> threeDay = new List<double>() { 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 100.0, 99.2, 75.0, 0.0 };
            List<double> sevenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            List<double> fourteenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            damFun.Add(0.0d, zeroDay);
            damFun.Add(72.0d, threeDay);
            damFun.Add(168.0d, sevenDay);
            damFun.Add(336.0d, fourteenDay);
            DamageFunctions.DurationDamageFunction CornDamageFunction = new Agriculture.DamageFunctions.DurationDamageFunction(damFun);
            Crops.Crop corn = new Crops.Crop("Corn", 1, 5.2, 194, cornSchedule, cpf, CornDamageFunction);
            crops.Add(corn);

            CropSchedules.SingleYearCropSchedule soySchedule = new CropSchedules.SingleYearCropSchedule(new DateTime(2014, 5, 2), 53, 189);

            System.Collections.Generic.List<double> soymonthlyCosts = new System.Collections.Generic.List<double>() { 0.0, 0.0, 5.0, 28.0, 139.0, 23.0, 11.0, 7.0, 14.0, 5.0, 0.0, 0.0 };
            Interfaces.ICropProductionFunction spf = new ProductionFunctions.AgProductionFunction(soymonthlyCosts, soySchedule, .3, 4.29);

            Dictionary<double, List<double>> soydamFun = new Dictionary<double, List<double>>();
            List<double> soyzeroDay = new List<double>() { 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d };
            List<double> soythreeDay = new List<double>() { 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 75.0, 100.0, 99.2, 75.0, 0.0 };
            List<double> soysevenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            List<double> soyfourteenDay = new List<double>() { 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 0.0 };
            soydamFun.Add(0.0d, soyzeroDay);
            soydamFun.Add(72.0d, soythreeDay);
            soydamFun.Add(168.0d, soysevenDay);
            soydamFun.Add(336.0d, soyfourteenDay);
            DamageFunctions.DurationDamageFunction soyDamageFunction = new Agriculture.DamageFunctions.DurationDamageFunction(soydamFun);
            Crops.Crop soy = new Crops.Crop("Soy Beans", 5, 11.58, 56, soySchedule, spf, soyDamageFunction);
            crops.Add(soy);

            Agriculture.Compute.AgricultureComputeBase compute = new Compute.AgricultureComputeBase();
            compute.Crops = crops;

            Agriculture.Nass.BaseTiffFilter filter = new Nass.BaseTiffFilter();
            Agriculture.Nass.NassGeoTiffReader nassReader = new Nass.NassGeoTiffReader(@"C:\Temp\NASS\SLR_Data\Agriculture\SLR_AG.tif");
            Agriculture.EventProviders.GriddedHydraulicDataProvider gridReader = new EventProviders.GriddedHydraulicDataProvider(@"C:\Temp\NASS\SLR_Data\Hydraulics\Arrival Time (hrs).tif", @"C:\Temp\NASS\SLR_Data\Hydraulics\Duration (hrs).tif");

            Agriculture.Compute.Results.AgComputeResult result = (Agriculture.Compute.Results.AgComputeResult)compute.Compute(nassReader, filter, gridReader, new DateTime(2014, 7, 4, 0, 0, 0));
            result.ToShapeFile(@"C:\Temp\NASS\SLR_Data\Output\ComputeOutput.shp", nassReader.Projection);
        }
    }
}
