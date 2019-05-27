using System.Collections.Generic;
using Agriculture.Interfaces;

namespace Agriculture.Compute.Results
{
    public class AgComputeResult : Agriculture.Interfaces.IAgricultureComputeResult
    {
        public IList<ICropResult> Results { get; set; }
        public AgComputeResult()
        {
            Results = new List<ICropResult>();
        }
        public void ToShapeFile(string outputDestination, GDALAssist.Projection proj)
        {
            LifeSimGIS.ShapefileWriter sr = new LifeSimGIS.ShapefileWriter(outputDestination);
            System.Data.DataTable dt = new System.Data.DataTable();
            System.Data.DataColumn CropName = new System.Data.DataColumn("Name", typeof(string));
            System.Data.DataColumn Outcome = new System.Data.DataColumn("Outcome", typeof(string));
            System.Data.DataColumn Damage = new System.Data.DataColumn("Damage", typeof(double));
            System.Data.DataColumn Duration = new System.Data.DataColumn("Duration", typeof(string));
            System.Data.DataColumn Time = new System.Data.DataColumn("Arrival", typeof(string));
            dt.Columns.Add(CropName);
            dt.Columns.Add(Outcome);
            dt.Columns.Add(Damage);
            dt.Columns.Add(Duration);
            dt.Columns.Add(Time);
            LifeSimGIS.PointFeatures pf = new LifeSimGIS.PointFeatures();
            foreach(ICropResult r in Results)
            {
                pf.Add(new LifeSimGIS.PointFeature(new LifeSimGIS.PointD(r.X, r.Y)));
                dt.Rows.Add(new object[] { r.CropName, r.DamageCase.ToString(), r.Damage, r.DurationInDecimalHours, r.StartDate.ToString() });
            }
            //pf.Extent.BaseExtent.UpdateExtent();
            LifeSimGIS.PointFeatures pfs = new LifeSimGIS.PointFeatures(pf.GetPointsArray(), outputDestination, proj);
            sr.WriteFeatures(pfs, dt, proj);
        }
    }
}
