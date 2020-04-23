using System;
using System.Collections.Generic;
using DLaB.Xrm;
using Microsoft.Xrm.Sdk;


namespace CCLLC.CDS.Test.Builders
{
    public class OrganizationServiceBuilder : DLaB.Xrm.Test.Builders.OrganizationServiceBuilderBase<OrganizationServiceBuilder>
    {
       
        private List<Func<IOrganizationService, Entity, Entity>> CreatePreProcessors { get; }
        private List<Func<IOrganizationService, Entity, Entity>> UpdatePreProcessors { get; }
        private List<Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>> AssociatePreProcessors { get; }
        private Dictionary<string, Func<IOrganizationService, OrganizationRequest, OrganizationResponse>> ActionPreProcessors { get; }

        protected override OrganizationServiceBuilder This => this;

        #region Constructors

        public OrganizationServiceBuilder() 
            : this(GetOrganizationService()) { }

        public OrganizationServiceBuilder(IOrganizationService service) 
            : base(service)
        {            
            CreatePreProcessors = new List<Func<IOrganizationService, Entity, Entity>>();
            UpdatePreProcessors = new List<Func<IOrganizationService, Entity, Entity>>();
            AssociatePreProcessors = new List<Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>>();
            ActionPreProcessors = new Dictionary<string,Func<IOrganizationService, OrganizationRequest, OrganizationResponse>>();

            CreatePreProcessors.Add(ConvertToSdkEntityPreProcessor);
            UpdatePreProcessors.Add(ConvertToSdkEntityPreProcessor);
        }

        #endregion Constructors

        #region Fluent Methods

        public OrganizationServiceBuilder WithAssociationLogger(Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection> logger)
        {
            AssociatePreProcessors.Add(logger);
            return this;
        }


        public OrganizationServiceBuilder WithActionHandler<T>(Func<IOrganizationService, OrganizationRequest, OrganizationResponse> action) where T : OrganizationRequest, new()
        {
            var requestName = new T().RequestName;
            ActionPreProcessors.Add(requestName, action);
            return this;
        }

        #endregion Fluent Methods      

        public new IOrganizationService Build()
        {
            var service = base.Build();
            var preProcessor = new OrganizationServicePreProcessor(service);
            preProcessor.AddCreatePreProcessors(CreatePreProcessors.ToArray());
            preProcessor.AddUpdatePreProcessors(UpdatePreProcessors.ToArray());
            preProcessor.AddAssociatePreProcessors(AssociatePreProcessors.ToArray());
            preProcessor.AddActionPreProcessors(ActionPreProcessors);

            return preProcessor;
        }

        private Entity ConvertToSdkEntityPreProcessor(IOrganizationService service, Entity entity)
        {
            if (entity.GetType() == typeof(Entity))
            {
                return entity;
            }

            return entity.ToEntity<Entity>();
        }

        private static IOrganizationService GetOrganizationService()
        {
            return DLaB.Xrm.Test.TestBase.GetOrganizationService();
        }
    }
}