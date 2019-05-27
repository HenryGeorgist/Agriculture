using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.Interfaces
{
    public interface IAgricultureFloodEventProvider
    {
        IAgriculturalFloodEvent ProvideFloodEvent(System.DateTime startDate, ICropLocation location);
        IList<IAgriculturalFloodEvent> ProvideFloodEvents(System.DateTime startDate, IList<ICropLocation> locations);
    }
}
