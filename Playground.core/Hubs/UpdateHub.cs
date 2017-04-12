using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Playground.core.Hubs
{
    public class UpdateHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task Send(string message)
        {
            await Clients.All.InvokeAsync("Send", message);            
        }
    }
}
