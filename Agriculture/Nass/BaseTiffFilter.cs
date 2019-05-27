using Agriculture.Interfaces;

namespace Agriculture.Nass
{
    public class BaseTiffFilter : Interfaces.ICropFilter
    {
        public virtual bool IncludeCrop(ICropLocation loc)
        {
            if (loc.CropID == 1) return true;
            if (loc.CropID == 5) return true;
            return false;
        }
    }
}
