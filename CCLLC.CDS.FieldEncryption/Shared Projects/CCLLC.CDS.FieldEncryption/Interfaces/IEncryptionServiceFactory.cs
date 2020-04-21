using CCLLC.Core;

namespace CCLLC.CDS.FieldEncryption
{
    public interface IEncryptionServiceFactory 
    {
        IEncryptionService Create(IProcessExecutionContext executionContext, bool disableCache = false);
    }
}
