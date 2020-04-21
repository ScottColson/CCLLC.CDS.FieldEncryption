using System;
using System.Collections.Generic;
using CCLLC.Core;

namespace CCLLC.CDS.FieldEncryption
{
    /// <summary>
    /// Extends the default <see cref="SettingsProvider"/> implementation to provide default values for
    /// all defined settings and setting key values that can be used to override those defaults. This 
    /// class is typically used to wrap an existing <see cref="ISettingsProvider"/> passed in as part of
    /// an execution context.
    /// </summary>
    public class EncryptedFieldSettings : SettingsProvider, IEncryptedFieldSettings
    {
        public EncryptedFieldSettings(IReadOnlyDictionary<string, string> settings)
            : base(settings) { }
        
        public string ConfigurationDataPath => 
            this.GetValue<string>("CCLLC.EncryptedFields.DataPath","ccllc_/configuration/encryptedfields/");

        public TimeSpan? CacheTimeout => 
            this.GetValue<TimeSpan?>("CCLLC.EncryptedFields.CacheTimeout", TimeSpan.FromMinutes(15));

        public string EncryptionKeyName => 
            this.GetValue<string>("CCLLC.EncryptedFields.KeyName", "CDSFieldEncryptionKey");
    }
}
