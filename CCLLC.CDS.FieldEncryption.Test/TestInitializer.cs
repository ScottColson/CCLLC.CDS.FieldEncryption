using DLaB.Xrm.Test;
using DLaB.Xrm.Client;
using CCLLC.CDS.Test.Builders;
using Microsoft.Xrm.Sdk;

namespace CCLLC.CDS.Test
{
    /// <summary>
    /// Class to Initialize all TestSettings used by the Framework
    /// </summary>
    public class TestInitializer
    {
        /// <summary>
        /// Initializes the test settings.
        /// </summary>
        public static void InitializeTestSettings()
        {
            if (!TestSettings.AssumptionXmlPath.IsConfigured)
            {
               TestSettings.AssumptionXmlPath.Configure(new PatherFinderProjectOfType(typeof(MsTestProvider), "Assumptions\\Entity Xml"));
            }
            if (!TestSettings.UserTestConfigPath.IsConfigured)
            {
                TestSettings.UserTestConfigPath.Configure(new PatherFinderProjectOfType(typeof(MsTestProvider), "UnitTestSettings.user.config"));
            }
            if (!TestSettings.EntityBuilder.IsConfigured)
            {
                TestSettings.EntityBuilder.ConfigureDerivedAssembly<AccountBuilder>();
            }
            if (!TestSettings.EarlyBound.IsConfigured)
            {
                TestSettings.EarlyBound.ConfigureDerivedAssembly<CrmServiceContext>();
                CrmServiceUtility.GetEarlyBoundProxyAssembly(TestSettings.EarlyBound.Assembly);
            }
            if (!TestSettings.TestFrameworkProvider.IsConfigured)
            {
                TestSettings.TestFrameworkProvider.Configure(new MsTestProvider());
            }
        }
    }
}
