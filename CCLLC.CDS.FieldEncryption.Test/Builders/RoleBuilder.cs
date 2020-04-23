using DLaB.Xrm.Test;

namespace CCLLC.CDS.Test.Builders
{
    public class RoleBuilder : EntityBuilder<Role, RoleBuilder>
    {
        private Role Proxy { get; set; }

        public RoleBuilder()
        {
            Proxy = new Role();
        }
        public RoleBuilder(Id id) : this()
        {
            Id = id;
        }

        public RoleBuilder WithName(string value)
        {
            Proxy.Name = value;
            return this;
        }

        protected override Role BuildInternal()
        {
            return Proxy;
        }
    }
}
