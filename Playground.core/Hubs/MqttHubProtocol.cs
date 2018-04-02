using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR.Internal;
using Microsoft.AspNetCore.SignalR.Internal.Protocol;
using MQTTnet.Packets;
using MQTTnet.Serializer;

namespace Playground.core.Hubs
{
    public class MqttHubProtocol : IHubProtocol
    {
        private readonly MqttPacketSerializer _serializer;
        private readonly ConnectionContext connectionContext;

        public string Name => "MQTT";

        public int Version => 123;

        public TransferFormat TransferFormat => TransferFormat.Binary;

        public MqttHubProtocol(MqttPacketSerializer serializer)
        {
            _serializer = serializer;
        }

        public bool IsVersionSupported(int version)
        {
            return true;
        }

        private Dictionary<string, MqttSubscribePacket> _subscriptions = new Dictionary<string, MqttSubscribePacket>();

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
                        message = new InvocationMessage(invokation, nameof(MqttHub.Connect), null, connect);
                        break;
                    case MqttPublishPacket publish:
                        message = new InvocationMessage(invokation, nameof(MqttHub.OnPublish), null, publish);
                        break;
                    case MqttSubscribePacket subscribe:
                        message = new StreamInvocationMessage(invokation, nameof(MqttHub.OnSubscribe), null, subscribe);
                        _subscriptions[invokation] = subscribe;
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
                    WriteCompletionMessage(completion, output);
                    break;
                case CloseMessage close:
                    WriteMqttPacket(new MqttDisconnectPacket() { }, output);
                    break;
                case StreamItemMessage streamItem:
                    if (_subscriptions.TryGetValue(streamItem.InvocationId, out var subscription)) 
                    {
                        WriteMqttPacket(new MqttPublishPacket() {
                            Topic = subscription.TopicFilters[0].Topic
                        }, output);
                    }
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
