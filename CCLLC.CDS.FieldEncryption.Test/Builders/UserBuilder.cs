using DLaB.Xrm.Test;

namespace CCLLC.CDS.Test.Builders
{
    public class UserBuilder : EntityBuilder<SystemUser, UserBuilder>
    {
        private SystemUser Proxy { get; set; }

        public UserBuilder()
        {
            Proxy = new SystemUser();
        }

        public UserBuilder(Id id) : this()
        {
            Id = id;
        }

        #region Fluent Methods

        public UserBuilder AsSystemUser()
        {
            Proxy["lastname"] = "SYSTEM";
            Proxy["firstname"] = "";
            return this;
        }

        #endregion

        protected override SystemUser BuildInternal()
        {
            return Proxy;
        }

    }
}
