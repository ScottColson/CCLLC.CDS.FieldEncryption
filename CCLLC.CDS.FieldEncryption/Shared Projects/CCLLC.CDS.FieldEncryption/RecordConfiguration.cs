using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CCLLC.CDS.FieldEncryption
{
    internal class RecordConfiguration : IRecordConfiguration
    {
        private readonly List<IFieldConfiguration> configuredFields;
        public string RecordType { get; }

        public IReadOnlyList<IFieldConfiguration> ConfiguredFields => configuredFields;

        public string UnmaskTriggerAttributeName { get; }

        internal RecordConfiguration(XElement configurationData)
        {           
            this.RecordType = configurationData.Attribute("recordType")?.Value 
                ?? throw new Exception("Encrypted Field Record Type configuration is missing required record type attribute.");

            this.UnmaskTriggerAttributeName = configurationData.Attribute("unmaskTriggerAttribute")?.Value;
           
            configuredFields = new List<IFieldConfiguration>();
            foreach (XElement el in configurationData.Descendants("field"))
            {
                configuredFields.Add(new FieldConfiguration(el));
            }
        }
       
        public override int GetHashCode()
        {
            return this.RecordType.GetHashCode();
        }

        public IFieldConfiguration ConfiguredField(string name)
        {
            return configuredFields.Where(f => f.FieldName == name).FirstOrDefault();
        }
    }
}
