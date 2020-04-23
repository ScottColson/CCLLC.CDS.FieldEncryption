using System;
using System.Collections.Generic;
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;
using Microsoft.Xrm.Sdk;
using CCLLC.Core;


namespace CCLLC.CDS.Test
{
    public abstract class TestMethodClassBase : TestMethodClassBaseDLaB
    {
        public class AssociationInfo
        {
            public string EntityName { get; set; }
            public Guid Id { get; set; }
            public Relationship Relationship { get; set; }
            public EntityReferenceCollection RelatedEntities { get; set; }
        }

        protected IIocContainer Container { get; }
        protected List<AssociationInfo> CreatedAssociations { get; }

        public TestMethodClassBase() : base()
        {
            Container = new IocContainer();
            CreatedAssociations = new List<AssociationInfo>();
        }

        protected override IAgnosticServiceBuilder GetOrganizationServiceBuilder(IOrganizationService service) { return new Builders.OrganizationServiceBuilder(service); }


        protected override void LoadConfigurationSettings()
        {
            TestInitializer.InitializeTestSettings();
        }

      
        public void Test()
        {           
                Test(new DebugLogger());           
        }


        protected override IOrganizationService GetIOrganizationService()
        {
            return new Builders.OrganizationServiceBuilder(base.GetIOrganizationService())
                .WithAssociationLogger(associationLogger)
                .Build();            
        }

        protected override void CleanupTestData(IOrganizationService service, bool shouldExist)
        {
            if (shouldExist)
            {
                foreach(var a in CreatedAssociations)
                {
                    service.Disassociate(a.EntityName, a.Id, a.Relationship, a.RelatedEntities);
                }
            }
            base.CleanupTestData(service, shouldExist);
        }

         private void associationLogger(IOrganizationService service, string entityName, Guid id, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            CreatedAssociations.Add(
                new AssociationInfo
                {
                    EntityName = entityName,
                    Id = id,
                    Relationship = relationship,
                    RelatedEntities = relatedEntities
                });
        }
    }
}
