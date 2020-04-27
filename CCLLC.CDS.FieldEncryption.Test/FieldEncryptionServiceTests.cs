using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;

namespace CCLLC.CDS.FieldEncryption.Test
{
    using CCLLC.Core;
    using CCLLC.Azure.Secrets;
    using CCLLC.CDS.Test;
    using CCLLC.CDS.Test.Builders;

    [TestClass]
    public class FieldEncryptionServiceTests
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
               
                var plugin = new Sample.FieldEncryptionPlugin(null, null);

                plugin.Container.Implement<ICache>().Using<Fakes.FakeCacheProvider>().WithOverwrite();
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

                var plugin = new Sample.FieldEncryptionPlugin(null, null);

                plugin.Container.Implement<ICache>().Using<Fakes.FakeCacheProvider>().WithOverwrite();
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

        #endregion UpdateHandler_Should_EncryptDepartmentField



        #region RetrieveHandler_Should_GenerateMaskingInstructions

        [TestMethod]
        public void Test_RetrieveHandler_Should_GenerateMaskingInstructions()
        {
            new RetrieveHandler_Should_GenerateMaskingInstructions().Test();
        }

        private class RetrieveHandler_Should_GenerateMaskingInstructions : TestMethodClassBase
        {
            private struct ExistingIds
            {
                public static readonly Id Contact = new Id<Contact>("{6B7BC6CE-69DD-4D70-B42B-E2C632A91E11}");
                public static readonly Id ConfigWebResource = new Id<WebResource>("{510977C4-A0DE-4658-8B9B-C56A21BF4F95}");
            }

            private struct TestData
            {
                public static readonly string contactConfig = "<configuration><entity recordType=\"contact\"><field fieldName=\"department\" unmaskTriggerAttribute=\"*\"/></entity></configuration>";
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
                    .WithBuilder<ContactBuilder>(b => b
                        .WithAttributeValue("department", TestData.EncryptedValue))
                    .WithEntities<ExistingIds>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                service = new OrganizationServiceBuilder(service)
                   .Build();

                var plugin = new Sample.FieldEncryptionPlugin(null, null);

                plugin.Container.Implement<ICache>().Using<Fakes.FakeCacheProvider>().WithOverwrite();
                plugin.Container.Implement<ISecretProviderFactory>().Using<Fakes.FakeSecretProviderFactory<Fakes.FakeSecretProvider>>().WithOverwrite();


                var target = new Contact()
                {
                    Id = ExistingIds.Contact.EntityId,
                    Department = TestData.EncryptedValue
                };

                var executionContext = new PluginExecutionContextBuilder()
                        .WithRegisteredEvent(20, "Retrieve", Contact.EntityLogicalName)
                        .WithInputParameter("Target", target.ToEntityReference())
                        .WithInputParameter("ColumnSet", new ColumnSet("department"))
                        .WithOutputParameter("Entity", target)
                        .Build();

                var serviceProvider = new ServiceProviderBuilder(
                    service,
                    executionContext,
                    new DebugLogger()).Build();

                plugin.Execute(serviceProvider);

                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));


                var maskingInstructions = context.SharedVariables["CCLLC.EncryptedFieldService.DecryptColumns"] as Dictionary<string, EncryptedFieldService.MaskingInstruction>;

