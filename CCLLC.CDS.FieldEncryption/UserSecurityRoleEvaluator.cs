using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace CCLLC.CDS.FieldEncryption
{
    using CCLLC.Core;
    using CCLLC.CDS.Sdk;

    public class UserSecurityRoleEvaluator : IUserRoleEvaluator
    {
        public bool IsAssignedRole(IProcessExecutionContext executionContext, Guid userId, params string[] roles)
        {
            var orgService = executionContext.DataService.ToOrgService();

            if (roles is null || roles.Length <= 0)
            {
                return false;
            }

            // wild card is always true
            if (roles.Contains("*"))
            {
                return true;
            }

            var qry = new QueryExpression
            {
                EntityName = "role",
                ColumnSet = new ColumnSet(new string[] { "name" }),

                LinkEntities = {
                    new LinkEntity
                    {
                        JoinOperator = JoinOperator.Inner,
                        LinkFromEntityName = "role",
                        LinkFromAttributeName = "roleid",
                        LinkToEntityName = "systemuserroles",
                        LinkToAttributeName = "roleid",

                        LinkCriteria = new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression("systemuserid",ConditionOperator.Equal,userId)
                            }
                        }
                    }
                }
            };

            //OR Filter on each role name.
            qry.Criteria = new FilterExpression
            {
                IsQuickFindFilter = false,
                FilterOperator = LogicalOperator.Or,
            };

            foreach (string name in roles)
            {
                qry.Criteria.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, name));
            }

            var result = orgService.RetrieveMultiple(qry);

            // Return true if user has at least one of the roles.
            if (result.Entities.Count > 0)
            {
                return true;
            }


            // Retry query finding roles assigned to teams that the user is a member of.
            qry = new QueryExpression
            {
                EntityName = "role",
                ColumnSet = new ColumnSet(new string[] { "name" }),

                LinkEntities =
                    {
                        new LinkEntity //INNER JOIN TeamRoles
                        {
                            JoinOperator = JoinOperator.Inner,
                            LinkFromEntityName = "role",
                            LinkFromAttributeName = "roleid",
                            LinkToEntityName = "teamroles",
                            LinkToAttributeName = "roleid",

                            LinkEntities =
                            {
                                new LinkEntity //INNER JOIN TeamMembership where user is a team member
                                {
                                    JoinOperator = JoinOperator.Inner,
                                    LinkFromEntityName = "teamroles",
                                    LinkFromAttributeName = "teamid",
                                    LinkToEntityName = "teammembership",
                                    LinkToAttributeName = "teamid",
                                    LinkCriteria = new FilterExpression
                                    {
                                        FilterOperator = LogicalOperator.Or,
                                        Conditions =
                                        {
                                            new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)
                                        }
                                    }
                                }
                            }
                        }
                    }
            };

            //Add role filter with OR clause for one or more matching names. 
            qry.Criteria = new FilterExpression
            {
                IsQuickFindFilter = false,
                FilterOperator = LogicalOperator.Or,
            };

            foreach (string name in roles)
            {
                qry.Criteria.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, name));
            }
                       
            result = orgService.RetrieveMultiple(qry);

            return result.Entities.Count > 0;            
        }

    }
}
