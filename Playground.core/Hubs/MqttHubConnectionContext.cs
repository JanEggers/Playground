using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Internal.Protocol;
using Microsoft.Extensions.Logging;
using MQTTnet.Packets;
using System;
using System.Threading.Tasks;

namespace Playground.core.Hubs
{
    public class MqttHubConnectionContext : HubConnectionContext
    {
        public MqttHubConnectionContext(ConnectionContext connectionContext, TimeSpan keepAliveInterval, ILoggerFactory loggerFactory)
            : base(connectionContext, keepAliveInterval, loggerFactory)
        {
        }

        public ValueTask WriteAsync(MqttBasePacket packet) 
        {
            return WriteAsync(new CompletionMessage("", null, packet, true));
        }
    }
}
