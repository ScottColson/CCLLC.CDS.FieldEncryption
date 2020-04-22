using System;

namespace CCLLC.CDS.FieldEncryption
{
    using CCLLC.Core;

    /// <summary>
    /// Defines required settings values for Field Encryption.
    /// </summary>
    public interface IEncryptedFieldSettings : ISettingsProvider
    {
        /// <summary>
        /// The virtual CDS webresource path where the entity and field
        /// encryption configuration data is stored as XML Data resources.
        /// </summary>
        string ConfigurationDataPath { get; }

        /// <summary>
        /// The name used to retrieve the encryption key secret from the 
        /// key vault.
        /// </summary>
        string EncryptionKeyName { get; }

        /// <summary>
        /// The time span that an existing <see cref="IEncryptedFieldService"/> object will
        /// exist in the <see cref="IProcessExecutionContext.Cache"/> after creation. 
        /// </summary>
        TimeSpan? CacheTimeout { get; }
    }
}
