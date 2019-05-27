using System.Net.Http;
using System.Threading.Tasks;

namespace Agriculture.Nass
{
    public class GeoTiff
    {
        private const string _baseUrl = "http://nassgeodata.gmu.edu/axis2/services/CDLService/GetCDLFile?";
        private string CreateBboxFileRequestString(string year, double minx, double miny, double maxx, double maxy)
        {
            return _baseUrl + "year=" + year + "&bbox=" + minx + "," + miny + "," + maxx + "," + maxy;
        }
        public async Task<bool> RequestFileForBoundingBox(string outputDestination, string year, double minx, double miny, double maxx, double maxy)
        {
            using (var handler = new WebRequestHandler())
            {
                handler.ServerCertificateValidationCallback = (obj, cert, chain, sslpe) =>
                {
                    return true;
                };
                using (var client = new HttpClient(handler))
                {
                    var message = await client.GetAsync(CreateBboxFileRequestString(year, minx, miny, maxx, maxy)).ConfigureAwait(false);
                    
                    var xmlString = await message.Content.ReadAsStringAsync();
                    System.Xml.Linq.XDocument doc = new System.Xml.Linq.XDocument();

                    System.IO.StringReader reader = new System.IO.StringReader(xmlString);
                    doc = System.Xml.Linq.XDocument.Load(reader);
                    System.Xml.Linq.XElement root = doc.Root;
                    System.Xml.Linq.XElement ele = root.Element("returnURL");
                    if (ele == null)
                    {
                        return false;
                    }
                    string Output = ele.Value;
                    var fileStream = await client.GetStreamAsync(Output);
                    using (var outputStream = System.IO.File.Create(outputDestination))
                    {
                        //fileStream.Seek(0, System.IO.SeekOrigin.Begin);
                        fileStream.CopyTo(outputStream);
                    }


                }
            }
            return true;
        }
    }
}
