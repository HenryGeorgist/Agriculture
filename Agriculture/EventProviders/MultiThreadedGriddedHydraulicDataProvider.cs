using Agriculture.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TiffBinaryReader;

namespace Agriculture.EventProviders
{
    public class MultiThreadedGriddedHydraulicDataProvider : IAgricultureFloodEventProvider
    {
        private string _ArrivalGridReader;
        private string _DurationGridReader;
        private double _DryOutPeriodInHours;
        public MultiThreadedGriddedHydraulicDataProvider(string arrivalGridPath, string durationGridPath):this(arrivalGridPath,durationGridPath,0.0d)
        {

        }
        public MultiThreadedGriddedHydraulicDataProvider(string arrivalGridPath, string durationGridPath, double dryoutPeriodInHours)
        {
            _ArrivalGridReader = arrivalGridPath;
            _DurationGridReader = durationGridPath;
            _DryOutPeriodInHours = dryoutPeriodInHours;
        }
        public IAgriculturalFloodEvent ProvideFloodEvent(DateTime startDate, ICropLocation location)
        {
            throw new NotImplementedException();
        }

        public IList<IAgriculturalFloodEvent> ProvideFloodEvents(DateTime startDate, IList<ICropLocation> locations)
        {

            double cellSizeX = 0;
            double cellSizeY = 0;
            double yMin = 0;
            double yMax = 0;
            double xMin = 0;
            double xMax = 0;
            int width = 0;
            int height = 0;
            double noDataValue = 0;
            using (GDALAssist.GDALRaster r = new GDALAssist.GDALRaster(_ArrivalGridReader))
            {
                cellSizeX = r.GetCellSizeX();
                cellSizeY = r.GetCellSizeY();
                yMin = r.GetFullNativeExtents().Bottom;
                yMax = r.GetFullNativeExtents().Top;
                xMin = r.GetFullNativeExtents().Left;
                xMax = r.GetFullNativeExtents().Right;
                noDataValue = r.GetNoData()[0];//??
                height = r.PixelsTall;
                width = r.PixelsWide;

                //Projection = r.GetProjection();
            }
            //int numtiles = (width / 256) * (height / 256);
            List<ICropLocation>[] tiled = null;//how many tiles are there?
            List<IAgriculturalFloodEvent>[] events = null;
            int tilenum = 0;
            double xcomp = 0;
            double ycomp = 0;

            using (var tf = new TiffFile(_ArrivalGridReader))
            {
                using (var tfD = new TiffFile(_DurationGridReader))
                {

                    //float[] arr = new float[256 * 256];
                    var dir0 = tf.Directories[0];
                    var dir0d = tfD.Directories[0];
                    int nTiles = dir0.Metadata.TileCount;
                    int tHeight = dir0.Metadata.TileHeight;
                    int tWidth = dir0.Metadata.TileWidth;
                    int nTilesWide = dir0.Metadata.TilesWide;
                    int nTilesHigh = dir0.Metadata.TilesTall;
                    var tlBuffs = new ThreadLocal<float[]>(() => new float[tHeight * tWidth]);
                    var tldBluffs = new ThreadLocal<float[]>(() => new float[tWidth * tHeight]);
                    tiled = new List<ICropLocation>[nTiles];
                    List<int> tilesNeeded = new List<int>();
                    events = new List<IAgriculturalFloodEvent>[nTiles];
                    for (int i = 0; i < nTiles; i++)
                    {
                        tiled[i] = new List<ICropLocation>();
                        events[i] = new List<IAgriculturalFloodEvent>();
                    }
                    foreach (ICropLocation loc in locations)
                    {
                        xcomp = Math.Floor((loc.X - xMin) / (cellSizeX * tWidth));
                        if (xcomp < 0 || xcomp > nTilesWide)
                        {
                            //ignore this value outsize the extent of the grid
                            
                        }else
                        {
                            ycomp = Math.Floor((yMax-loc.Y) / (cellSizeY * tHeight));
                            if (ycomp < 0 || ycomp > nTilesHigh)
                            {
                                //ignorethis case outside the extent of the grid.
                            }
                            else
                            {
                                tilenum = Convert.ToInt32(nTilesWide * ycomp + xcomp);
                                tiled[tilenum].Add(loc);
                                if (!tilesNeeded.Contains(tilenum)) tilesNeeded.Add(tilenum);
                            }
                        }

                    }

                    Parallel.For(0, tilesNeeded.Count, i =>
                    {
                        float[] arr = tlBuffs.Value;
                        float[] arrd = tldBluffs.Value;
                        double tXloc;
                        double tYloc;

                        tXloc = xMin + (tilesNeeded[i]%nTilesWide) * tWidth * cellSizeX;
                        tYloc = yMax - Math.Floor((double)tilesNeeded[i] / nTilesWide) * tHeight * cellSizeY;
                        // Please don't let release mode kill this call.
                        dir0.GetTile(tilesNeeded[i], ref arr);
                        dir0d.GetTile(tilesNeeded[i], ref arrd);
                        foreach (ICropLocation l in tiled[tilesNeeded[i]])
                        {
                            //cell location is based on cell size.
                            double cellyComponent = Math.Floor((tYloc-l.Y) / (cellSizeY ));
                            double cellxComponent = Math.Floor((l.X - tXloc) / (cellSizeX));
                            int cellloc = Convert.ToInt32((tWidth* cellyComponent) + cellxComponent);
                            double value = arr[cellloc];
                            double dvalue = arrd[cellloc];
                            if (value >= 0)//(value != noDataValue)
                            {
                                AgricultureEvent AE = new AgricultureEvent();
                                AE.X = l.X;
                                AE.Y = l.Y;
                                AE.CropID = l.CropID;
                                AE.CropName = l.CropName;
                                AE.StartDate = startDate.AddHours(value);
                                AE.DurationInDecimalHours = dvalue + _DryOutPeriodInHours;
                                events[tilesNeeded[i]].Add(AE);
                            }
                        }
                    });
                }
            }
            List<IAgriculturalFloodEvent> masterEvents = new List<IAgriculturalFloodEvent>();
            for (int i = 0; i < events.Count(); i++)
            {
                masterEvents.AddRange(events[i]);
            }
            return masterEvents;
        }
    }
}
