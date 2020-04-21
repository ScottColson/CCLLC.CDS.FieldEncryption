using CCLLC.Core;

namespace CCLLC.CDS.FieldEncryption
{
    /// <summary>
    /// Factory for creating <see cref="IEncryptedFieldAccessLogger"/>.
    /// </summary>
    public interface IEncryptedFieldAccessLoggerFactory
    {
        IEncryptedFieldAccessLogger Create(IProcessExecutionContext executionContext, bool disableCache = false);
    }
}
