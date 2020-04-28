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
    using CCLLC.CDS.Sdk;
    using CCLLC.Azure.Secrets;
    using CCLLC.CDS.Test;
    using CCLLC.CDS.Test.Builders;

    [TestClass]
    public class SearchQueryTests
    {
        #region EncryptedQueryPrep_Should_IgnoreQueryWithoutLikeCondition

        [TestMethod]
        public void Test_EncryptedQueryPrep_Should_IgnoreQueryWithoutLikeCondition()
        {
            new EncryptedQueryPrep_Should_IgnoreQueryWithoutLikeCondition().Test();
        }   

        private class EncryptedQueryPrep_Should_IgnoreQueryWithoutLikeCondition : TestMethodClassBase
        {
            private struct ExistingIds
            {
                public static readonly Id Contact = new Id<Contact>("{1B922CFA-4E1F-4491-847F-EE285E1F5991}");
                public static readonly Id ConfigWebResource = new Id<WebResource>("{9CAD85C2-BED0-418E-83BD-4B6162C63C16}");
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

                var testQry = new QueryExpressionBuilder<Contact>()
                    .Select("department")
                    .WhereAll(e => e
                        .IsActive()
                        .Attribute("fullname").IsEqualTo( "#" + TestData.DecryptedValue + "%"))
                    .Build();


                var executionContext = new PluginExecutionContextBuilder()
                        .WithRegisteredEvent(20, "RetrieveMultiple", Contact.EntityLogicalName)
                        .WithInputParameter("Query", testQry)
                        .Build();

                var serviceProvider = new ServiceProviderBuilder(
                    service,
                    executionContext,
                    new DebugLogger()).Build();

                plugin.Execute(serviceProvider);

                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                var outQuery = context.InputParameters["Query"] as QueryExpression;

                var criteria = outQuery.Criteria;

                Assert.AreEqual(2, criteria.Conditions.Count);
                Assert.AreEqual(LogicalOperator.And, criteria.FilterOperator);

            }
        }

        #endregion EncryptedQueryPrep_Should_IgnoreQueryWithoutLikeCondition


        #region EncryptedQueryPrep_Should_IgnoreQueryWithoutEncryptedSearchTag

        [TestMethod]
        public void Test_EncryptedQueryPrep_Should_IgnoreQueryWithoutEncryptedSearchTag()
        {
            new EncryptedQueryPrep_Should_IgnoreQueryWithoutEncryptedSearchTag().Test();
        }

        private class EncryptedQueryPrep_Should_IgnoreQueryWithoutEncryptedSearchTag : TestMethodClassBase
        {
            private struct ExistingIds
            {
                public static readonly Id Contact = new Id<Contact>("{1B922CFA-4E1F-4491-847F-EE285E1F5991}");
                public static readonly Id ConfigWebResource = new Id<WebResource>("{9CAD85C2-BED0-418E-83BD-4B6162C63C16}");
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

                var testQry = new QueryExpressionBuilder<Contact>()
                    .Select("department")
                    .WhereAll(e => e
                        .IsActive()
                        .Attribute("fullname").Is( ConditionOperator.Like, TestData.DecryptedValue + "%"))
                    .Build();


                var executionContext = new PluginExecutionContextBuilder()
                        .WithRegisteredEvent(20, "RetrieveMultiple", Contact.EntityLogicalName)
                        .WithInputParameter("Query", testQry)
                        .Build();

                var serviceProvider = new ServiceProviderBuilder(
                    service,
                    executionContext,
                    new DebugLogger()).Build();

                plugin.Execute(serviceProvider);

                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                var outQuery = context.InputParameters["Query"] as QueryExpression;

                var criteria = outQuery.Criteria;

                Assert.AreEqual(2, criteria.Conditions.Count);
                Assert.AreEqual(LogicalOperator.And, criteria.FilterOperator);

            }
        }

        #endregion EncryptedQueryPrep_Should_IgnoreQueryWithoutEncryptedSearchTag


        #region EncryptedQueryPrep_Should_ModifyQueryWithLikeAndEncryptedSearchTag

        [TestMethod]
        public void Test_EncryptedQueryPrep_Should_ModifyQueryWithLikeAndEncryptedSearchTag()
        {
            new EncryptedQueryPrep_Should_ModifyQueryWithLikeAndEncryptedSearchTag().Test();
        }

        private class EncryptedQueryPrep_Should_ModifyQueryWithLikeAndEncryptedSearchTag : TestMethodClassBase
        {
            private struct ExistingIds
            {
                public static readonly Id Contact = new Id<Contact>("{EAEF67B5-65EC-4AD9-934E-7987649C36BB}");
                public static readonly Id ConfigWebResource = new Id<WebResource>("{92DBBD60-61F4-413C-8894-4E7771F95139}");
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

                var testQry = new QueryExpressionBuilder<Contact>()
                    .Select("department")
                    .WhereAll(e => e
                        .IsActive()
                        .WhereAll(e1 => e1
                            .Attribute("fullname").Is(ConditionOperator.Like, "# " + TestData.DecryptedValue + "%")))
                    .Build();


                var executionContext = new PluginExecutionContextBuilder()
                        .WithRegisteredEvent(20, "RetrieveMultiple", Contact.EntityLogicalName)
                        .WithInputParameter("Query", testQry)
                        .Build();

                var serviceProvider = new ServiceProviderBuilder(
                    service,
                    executionContext,
                    new DebugLogger()).Build();

                plugin.Execute(serviceProvider);

                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // get query and verify that it is modified as expected.
                var outQuery = context.InputParameters["Query"] as QueryExpression;

                var criteria = outQuery.Criteria;

                Assert.AreEqual(LogicalOperator.Or, criteria.FilterOperator);
                Assert.AreEqual(0, criteria.Conditions.Count);
                Assert.AreEqual(1, criteria.Filters.Count);

                // check sub filter
                criteria = criteria.Filters[0];

                Assert.AreEqual(LogicalOperator.And, criteria.FilterOperator);
                Assert.AreEqual(2, criteria.Conditions.Count);
                Assert.AreEqual(0, criteria.Filters.Count);

                Assert.AreEqual("department", criteria.Conditions[0].AttributeName);
                Assert.AreEqual(TestData.EncryptedValue, criteria.Conditions[0].Values[0]);

                Assert.AreEqual("statecode", criteria.Conditions[1].AttributeName);
                Assert.AreEqual(0, criteria.Conditions[1].Values[0]);

            }
        }

        #endregion EncryptedQueryPrep_Should_ModifyQueryWithLikeAndEncryptedSearchTag

    }
}
