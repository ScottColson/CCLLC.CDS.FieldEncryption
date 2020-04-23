using Microsoft.Xrm.Sdk;
using DLaB.Xrm.Test;
using System.Xml.Linq;
using System;
using System.Text;

namespace CCLLC.CDS.Test.Builders
{
   
    class WebResourceBuilder : EntityBuilder<WebResource, WebResourceBuilder>
    {
        private WebResource Proxy { get; set; }

        public WebResourceBuilder()
        {
            Proxy = new WebResource();
        }

        public WebResourceBuilder(Id id) : this()
        {
            Id = id;
        }

        #region Fluent Methods

        public WebResourceBuilder WithName(string value)
        {
            Proxy.Name = value;
            return this;
        }

        public WebResourceBuilder WithType(WebResource_WebResourceType type)
        {
            Proxy.WebResourceType = new OptionSetValue((int)type);
            return this;
        }

        public WebResourceBuilder WithContent(XDocument value)
        {
            return this.WithContent(value.ToString());            
        }

        public WebResourceBuilder WithContent(string value)
        {
            Proxy.Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            return this;
        }

        #endregion

        protected override WebResource BuildInternal()
        {
            return Proxy;
        }

    }
}
