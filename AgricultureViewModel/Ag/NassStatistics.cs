using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.ViewModel.Ag
{
    public class NassStatistics: ViewModelBase.BaseViewModel, Interfaces.IRelateCrops
    {
        private readonly Agriculture.Nass.Json.StatisticsRow _Statistics;
        private bool _ImportCrop = false;
        public string CropName
        {
            get { return _Statistics.category; }
            set { }
        }
        public int CropID
        {
            get { return _Statistics.value; }
            set { }
        }
        public float Acerage
        {
            get { return _Statistics.acreage; }
        }
        
        public bool ImportCrop
        {
            get { return _ImportCrop; }
            set { _ImportCrop = value; NotifyPropertyChanged(); }
        }
        public NassStatistics(Agriculture.Nass.Json.StatisticsRow stats)
        {
            _Statistics = stats;
        }
    }
}
