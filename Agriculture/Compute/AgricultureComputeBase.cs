using System.Collections.Generic;
using System.Linq;
using Agriculture.Interfaces;

namespace Agriculture.Compute
{
    public class AgricultureComputeBase
    {
        protected IList<Agriculture.Interfaces.ICrop> _Crops;
        protected IList<Interfaces.ICropLocation> _CropLocations;
        private double _AreaToAcresConversion;
        public IList<ICrop> Crops
        {
            get { return _Crops; }
            set { _Crops = value; }
        }
        public AgricultureComputeBase()
        {
        }
        protected virtual void InitializeCrops()
        {
            //write a reader? get from EF?
        }
        protected virtual void InitializeCropLocations(IGeographicCropDataProvider dataProvider, ICropFilter filter)
        {
            _CropLocations = dataProvider.FilteredCrops(filter);
            _AreaToAcresConversion = dataProvider.AcresPerLocation();
        }
        public virtual IAgricultureComputeResult Compute(IGeographicCropDataProvider dataProvider, ICropFilter filter, IAgricultureFloodEventProvider hydraulicsProvider, System.DateTime startDate)
        {
            System.Diagnostics.Stopwatch OverAlltimer = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch AgComputeTimer = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch HydraulicsComputeTimer = new System.Diagnostics.Stopwatch();
            OverAlltimer.Start();
            InitializeCrops();
            InitializeCropLocations(dataProvider, filter);

            IAgricultureComputeResult result = new Agriculture.Compute.Results.AgComputeResult();
            OverAlltimer.Stop();
            System.Diagnostics.Debug.WriteLine(OverAlltimer.Elapsed.TotalSeconds + " processing Ag tiffs");
            //OverAlltimer = new System.Diagnostics.Stopwatch();
            HydraulicsComputeTimer.Start();
            OverAlltimer.Start();
            IList<IAgriculturalFloodEvent> events = hydraulicsProvider.ProvideFloodEvents(startDate, _CropLocations);
            HydraulicsComputeTimer.Stop();
            OverAlltimer.Stop();
            System.Diagnostics.Debug.WriteLine(HydraulicsComputeTimer.Elapsed.TotalSeconds + " processing Hydraulics tiffs");
            OverAlltimer.Start();
            AgComputeTimer.Start();

            for (int i = 0; i < events.Count; i++)
            {
                ICrop tmpcrop = _Crops.FirstOrDefault(crop => crop.CropID == events[i].CropID);
                if (tmpcrop != null)
                {
                    ICropResult cropresult = tmpcrop.ComputeDamages(events[i]);
                    cropresult.DurationInDecimalHours = events[i].DurationInDecimalHours;
                    cropresult.StartDate = events[i].StartDate;
                    cropresult.CropName = tmpcrop.CropName;
                    cropresult.Damage = cropresult.Damage * _AreaToAcresConversion;//convert to acres
                    result.Results.Add(cropresult);
                    if (i % 50000 == 0)
                    {
                        AgComputeTimer.Stop();
                        System.Diagnostics.Debug.WriteLine((double)i / _CropLocations.Count() + " percent complete, 50000 events completed. Took " + AgComputeTimer.Elapsed.TotalSeconds + " seconds");
                        AgComputeTimer = new System.Diagnostics.Stopwatch();
                        AgComputeTimer.Start();
                    }
                }
            }
            OverAlltimer.Stop();
            System.Diagnostics.Debug.WriteLine("complete Took " + OverAlltimer.Elapsed.TotalSeconds + " seconds");
            return result;
        }
    }
}
