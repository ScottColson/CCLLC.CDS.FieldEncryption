using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CCLLC.CDS.FieldEncryption
{
    internal class ServiceConfiguration : IServiceConfiguration
    {
        private readonly HashSet<IRecordConfiguration> recordConfigurations;
       
        public string GlobalFieldPrepPattern { get; private set; }

        public string GlobalUnmaskTriggerAttributeName { get; private set; }

        public string EncryptedSearchTrigger { get; private set; }

        public IEnumerable<IRecordConfiguration> RecordConfigurations => recordConfigurations;

        internal ServiceConfiguration()
        {
            recordConfigurations = new HashSet<IRecordConfiguration>();
            GlobalFieldPrepPattern = "";
            GlobalUnmaskTriggerAttributeName = "ccllc_decryptthis";
            EncryptedSearchTrigger = "#";
        }       

        public void AddConfiguration(XDocument xmlConfiguration)
        {
           
            // Process and setting elements in the configuration data to override 
            // global configuration defaults.
            foreach (XElement el in xmlConfiguration.Descendants("setting"))
            {
                
                var settingName = el.Attribute("name")?.Value.ToLower();
                var settingValue = el.Attribute("value")?.Value;

                //set the appropriate attribute of the class based on the setting name in the config file.
                if (!string.IsNullOrEmpty(settingName) && !string.IsNullOrEmpty(settingValue))
                {
                    switch (settingName)
                    {
                        case "unmasktriggerattribute":
                            this.GlobalUnmaskTriggerAttributeName = settingValue;
                            break;

                        case "fieldpreppattern":
                            this.GlobalFieldPrepPattern = settingValue;
                            break;

                        case "searchtrigger":
                            this.EncryptedSearchTrigger = settingValue;
                            break;
                    }
                }
            }

            foreach (XElement el in xmlConfiguration.Descendants("entity"))
            {
                recordConfigurations.Add(new RecordConfiguration(el));
            }
        }

        public IRecordConfiguration RecordConfiguration(string recordType)
        {
            return this.RecordConfigurations.Where(r => r.RecordType == recordType).FirstOrDefault();
        }
    }
}
