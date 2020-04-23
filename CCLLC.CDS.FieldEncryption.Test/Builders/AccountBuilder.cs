using System;
using DLaB.Xrm.Test;

namespace CCLLC.CDS.Test.Builders
{
    public class AccountBuilder : EntityBuilder<Account, AccountBuilder>
    {
        private Account Proxy { get; set; }

        public AccountBuilder()
        {
            Proxy = new Account();
        }

        public AccountBuilder(Id id) : this()
        {
            Id = id;
        }

        #region Fluent Methods

        public AccountBuilder WithNinjaId(int value)
        {
            Proxy.ccllc_InvoiceNinjaId = value;
            return this;
        }

        #endregion

        protected override Account BuildInternal()
        {
            return Proxy;
        }

    }
}
