using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;

namespace CCLLC.CDS.FieldEncryption.Test
{
    using CCLLC.Azure.Secrets;
    using CCLLC.CDS.Test;
    using CCLLC.CDS.Test.Builders;

    [TestClass]
    public class UnitTest1
    {
        #region CreateHandler_Should_EncryptDepartmentField

        [TestMethod]
        public void Test_CreateHandler_Should_EncryptDepartmentField()
        {
            new CreateHandler_Should_EncryptDepartmentField().Test();
        }

        private class CreateHandler_Should_EncryptDepartmentField : TestMethodClassBase
        {
            private struct ExistingIds
            {
                public static readonly Id Contact = new Id<Contact>("{34E01509-DD7D-477C-9E90-D821B15A4B18}");
                public static readonly Id ConfigWebResource = new Id<WebResource>("{268CF1F1-1438-4928-AD1A-9301D0EAD5D5}");
            }

            private struct CreatedIds
            {
                public static readonly Id Contact = new Id<Contact>("{F98C4427-C594-4B58-A41E-1327DC83CF74}");
            }

            private struct TestData
            {
                public static readonly string contactConfig = "<configuration><entity recordType=\"contact\"><field fieldName=\"department\"/></entity></configuration>";
                public static readonly string DecryptedValue = "TestValue";
                public static readonly string EncryptedValue = "n0VGGyXQ5RH9omZWxFD7560EsIxIbTn4szad513eYOo=";
            }


            protected override void InitializeTestData(IOrganizationService service)
            {
                new CrmEnvironmentBuilder()
                    .WithBuilder<WebResourceBuilder>(b => b
                        .WithName("ccllc_/configuration/encryptedfields/contacts.xml")
                        .WithType(WebResource_WebResourceType.Data_XML)
                        .WithContent(TestData.contactConfig))
                    .WithEntities<ExistingIds>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                service = new OrganizationServiceBuilder(service)
                   .WithIdsDefaultedForCreate(
                    CreatedIds.Contact)
                   .Build();

                var resource = service.Retrieve(WebResource.EntityLogicalName, ExistingIds.ConfigWebResource.EntityId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true)).ToEntity<WebResource>();


                Assert.AreEqual("ccllc_/configuration/encryptedfields/contacts.xml", resource.Name);

                var plugin = new Sample.FieldEncryptionPlugin(null, null);

                plugin.Container.Implement<ISecretProviderFactory>().Using<Fakes.FakeSecretProviderFactory<Fakes.FakeSecretProvider>>().WithOverwrite();


                var target = new Contact()
                {
                    Department = TestData.DecryptedValue
                };


                var executionContext = new PluginExecutionContextBuilder()
                        .WithRegisteredEvent(20, "Create", Contact.EntityLogicalName)
                        .WithInputParameter("Target", target)
                        .Build();

                var serviceProvider = new ServiceProviderBuilder(
                    service,
                    executionContext,
                    new DebugLogger()).Build();

                plugin.Execute(serviceProvider);


                var contextTarget = executionContext.InputParameters["Target"] as Contact;

                Assert.AreEqual(TestData.EncryptedValue, contextTarget.Department);
            }
        }

        #endregion CreateHandler_Should_EncryptDepartmentField


        #region UpdateHandler_Should_EncryptDepartmentField

        [TestMethod]
        public void Test_UpdateHandler_Should_EncryptDepartmentField()
        {
            new UpdateHandler_Should_EncryptDepartmentField().Test();
        }

        private class UpdateHandler_Should_EncryptDepartmentField : TestMethodClassBase
        {
            private struct ExistingIds
            {
                public static readonly Id Contact = new Id<Contact>("{34E01509-DD7D-477C-9E90-D821B15A4B18}");
                public static readonly Id ConfigWebResource = new Id<WebResource>("{268CF1F1-1438-4928-AD1A-9301D0EAD5D5}");
            }

            private struct TestData
            {
                public static readonly string contactConfig = "<configuration><entity recordType=\"contact\"><field fieldName=\"department\"/></entity></configuration>";
                public static readonly string DecryptedValue = "TestValue";
                public static readonly string EncryptedValue = "n0VGGyXQ5RH9omZWxFD7560EsIxIbTn4szad513eYOo=";
            }


            protected override void InitializeTestData(IOrganizationService service)
            {
                new CrmEnvironmentBuilder()
                    .WithBuilder<WebResourceBuilder>(b => b
                        .WithName("ccllc_/configuration/encryptedfields/contacts.xml")
                        .WithType(WebResource_WebResourceType.Data_XML)
                        .WithContent(TestData.contactConfig))
                    .WithEntities<ExistingIds>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                service = new OrganizationServiceBuilder(service)
                   .Build();

                var resource = service.Retrieve(WebResource.EntityLogicalName, ExistingIds.ConfigWebResource.EntityId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true)).ToEntity<WebResource>();

                Assert.AreEqual("ccllc_/configuration/encryptedfields/contacts.xml", resource.Name);

                var plugin = new Sample.FieldEncryptionPlugin(null, null);

                plugin.Container.Implement<ISecretProviderFactory>().Using<Fakes.FakeSecretProviderFactory<Fakes.FakeSecretProvider>>().WithOverwrite();


                var target = new Contact()
                {
                    Id = ExistingIds.Contact.EntityId,
                    Department = TestData.DecryptedValue
                };


                var executionContext = new PluginExecutionContextBuilder()
                        .WithRegisteredEvent(20, "Update", Contact.EntityLogicalName)
                        .WithInputParameter("Target", target)
                        .Build();

                var serviceProvider = new ServiceProviderBuilder(
                    service,
                    executionContext,
                    new DebugLogger()).Build();

                plugin.Execute(serviceProvider);


                var contextTarget = executionContext.InputParameters["Target"] as Contact;

                Assert.AreEqual(TestData.EncryptedValue, contextTarget.Department);
            }
        }
    }

    #endregion UpdateHandler_Should_EncryptDepartmentField
}
