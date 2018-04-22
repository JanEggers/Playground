using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using MQTTnet.Packets;
using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Playground.core.Hubs
{
    public class MqttHubConnectionContext : HubConnectionContext
    {
        private readonly ConnectionContext connectionContext;

        public MqttHubConnectionContext(ConnectionContext connectionContext, TimeSpan keepAliveInterval, ILoggerFactory loggerFactory)
            : base(connectionContext, keepAliveInterval, loggerFactory)
        {
            this.connectionContext = connectionContext;
        }

        public PipeReader Input => connectionContext.Transport.Input;

        public ValueTask WriteAsync(MqttBasePacket packet) 
        {
            return WriteAsync(new CompletionMessage("", null, packet, true));
        }

        public ValueTask PublishAsync(MqttPublishPacket packet)
        {
            return WriteAsync(packet);
        }
    }
}