                Assert.AreEqual(1, maskingInstructions.Count);
                Assert.IsTrue(maskingInstructions.ContainsKey("department"));
                Assert.AreEqual(EncryptedFieldService.MaskingInstruction.Unmask, maskingInstructions["department"]);

            }
        }

        #endregion RetrieveHandler_Should_GenerateMaskingInstructions



        #region RetrieveHandler_Should_RemoveDecryptionTriggerField

        [TestMethod]
        public void Test_RetrieveHandler_Should_RemoveDecryptionTriggerField()
        {
            new RetrieveHandler_Should_RemoveDecryptionTriggerField().Test();
        }

        private class RetrieveHandler_Should_RemoveDecryptionTriggerField : TestMethodClassBase
        {
            private struct ExistingIds
            {
                public static readonly Id Contact = new Id<Contact>("{FDC79A0A-BF58-457C-80FC-A6B500D61D5D}");
                public static readonly Id ConfigWebResource = new Id<WebResource>("{624AD0D2-EE45-4A26-A440-218424B6D116}");
            }

            private struct TestData
            {
                // Decrypt field department using trigger field "telephone1"
                public static readonly string contactConfig = "<configuration><entity recordType=\"contact\"><field fieldName=\"department\" unmaskTriggerAttribute=\"telephone1\"/></entity></configuration>";
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
                    .WithBuilder<ContactBuilder>(b => b
                        .WithAttributeValue("department", TestData.EncryptedValue))
                    .WithEntities<ExistingIds>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                service = new OrganizationServiceBuilder(service)
                   .Build();

                var plugin = new Sample.FieldEncryptionPlugin(null, null);

                plugin.Container.Implement<ICache>().Using<Fakes.FakeCacheProvider>().WithOverwrite();
                plugin.Container.Implement<ISecretProviderFactory>().Using<Fakes.FakeSecretProviderFactory<Fakes.FakeSecretProvider>>().WithOverwrite();


                var target = new Contact()
                {
                    Id = ExistingIds.Contact.EntityId,
                    Department = TestData.EncryptedValue
                };

                var executionContext = new PluginExecutionContextBuilder()
                        .WithRegisteredEvent(20, "Retrieve", Contact.EntityLogicalName)
                        .WithInputParameter("Target", target.ToEntityReference())
                        .WithInputParameter("ColumnSet", new ColumnSet("department","telephone1"))
                        .WithOutputParameter("Entity", target)
                        .Build();

                var serviceProvider = new ServiceProviderBuilder(
                    service,
                    executionContext,
                    new DebugLogger()).Build();

                plugin.Execute(serviceProvider);

                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                var columns = context.InputParameters["ColumnSet"] as ColumnSet;

                Assert.AreEqual(1, columns.Columns.Count);
                Assert.IsTrue(columns.Columns.Contains("department"));
            }
        }

        #endregion RetrieveHandler_Should_RemoveDecryptionTriggerField



        #region RetrieveHandler_Should_DecryptDepartmentField

        [TestMethod]
        public void Test_RetrieveHandler_Should_DecryptDepartmentField()
        {
            new RetrieveHandler_Should_DecryptDepartmentField().Test();
        }

        private class RetrieveHandler_Should_DecryptDepartmentField : TestMethodClassBase
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
                    .WithBuilder<ContactBuilder>(b => b
                        .WithAttributeValue("department", TestData.EncryptedValue))
                    .WithEntities<ExistingIds>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                service = new OrganizationServiceBuilder(service)
                   .Build();

                var plugin = new Sample.FieldEncryptionPlugin(null, null);
                
                plugin.Container.Implement<ICache>().Using<Fakes.FakeCacheProvider>().WithOverwrite();
                plugin.Container.Implement<ISecretProviderFactory>().Using<Fakes.FakeSecretProviderFactory<Fakes.FakeSecretProvider>>().WithOverwrite();


                plugin.Container.Implement<ISecretProviderFactory>().Using<Fakes.FakeSecretProviderFactory<Fakes.FakeSecretProvider>>().WithOverwrite();


                var target = new Contact()
                {
                    Id = ExistingIds.Contact.EntityId,
                    Department = TestData.EncryptedValue
                };

                var maskingInstructions = new Dictionary<string, EncryptedFieldService.MaskingInstruction>();
                maskingInstructions.Add("department", EncryptedFieldService.MaskingInstruction.Unmask);


                var executionContext = new PluginExecutionContextBuilder()
                        .WithRegisteredEvent(40, "Retrieve", Contact.EntityLogicalName)
                        .WithInputParameter("Target", target.ToEntityReference())
                        .WithInputParameter("ColumnSet", new ColumnSet("display"))
                        .WithOutputParameter("Entity", target)
                        .WithSharedVariable("CCLLC.EncryptedFieldService.DecryptColumns", maskingInstructions)
                        .Build();

                var serviceProvider = new ServiceProviderBuilder(
                    service,
                    executionContext,
                    new DebugLogger()).Build();

                plugin.Execute(serviceProvider);


                var contextTarget = executionContext.OutputParameters["Entity"] as Contact;

                Assert.AreEqual(TestData.DecryptedValue, contextTarget.Department);
            }
        }

        #endregion RetrieveHandler_Should_DecryptDepartmentField

    }


}
