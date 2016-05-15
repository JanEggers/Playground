using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Playground.Hubs
{
    public class UpdateHub : Hub<IUpdater>
    {
    }

    public class InjectedHub<THub, TClient> : IHubContext<TClient>
        where THub : IHub
        where TClient : class
    {
        private IHubContext<TClient> m_ctx;

        public InjectedHub()
        {
            m_ctx = GlobalHost.ConnectionManager.GetHubContext<THub, TClient>();
        }

        public IHubConnectionContext<TClient> Clients
        {
            get
            {
                return m_ctx.Clients;
            }
        }

        public IGroupManager Groups
        {
            get
            {
                return m_ctx.Groups;
            }
        }
    }
}