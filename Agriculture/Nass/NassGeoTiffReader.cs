using System;
using System.Collections.Generic;
using System.Linq;
using Agriculture.Interfaces;

namespace Agriculture.Nass
{
    public class NassGeoTiffReader : IGeographicCropDataProvider
    {
        private string _filePath;
        private double _AreaInAcres;
        public GDALAssist.Projection Projection { get; set; }
        public NassGeoTiffReader(string path)
        {
            _filePath = path;
        }
        public IList<ICropLocation> FilteredCrops(ICropFilter filter)
        {
            IList<ICropLocation> crops = new List<ICropLocation>();
            double cellSizeX = 0;
            double cellSizeY = 0;
            double yMin = 0;
            double yMax = 0;
            double xMin = 0;
            double xMax = 0;
            using (GDALAssist.GDALRaster r = new GDALAssist.GDALRaster(_filePath))
            {
                cellSizeX = r.GetCellSizeX();
                cellSizeY = r.GetCellSizeY();
                yMin = r.GetFullNativeExtents().Bottom;
                yMax = r.GetFullNativeExtents().Top;
                xMin = r.GetFullNativeExtents().Left;
                xMax = r.GetFullNativeExtents().Right;
                Projection = r.GetProjection();
            }
            switch (Projection.GetHorizDatumUnits())
            {
                case "Feet":
                    _AreaInAcres = cellSizeX * cellSizeY * 2.29568e-5;
                    break;
                case "Meters":
                    _AreaInAcres = cellSizeX * cellSizeY * 0.000247105;
                    break;
                case "Degrees":
                    _AreaInAcres = cellSizeX * cellSizeY * 0.009997883733872;
                    break;
                default:
                    _AreaInAcres = cellSizeX * cellSizeY * 0.000247105;//assumes meters.
                    break;
            }
            //process the tiff.
            Int32 rowsPerStrip;
                Int32 imageWidth = 0;
                Int32 imageHeight = 0;
                UInt32 j = 0;
            Int32[] stripOffsets;
            Int32[] stripBytes;
            using (BitMiracle.LibTiff.Classic.Tiff t = BitMiracle.LibTiff.Classic.Tiff.Open(_filePath, "r")){

                imageWidth = t.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGEWIDTH).FirstOrDefault().ToInt();
                imageHeight = t.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH).FirstOrDefault().ToInt();
                BitMiracle.LibTiff.Classic.FieldValue[] asdf = t.GetField(BitMiracle.LibTiff.Classic.TiffTag.STRIPOFFSETS);
                stripOffsets = asdf[0].ToIntArray();
                asdf = t.GetField(BitMiracle.LibTiff.Classic.TiffTag.STRIPBYTECOUNTS);
                stripBytes = asdf[0].ToIntArray();
                rowsPerStrip = t.GetField(BitMiracle.LibTiff.Classic.TiffTag.ROWSPERSTRIP)[0].ToInt();
            }

            using (System.IO.FileStream fs = new System.IO.FileStream(_filePath, System.IO.FileMode.Open))
            {
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs))
                {
                    byte[] bytes;// = br.ReadBytes(8);
                    int i = 0;
                    int k = 0;
                    Int32 xpos = 1;
                    Int32 ypos = 0;
                    for (i = 0; i < stripOffsets.Count(); i++)
                    {
                        br.BaseStream.Seek(stripOffsets[i], System.IO.SeekOrigin.Begin);
                        int numbytes = stripBytes[i];
                        bytes = br.ReadBytes(numbytes);
                        for (k = 0; k < stripBytes[i]; k++)
                        {
                            ICropLocation crop = new Crops.CropLocation();
                            crop.CropID = bytes[k];
                            crop.X = xMin + (xpos * cellSizeX) - (cellSizeX / 2);
                            crop.Y = yMax - (ypos * cellSizeY) - (cellSizeY / 2);
                            if (filter.IncludeCrop(crop))
                            {
                                crops.Add(crop);
                            }
                            xpos += 1;
                            if (xpos > imageWidth)
                            {
                                xpos = 1;
                                ypos += 1;
                            }
                        }
                    }
                }
            }

            return crops;
        }

        public double AcresPerLocation()
        {
            return _AreaInAcres;
        }
    }
}
