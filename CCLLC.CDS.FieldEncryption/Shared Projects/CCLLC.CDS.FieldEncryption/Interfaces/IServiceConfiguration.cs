using System.Collections.Generic;
using System.Xml.Linq;

namespace CCLLC.CDS.FieldEncryption
{
    /// <summary>
    /// Configuration information for the <see cref="IEncryptedFieldService"/>. Created from data stored in
    /// CDS XML Data resources.
    /// </summary>
    public interface IServiceConfiguration
    {        
        /// <summary>
        /// Defines an optional global RegEx pattern used to remove any unwanted formatting from a field that 
        /// will be encrypted before actually encrypting the data. Removing formating makes searching more 
        /// effective.
        /// </summary>
        string GlobalFieldPrepPattern { get; }

        string GlobalUnmaskTriggerAttributeName { get; }

        /// <summary>
        /// Text pattern at the beginning of a search query match value that indicates that the
        /// user is searching against encrypted fields.
        /// </summary>
        string EncryptedSearchTrigger { get; }

        IEnumerable<IRecordConfiguration> RecordConfigurations { get; }

        IRecordConfiguration RecordConfiguration(string recordType);

        void AddConfiguration(XDocument xmlConfiguration);

    }
}
