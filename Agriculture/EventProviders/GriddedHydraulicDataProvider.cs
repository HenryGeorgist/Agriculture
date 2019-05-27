using System;
using System.Collections.Generic;
using Agriculture.Interfaces;

namespace Agriculture.EventProviders
{
    public class GriddedHydraulicDataProvider : IAgricultureFloodEventProvider
    {
        private LifeSimGIS.GridReader _ArrivalGridReader;
        private LifeSimGIS.GridReader _DurationGridReader;
        public GriddedHydraulicDataProvider(string arrivalGridPath, string durationGridPath)
        {
            _ArrivalGridReader = new LifeSimGIS.TiffReader(arrivalGridPath);
            _DurationGridReader = new LifeSimGIS.TiffReader(durationGridPath);
        }
        public IAgriculturalFloodEvent ProvideFloodEvent(System.DateTime startDate, ICropLocation location)
        {
            //assumes nad 83 albers.
            IAgriculturalFloodEvent e = new Agriculture.EventProviders.AgricultureEvent();
            LifeSimGIS.PointD point = new LifeSimGIS.PointD(location.X, location.Y);
            double startOffset = _ArrivalGridReader.SampleValue(point);
            if (startOffset == _ArrivalGridReader.NoData[0] || Double.IsNaN(startOffset))
            {
                return null;
            }
            e.StartDate = startDate.AddHours(startOffset);
            e.DurationInDecimalHours = _DurationGridReader.SampleValue(point);
            e.X = location.X;
            e.Y = location.Y;
            return e;
        }

        public IList<IAgriculturalFloodEvent> ProvideFloodEvents(DateTime startDate, IList<ICropLocation> locations)
        {
            IList<IAgriculturalFloodEvent> events = new List<IAgriculturalFloodEvent>();
            LifeSimGIS.PointD[] points = new LifeSimGIS.PointD[locations.Count];
            for (int i = 0; i < locations.Count; i++)
            {
                points[i] = new LifeSimGIS.PointD(locations[i].X, locations[i].Y);
            }
            float[] arrivals = _ArrivalGridReader.SampleValues(points);
            float[] durations = _DurationGridReader.SampleValues(points);
            for (int i = 0; i < locations.Count; i++)
            {
                IAgriculturalFloodEvent e = new Agriculture.EventProviders.AgricultureEvent();
                float startOffset = arrivals[i];
                if (Single.IsNaN(startOffset))
                {
                    //events.Add(null);
                }
                else if (startOffset<-1)
                {
                    //events.Add(null);
                }
                else
                {
                    e.StartDate = startDate.AddHours(startOffset);
                    e.DurationInDecimalHours = durations[i];
                    e.X = locations[i].X;
                    e.Y = locations[i].Y;
                    e.CropName = locations[i].CropName;
                    e.CropID = locations[i].CropID;
                    events.Add(e);
                }

            }
            return events;
        }
    }
}
