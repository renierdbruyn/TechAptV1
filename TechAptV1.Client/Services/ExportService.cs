// Copyright © 2025 Always Active Technologies PTY Ltd

using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
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
        /// <summary>
        /// Fetches all data from the numbers table and serialized it as xml.
        /// </summary>
        /// <returns></returns>
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
                return Encoding.Default.GetBytes(stream.ToString());
            }
        }

        public byte[] GetBinBytes()
        {
            var data = _dataService.GetAll();
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
