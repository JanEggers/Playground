using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Playground.core.Hubs
{
    public class UpdateHub : Hub
    {
        private readonly ILogger<UpdateHub> logger;

        public UpdateHub(ILogger<UpdateHub> logger)
        {
            this.logger = logger;
        }

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
            await Clients.All.SendAsync("Send", message);            
        }

        public ChannelReader<DateTime> Tick()
        {
            var obs = Observable.Interval(TimeSpan.FromSeconds(1))
                .Select(x => DateTime.Now)
                .Do(x => logger.LogInformation($"{x}"));

            return obs.AsChannelReader(Context.ConnectionAborted);
        }
    }
}
