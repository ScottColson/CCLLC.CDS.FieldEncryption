namespace CCLLC.CDS.FieldEncryption
{
    /// <summary>
    /// Provides an implementation of the default field service factory functionality that does not require
    /// the container to have an access logger factory defined. 
    /// </summary>
    public class EncryptedFieldServiceFactoryNoLogging : EncryptedFieldServiceFactory
    {
        public EncryptedFieldServiceFactoryNoLogging(IEncryptedFieldSettingsFactory settingsFactory, IEncryptionServiceFactory encryptionServiceFactory, IUserRoleEvaluator roleEvaluator) 
            : base(settingsFactory, encryptionServiceFactory, roleEvaluator, null)
        {
        }
    }
}
