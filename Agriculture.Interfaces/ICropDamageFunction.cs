namespace Agriculture.Interfaces
{
    public interface ICropDamageFunction: IRelateCrops
    {
        double ComputeDamagePercent(IAgriculturalFloodEvent floodEvent);
    }
}