using System;
using System.Linq;
using System.Xml.Linq;

namespace CCLLC.CDS.FieldEncryption
{
    internal class FieldConfiguration : IFieldConfiguration
    {
        public string FieldName { get; }

        public string UnmaskTriggerAttributeName { get; }

        public bool DisableFieldPrep { get; }

        public string FieldPrepPattern { get; }

        public string FullyMaskedFormat { get; }

        public string[] PartiallyMaskedViewRoles { get; }

        public string PartiallyMaskedFormat { get; }

        public string PartiallyMaskedPattern { get; }

        public string[] UnmaskedViewRoles { get; }

        public string UnmaskedFormat { get; }

        public string UnmaskedPattern { get; }

        public string MatchPattern { get; }

        public string InvalidMatchMessage { get; }

        public string[] SearchRoles { get; }

        public bool AllowInactiveRecordSearch { get; }

        internal FieldConfiguration(XElement configurationData)
        {
            this.FieldName = configurationData.Attribute("fieldName")?.Value ?? throw new ArgumentNullException("Encrypted Field configuration data is missing required 'fieldName' attribute");

            this.UnmaskTriggerAttributeName = configurationData.Attribute("unmaskTriggerAttribute")?.Value;

            this.DisableFieldPrep = (configurationData.Attribute("disableFieldPrep")?.Value.ToLower() == "true");

            this.FieldPrepPattern = configurationData.Attribute("fieldPrepPattern")?.Value;

            this.FullyMaskedFormat = configurationData.Attribute("fullyMaskedFormat")?.Value ?? "*********";

            this.PartiallyMaskedFormat = configurationData.Attribute("partiallyMaskedFormat")?.Value ?? "*********";

            this.PartiallyMaskedPattern = configurationData.Attribute("partiallyMaskedPattern")?.Value;

            this.PartiallyMaskedViewRoles = configurationData.Attribute("partiallyMaskedViewRoles")?.Value
                .Split(',').Select(s => s.Trim()).ToArray()
                ?? new string[] { "*" };

            this.UnmaskedFormat = configurationData.Attribute("unmaskedFormat")?.Value;

            this.UnmaskedPattern = configurationData.Attribute("unmaskedPattern")?.Value;

            this.UnmaskedViewRoles = configurationData.Attribute("unmaskedViewRoles")?.Value
                .Split(',').Select(s => s.Trim()).ToArray()
                ?? new string[] { "*" };

            this.MatchPattern = configurationData.Attribute("matchPattern")?.Value ?? "*";

            this.InvalidMatchMessage = configurationData.Attribute("invalidMatchMessage")?.Value ?? string.Format("Input field {0} does not match the required format.", this.FieldName);

            this.SearchRoles = configurationData.Attribute("searchRoles")?.Value
                .Split(',').Select(s => s.Trim()).ToArray() 
                ?? new string[] { "*" };

            this.AllowInactiveRecordSearch = (configurationData.Attribute("inactiveRecordSearch")?.Value.ToLower() == "true");
        }
    }
}
