using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agriculture.Interfaces
{
    public interface IDurationDamageFunction : ICropDamageFunction
    {
        Dictionary<double,List<double>> DurationDamageFunction { get; }
    }
}
