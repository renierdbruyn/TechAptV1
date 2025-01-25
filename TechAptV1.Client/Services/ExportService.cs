// Copyright © 2025 Always Active Technologies PTY Ltd

using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using TechAptV1.Client.Models;

namespace TechAptV1.Client.Services
{
    public class ExportService
    {
        private readonly DataService _dataService;
        public ExportService(DataService dataService)
        {
            _dataService = dataService;
        }

        public byte[] GetXmLBytes()
        {
            var data = _dataService.GetAll();

            var numbersXml = data.Select(x => new XElement("Numbers", new XAttribute("Value", x.Value), new XAttribute("IsPrime", x.IsPrime)));
            var bodyXml = new XElement("Numbers", numbersXml);

            // Define the root element to avoid ArrayOfBranch
            var serializer = new XmlSerializer(typeof(List<Number>),
                                               new XmlRootAttribute("Numbers"));
            using (var stream = new StringWriter())
            {
                serializer.Serialize(stream, data);
                System.Console.Write(stream.ToString());
            byte[] bytes = Encoding.Default.GetBytes(stream.ToString());
            return bytes;
            }
        }
    }
}
