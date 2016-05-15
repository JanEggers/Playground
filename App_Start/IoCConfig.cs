using Microsoft.Practices.Unity;
using Playground.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace Playground
{
    public static class IoCConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var container = new PlaygroundContainer();
            config.DependencyResolver = new UnityResolver(container);
        }
    }

    public class PlaygroundContainer : UnityContainer
    {
        public PlaygroundContainer()
        {
            this.RegisterType<PlaygroundContext>();
        }
    }

    public class UnityResolver : IDependencyResolver
    {
        protected IUnityContainer container;

        public UnityResolver(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return new List<object>();
            }
        }

        public IDependencyScope BeginScope()
        {
            var child = container.CreateChildContainer();
            return new UnityResolver(child);
        }

        public void Dispose()
        {
            container.Dispose();
        }
    }
}