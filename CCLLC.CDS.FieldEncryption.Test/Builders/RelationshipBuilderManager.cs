using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using DLaB.Xrm.Test;

namespace CCLLC.CDS.Test.Builders
{
    public class RelationshipBuilderManager
    {
        IList<RelationshipBuilder> Builders { get; }

        public RelationshipBuilderManager()
        {
            Builders = new List<RelationshipBuilder>();
        }

        public void AddBuilderForTargetRecord<TBuilder>(Id id, Action<TBuilder> action) where TBuilder : RelationshipBuilder
        {
            var constructor = GetIdConstructor<TBuilder>();
            var builder = (TBuilder)constructor.Invoke(new object[] { id });
            action(builder);
            Builders.Add(builder);
        }

        public void Build(IOrganizationService service)
        {
            foreach(var b in Builders)
            {
                b.Associate(service);
            }
        }


        public void Remove(IOrganizationService service)
        {
            foreach (var b in Builders)
            {
                b.Disassociate(service);
            }
        }
        private static ConstructorInfo GetIdConstructor<TBuilder>() where TBuilder : RelationshipBuilder
        {
            var constructor = typeof(TBuilder).GetConstructor(new[] { typeof(Id) });
            if (constructor == null)
            {
                throw new Exception($"{typeof(TBuilder).FullName} does not contain a constructor with a single parameter of type {typeof(Id).FullName}");
            }
            return constructor;
        }

    }

    
}
