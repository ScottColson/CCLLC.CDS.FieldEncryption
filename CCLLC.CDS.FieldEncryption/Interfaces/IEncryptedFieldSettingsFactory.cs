namespace CCLLC.CDS.FieldEncryption
{
    using CCLLC.Core;

    /// <summary>
    /// Factory to create <see cref="IEncryptedFieldSettings"/>.
    /// </summary>
    public interface IEncryptedFieldSettingsFactory
    {
        IEncryptedFieldSettings Create(IProcessExecutionContext executionContext);       
    }
}
