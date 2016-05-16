using EntityFramework.Extensions;
using Microsoft.AspNet.SignalR;
using Microsoft.Data.Edm;
using Playground.Hubs;
using Playground.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.OData;
using System.Web.Http.OData.Formatter.Serialization;

namespace Playground.Services
{
    public class UpdateService
    {
        private PlaygroundContext m_context;
        private IEdmModel m_model;
        private IHubContext<IUpdater> m_updater;

        public UpdateService(PlaygroundContext context, IEdmModel model, IHubContext<IUpdater> updater)
        {
            m_context = context;
            m_model = model;
            m_updater = updater;
        }

        public PlaygroundContext Context
        {
            get { return m_context; }
        }

        public void SaveChanges(HttpRequestMessage request)
        {
            var audit = m_context.BeginAudit();

            m_context.SaveChanges();

            foreach (var entity in audit.LastLog.Entities)
            {
                var changes = new Dictionary<string, object>();

                var msg = entity.EntityType.Name + " " + string.Join(",", entity.Keys.Select(p => p.Value)) + " changed";
                Trace.WriteLine(msg);

                foreach (var property in entity.Properties)
                {
                    msg = property.Name + " changed from " + property.Original + " to " + property.Current;
                    if (property.IsRelationship) {
                        msg = "Relation " + msg;
                    }
                    
                    Trace.WriteLine(msg);

                    changes[property.Name] = property.Current;
                }

                var type = m_model.FindType(entity.EntityType.FullName);
                var container = GetContainer();

                var serializationContext = new ODataSerializerContext();
                serializationContext.Model = m_model;
                serializationContext.EntitySet = container.EntitySets().FirstOrDefault(s => s.ElementType.Name == entity.EntityType.Name);


                var eic = new EntityInstanceContext(serializationContext, new FakeItReference { Definition = type }, entity.Current);
                eic.Url = new System.Web.Http.Routing.UrlHelper(request);
                var builder = m_model.GetEntitySetLinkBuilder(serializationContext.EntitySet);

                var idLink = builder.BuildIdLink(eic, System.Web.Http.OData.Formatter.ODataMetadataLevel.Default);

                m_updater.Clients.All.Update(idLink, changes);
            }
        }

        private IEdmEntityContainer GetContainer() { 
            return m_model.FindEntityContainer(m_context.GetType().Name);
        }

        public Type GetRelatedEntityType(Type entity, string relation) 
        {
            var container = GetContainer();

            var set = container.EntitySets().FirstOrDefault(s => s.ElementType.Name == entity.BaseType.Name);

            var targetSet = set.NavigationTargets.Where(n => n.NavigationProperty.Name == relation).Select(n => n.TargetEntitySet).FirstOrDefault();

            var targetType = entity.BaseType.Assembly.GetType(targetSet.ElementType.Namespace + "." + targetSet.ElementType.Name);

            return targetType;
        }
    }

    public class FakeItReference : IEdmEntityTypeReference
    {
        public IEdmType Definition
        {
            get; set;
        }

        public bool IsNullable
        {
            get; set;
        }
    }
}