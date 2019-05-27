using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.Interfaces
{
    public interface IRelateCrops
    {
        int CropID { get; set; }
        string CropName { get; set; }
    }
}
