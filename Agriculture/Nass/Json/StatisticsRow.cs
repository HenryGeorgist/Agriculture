namespace Agriculture.Nass.Json
{
    public class StatisticsRow
    {
        public int value { get; set; }
        public int count { get; set; }
        public string category { get; set; }
        public string color { get; set; }
        public float acreage { get; set; }
        public StatisticsRow(int val, int cnt, string name, string clr, float acres)
        {
            value = val;
            count = cnt;
            category = name;
            color = clr;
            acreage = acres;
        }
    }
}