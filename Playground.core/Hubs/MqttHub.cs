using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Playground.core.Hubs
{
    public class MqttHub : Hub
    {
        private readonly ILogger<MqttHub> logger;

        public MqttHub(ILogger<MqttHub> logger)
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
    }
}
