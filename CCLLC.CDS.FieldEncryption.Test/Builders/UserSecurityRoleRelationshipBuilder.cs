using DLaB.Xrm.Test;

namespace CCLLC.CDS.Test.Builders
{    
    public class UserSecurityRoleRelationshipBuilder : RelationshipBuilder
    {
        public UserSecurityRoleRelationshipBuilder(Id id) : 
            base("systemuserroles", id)
        {
        }
    }
}
