using Microsoft.AspNetCore.SignalR;
using MQTTnet.Packets;
using System;
using System.Threading.Tasks;

namespace Playground.core.Hubs
{
    public class MqttHub : Hub
    {
        public MqttConnAckPacket Connect(MqttConnectPacket connect)
        {
            return new MqttConnAckPacket()
            {
                ConnectReturnCode = MQTTnet.Protocol.MqttConnectReturnCode.ConnectionAccepted
            };
        }


        public void OnPublish(MqttPublishPacket publish)
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
