using CCLLC.Core;

namespace CCLLC.CDS.FieldEncryption
{
    /// <summary>
    /// Creates a new <see cref="EncryptedFieldSettings"/> object that wraps the 
    /// <see cref="IProcessExecutionContext.Settings"/> object to provide setting
    /// defaults and define override keys.
    /// </summary>
    public class EncryptedFieldSettingsFactory : IEncryptedFieldSettingsFactory
    {
        public IEncryptedFieldSettings Create(IProcessExecutionContext executionContext)
        {
            return new EncryptedFieldSettings(executionContext.Settings);
        }
    }
}
