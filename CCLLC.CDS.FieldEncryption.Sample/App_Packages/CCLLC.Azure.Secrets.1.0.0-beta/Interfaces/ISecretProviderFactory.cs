namespace CCLLC.Azure.Secrets
{
    using CCLLC.Core;

    public interface ISecretProviderFactory
    {
        ISecretProvider Create(IProcessExecutionContext executionContext, bool disableCache = false);
    }
}
