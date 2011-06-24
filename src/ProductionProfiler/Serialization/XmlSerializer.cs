using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using ProductionProfiler.Core.Extensions;
using Sys = System.Xml.Serialization;

namespace ProductionProfiler.Core.Serialization
{
    public class XmlSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                Sys.XmlSerializer xmlSerializer = new Sys.XmlSerializer(obj.GetType());

                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)))
                {
                    xmlSerializer.Serialize(xmlTextWriter, obj);
                }

                return stringBuilder.ToString();
            }
            catch (Exception e)
            {
                return "<Error>Failed to serialize type:={0}, Message:={1}</Error>".FormatWith(obj.GetType(), e.Message);
            }
        }

        public DataFormat Format
        {
            get { return DataFormat.Xml; }
        }
    }
}