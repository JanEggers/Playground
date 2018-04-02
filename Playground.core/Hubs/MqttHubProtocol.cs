using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR.Internal;
using Microsoft.AspNetCore.SignalR.Internal.Protocol;
using Microsoft.Extensions.Logging;
using MQTTnet.Packets;
using MQTTnet.Serializer;

namespace Playground.core.Hubs
{
    public class MqttHubProtocol : IHubProtocol
    {
        private readonly MqttPacketSerializer _serializer;
        private readonly ILogger<MqttHubProtocol> _logger;

        public string Name => "MQTT";

        public int Version => 123;

        public TransferFormat TransferFormat => TransferFormat.Binary;

        public MqttHubProtocol(MqttPacketSerializer serializer, ILogger<MqttHubProtocol> logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        public bool IsVersionSupported(int version)
        {
            return true;
        }

        private Subject<MqttPublishPacket> _packets = new Subject<MqttPublishPacket>();
        
        public bool TryParseMessage(ref ReadOnlySequence<byte> input, IInvocationBinder binder, out HubMessage message)
        {
            MqttBasePacket packet;
            message = null;
            do
            {
                packet = _serializer.Deserialize(ref input);

                var invokation = Guid.NewGuid().ToString();

                switch (packet)
                {
                    case MqttConnectPacket connect:
                        _logger.LogInformation($"connect {invokation}");
                        message = new InvocationMessage(invokation, nameof(MqttHub.Connect), null, connect);
                        break;
                    case MqttPublishPacket publish:
                        _logger.LogInformation($"publish {invokation} {publish.Topic}");
                        _packets.OnNext(publish);
                        message = new InvocationMessage(invokation, nameof(MqttHub.OnPublish), null, publish);
                        break;
                    case MqttSubscribePacket subscribe:
                        _logger.LogInformation($"subscribe {invokation} {string.Join(", ", subscribe.TopicFilters.Select(t => t.Topic))}");
                        message = new StreamInvocationMessage(invokation, nameof(MqttHub.OnSubscribe), null, _packets, subscribe);
                        break;
                    case null:
                        break;
                    default:
                        break;
                }
            } while (message == null && packet != null);
            return message != null;
        }

        public void WriteMessage(HubMessage message, Stream output)
        {
            switch (message)
            {
                case CompletionMessage completion:
                    _logger.LogInformation($"complete {completion.InvocationId}");
                    WriteCompletionMessage(completion, output);
                    break;
                case StreamItemMessage streamItem:
                    _logger.LogInformation($"streamItem {streamItem.InvocationId}");
                    WriteStreamMessage(streamItem, output);
                    break;
                case CloseMessage close:
                    _logger.LogInformation($"close");
                    WriteMqttPacket(new MqttDisconnectPacket() { }, output);
                    break;
                default:
                    break;
            }
        }

        public void WriteStreamMessage(StreamItemMessage streamItem, Stream output)
        {
            switch (streamItem.Item)
            {
                case MqttBasePacket mqtt:
                    WriteMqttPacket(mqtt, output);
                    break;
                default:
                    break;
            }
        }

        public void WriteCompletionMessage(CompletionMessage completion, Stream output) 
        {
            switch (completion.Result) 
            {
                case MqttBasePacket mqtt:
                    WriteMqttPacket(mqtt, output);
                    break;
                default: 
                    break;
            }
        }

        private void WriteMqttPacket(MqttBasePacket mqtt, Stream output)
        {
            var buffer = _serializer.Serialize(mqtt);
            foreach (var chunk in buffer)
            {
                output.Write(chunk.Array, chunk.Offset, chunk.Count);
            }
        }
    }
}
