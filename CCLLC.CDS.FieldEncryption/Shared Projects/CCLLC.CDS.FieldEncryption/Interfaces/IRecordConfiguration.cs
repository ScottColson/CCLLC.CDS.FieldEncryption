using System;
using System.Collections.Generic;
using System.Text;

namespace CCLLC.CDS.FieldEncryption
{
    public interface IRecordConfiguration
    {
        /// <summary>
        /// The logical name or type of the record containing fields that require encryption.
        /// </summary>
        string RecordType { get; }

        string UnmaskTriggerAttributeName { get; }

        IReadOnlyList<IFieldConfiguration> ConfiguredFields { get; }

        IFieldConfiguration ConfiguredField(string name);
    }
}
