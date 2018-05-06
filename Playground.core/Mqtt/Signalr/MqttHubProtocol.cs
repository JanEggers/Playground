using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using MQTTnet.Packets;
using MQTTnet.Serializer;

namespace Playground.core.Mqtt.Signalr
{
    public class MqttHubProtocol : IHubProtocol
    {
        private readonly MqttPacketSerializer _serializer;
        private readonly ILogger<MqttHubProtocol> _logger;

        public string Name => "MQTT";

        public int Version => 311;

        public TransferFormat TransferFormat => TransferFormat.Binary;

        public MqttHubProtocol(MqttPacketSerializer serializer, ILogger<MqttHubProtocol> logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        public bool IsVersionSupported(int version)
        {
            return version == Version;
        }

        private Subject<MqttPublishPacket> _packets = new Subject<MqttPublishPacket>();
        
        public bool TryParseMessage(ref ReadOnlySequence<byte> input, IInvocationBinder binder, out HubMessage message)
        {
            message = null;
            return false;
        }
        
        public void WriteMessage(HubMessage message, IBufferWriter<byte> output)
        {
            var packet = GetMqttPacket(message);
            if (packet == null)
            {
                return;
            }
            var buffer = _serializer.Serialize(packet);            
            foreach (var chunk in buffer)
            {
                output.Write(chunk.Array.AsSpan(chunk.Offset, chunk.Count));
            }
        }

        public ReadOnlyMemory<byte> GetMessageBytes(HubMessage message)
        {
            return HubProtocolExtensions.GetMessageBytes(this, message);
        }

        private MqttBasePacket GetMqttPacket(HubMessage message)
        {
            switch (message)
            {
                case CompletionMessage completion:
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation($"complete {completion.InvocationId}");
                    }
                    return GetMqttPacket(completion);
                case StreamItemMessage streamItem:
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation($"streamItem {streamItem.InvocationId}");
                    }
                    return GetMqttPacket(streamItem);
                case CloseMessage close:
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation($"close");
                    }
                    return new MqttDisconnectPacket() { };
                case InvocationMessage invokation:
                    return invokation.Arguments.OfType<MqttPublishPacket>().FirstOrDefault();
                default:
                    return null;
            }
        }

        private MqttBasePacket GetMqttPacket(StreamItemMessage streamItem)
        {
            switch (streamItem.Item)
            {
                case MqttBasePacket mqtt:
                    return mqtt;
                default:
                    return null;
            }
        }

        private MqttBasePacket GetMqttPacket(CompletionMessage completion) 
        {
            switch (completion.Result) 
            {
                case MqttBasePacket mqtt:
                    return mqtt;
                default:
                    return null;
            }
        }
    }
}
