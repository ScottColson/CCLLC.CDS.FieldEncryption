using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using DLaB.Xrm.Test;

namespace CCLLC.CDS.Test.Builders
{
    /// <summary>
    /// Class to simplify the simplest cases of creating entities without changing the defaults.
    /// </summary>
    public class CrmEnvironmentBuilder : DLaB.Xrm.Test.Builders.CrmEnvironmentBuilderBase<CrmEnvironmentBuilder>
    {
        protected override CrmEnvironmentBuilder This => this;
        protected RelationshipBuilderManager Relationships { get; }

        public CrmEnvironmentBuilder() : base()
        {
            Relationships = new RelationshipBuilderManager();
        }
        #region Fluent Methods

        public CrmEnvironmentBuilder WithRelationship<TBuilder>(Id id, Action<TBuilder> action) where TBuilder : RelationshipBuilder
        {
            Relationships.AddBuilderForTargetRecord<TBuilder>(id, action);
            return this;
        }

        #endregion Fluent Methods

        new public Dictionary<Guid, Entity> Create(IOrganizationService service)
        {
            var baseReturn = base.Create(service);
            Relationships.Build(service);
            return baseReturn;
        }

        public void RemoveRelationships(IOrganizationService service)
        {
            Relationships.Remove(service);
        }
    }
}
