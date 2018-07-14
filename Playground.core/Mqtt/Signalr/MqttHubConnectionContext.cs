using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using MQTTnet.Packets;
using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Playground.core.Mqtt.Signalr
{
    public class MqttHubConnectionContext : HubConnectionContext
    {
        private readonly ConnectionContext connectionContext;

        public MqttHubConnectionContext(ConnectionContext connectionContext, TimeSpan keepAliveInterval, ILoggerFactory loggerFactory, IHubProtocol protocol)
            : base(connectionContext, keepAliveInterval, loggerFactory)
        {
            this.connectionContext = connectionContext;

            var p = typeof(HubConnectionContext).GetProperty("Protocol", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            p.SetValue(this, protocol);
        }
        
        public PipeReader Input => connectionContext.Transport.Input;

        public ValueTask WriteAsync(MqttBasePacket packet)
        {
            return WriteAsync(new CompletionMessage("", null, packet, true));
        }
    }
}
