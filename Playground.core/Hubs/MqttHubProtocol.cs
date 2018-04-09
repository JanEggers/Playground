using System.Buffers;
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

        public void WriteMessage(HubMessage message, Stream output)
        {
            switch (message)
            {
                case CompletionMessage completion:
                    if (_logger.IsEnabled(LogLevel.Information)) 
                    {
                        _logger.LogInformation($"complete {completion.InvocationId}");
                    }
                    WriteCompletionMessage(completion, output);
                    break;
                case StreamItemMessage streamItem:
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation($"streamItem {streamItem.InvocationId}");
                    }
                    WriteStreamMessage(streamItem, output);
                    break;
                case CloseMessage close:
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation($"close");
                    }
                    WriteMqttPacket(new MqttDisconnectPacket() { }, output);
                    break;
                case InvocationMessage invokation:
                    var packet = invokation.Arguments.OfType<MqttPublishPacket>().FirstOrDefault();
                    WriteMqttPacket(packet, output);
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
