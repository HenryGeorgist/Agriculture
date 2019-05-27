namespace Agriculture.Interfaces
{
    public interface IAgriculturalFloodEvent: ILocation, IRelateCrops
    {
        System.DateTime StartDate { get; set; }
        double DurationInDecimalHours { get; set; }
    }
}