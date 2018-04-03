using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MQTTnet.Packets;
using System;
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

        [Mqtt(Topic ="Step")]
        public void OnPublish(long payload) 
        {

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
