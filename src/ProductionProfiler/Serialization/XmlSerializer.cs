using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Sys = System.Xml.Serialization;

namespace ProductionProfiler.Core.Serialization
{
    public class XmlSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Sys.XmlSerializer xmlSerializer = new Sys.XmlSerializer(obj.GetType());

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)))
            {
                xmlSerializer.Serialize(xmlTextWriter, obj);
            }

            return stringBuilder.ToString();
        }

        public DataFormat Format
        {
            get { return DataFormat.Xml; }
        }
    }
}