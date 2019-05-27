using System.Collections.Generic;

namespace Agriculture.Nass.Json
{
    public class StatisticsResult
    {
        
        public bool success { get; set; }
        public string errorMessage { get; set; }
        //public int totalCount { get; set; }
        //public string fileName { get; set; }
        public List<StatisticsRow> rows { get; set; }
        public StatisticsResult()
        {
            rows = new List<StatisticsRow>();
        }
    }
}
