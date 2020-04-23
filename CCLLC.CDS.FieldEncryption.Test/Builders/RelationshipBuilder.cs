using Microsoft.Xrm.Sdk;
using DLaB.Xrm.Test;

namespace CCLLC.CDS.Test.Builders
{
    public abstract class RelationshipBuilder 
    {        
        protected Relationship Relationship { get; private set; }
        protected EntityReferenceCollection RelatedEntities { get; private set; }
        protected EntityReference Target { get; private set; }

        public RelationshipBuilder(string relationshipName, Id id)
        {            
            Relationship = new Relationship(relationshipName);
            RelatedEntities = new EntityReferenceCollection();
            Target = id;
        }
        
        public RelationshipBuilder RelateTo(Id id)
        {
            RelatedEntities.Add(id);
            return this;
        }

        public void Associate(IOrganizationService service)
        {
            service.Associate(Target.LogicalName,Target.Id, Relationship, RelatedEntities);
        }

        public void Disassociate(IOrganizationService service)
        {
            service.Disassociate(Target.LogicalName, Target.Id, Relationship, RelatedEntities);
        }
    }
}
