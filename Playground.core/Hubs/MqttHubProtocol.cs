using System.Buffers;
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

        public bool TryParseMessage(ref ReadOnlySequence<byte> input, IInvocationBinder binder, out HubMessage message)
        {
            MqttBasePacket packet;
            message = null;
            do
            {
                packet = _serializer.Deserialize(ref input);
                
                switch (packet)
                {
                    case MqttConnectPacket connect:
                        message = new InvocationMessage(nameof(MqttHub.Connect), null, connect);
                        break;
                    case MqttPublishPacket publish:
                        message = new InvocationMessage(nameof(MqttHub.OnPublish), null, publish);
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
        }
    }
}
