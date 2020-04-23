using System;
using DLaB.Xrm.Test;


namespace CCLLC.CDS.Test.Builders
{
    public class ContactBuilder : EntityBuilder<Contact, ContactBuilder>
    {
        private Contact Proxy { get; set; }

        public ContactBuilder()
        {
            Proxy = new Contact();
        }

        public ContactBuilder(Id id) : this()
        {
            Id = id;
        }

        #region Fluent Methods

        public ContactBuilder WithFirstName(string value)
        {
            Proxy.FirstName = value;
            return this;
        }

        public ContactBuilder WithLastName(string value)
        {
            Proxy.LastName = value;
            return this;
        }

        public ContactBuilder WithEmailAddress(string value)
        {
            Proxy.EMailAddress1 = value;
            return this;
        }

        #endregion

        protected override Contact BuildInternal()
        {
            return Proxy;
        }

    }
}

