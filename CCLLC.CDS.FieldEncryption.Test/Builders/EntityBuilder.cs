using Microsoft.Xrm.Sdk;

namespace CCLLC.CDS.Test.Builders
{
    public abstract class EntityBuilder<TEntity, TBuilder> : DLaB.Xrm.Test.Builders.DLaBEntityBuilder<TEntity, TBuilder>
        where TBuilder : EntityBuilder<TEntity, TBuilder>
        where TEntity : Entity
    {

    }
}
