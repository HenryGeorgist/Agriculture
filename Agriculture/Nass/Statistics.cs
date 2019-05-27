using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Agriculture.Nass
{
    public class Statistics
    {
        private const string _baseUrl = "http://nassgeodata.gmu.edu/axis2/services/CDLService/GetCDLStat?";
        private Json.StatisticsResult _Result;
        public Json.StatisticsResult Result
        {
            get { return _Result; }
        }
        private string CreateBboxFileRequestString(string year, double minx, double miny, double maxx, double maxy)
        {
            return _baseUrl + "year=" + year + "&bbox=" + minx + "," + miny + "," + maxx + "," + maxy + "&format=csv";
        }
        public async Task<Json.StatisticsResult> GetStatistics(string year, double minx, double miny, double maxx, double maxy)
        {
            using (var handler = new WebRequestHandler())
            {
                handler.ServerCertificateValidationCallback = (obj, cert, chain, sslpe) =>
                {
                    return true;
                };
                using (var client = new HttpClient(handler))
                {
                    _Result = new Json.StatisticsResult();
                    string finalUrl = CreateBboxFileRequestString(year, minx, miny, maxx, maxy);
                    try
                    {
                        HttpResponseMessage message = await client.GetAsync(finalUrl).ConfigureAwait(false);
                        var XmlString = await message.Content.ReadAsStringAsync();
                        System.Xml.Linq.XDocument doc = new System.Xml.Linq.XDocument();
                        System.IO.StringReader reader = new System.IO.StringReader(XmlString);
                        doc = System.Xml.Linq.XDocument.Load(reader);
                        System.Xml.Linq.XElement root = doc.Root;
                        System.Xml.Linq.XElement ele = root.Element("returnURL");
                        if (ele == null)
                        {
                            _Result.success = false;
                            _Result.errorMessage = root.Value;
                            return _Result;
                        }
                        string Output = ele.Value;
                        message = await client.GetAsync(Output);
                        var CsvString = await message.Content.ReadAsStringAsync();

                        if (CsvString == null || CsvString == "")
                        {
                            _Result.success = false;
                            _Result.errorMessage = "the returned statistics result contained no crops.";
                            return _Result;
                        }
                        _Result.success = true;
                        string[] rows = CsvString.Split(new char[] { '\n' });
                        for (int i = 1; i < rows.Length; i++)
                        {
                            string[] row = rows[i].Split(new char[] { ',' });
                            _Result.rows.Add(new Json.StatisticsRow(Int32.Parse(row[0]), Int32.Parse(row[2]), row[1], "", float.Parse(row[3])));
                        }
                        return _Result;
                    }
                    catch (System.ArgumentNullException ane)
                    {
                        _Result.success = false;
                        _Result.errorMessage = ane.Message;
                        return _Result;
                    }


                }
            }
            //return true;
        }
    }
}
