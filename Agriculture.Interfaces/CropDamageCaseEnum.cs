using System;

namespace Agriculture.Interfaces
{
    [Flags]
    public enum CropDamageCaseEnum : byte
    {
        Unassigned = 0x00,
        Flooded = 0x01,
        NotFloodedDuringSeason = 0x02,
        PlantingDelayed = 0x04,
        NotPlanted = 0x08,

        SubstituteCrop = 0x10,
    }
}
