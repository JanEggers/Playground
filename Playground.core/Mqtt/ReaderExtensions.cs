using System.Buffers;
using MQTTnet.Packets;
using MQTTnet.Serializer;
using MQTTnet.AspNetCore;

namespace Playground.core.Mqtt
{
    public static class ReaderExtensions 
    {
        public static bool TryDeserialize(this IMqttPacketSerializer serializer, ref ReadOnlySequence<byte> input, out MqttBasePacket packet)
        {
            if (!serializer.TryDeserialize(input, out packet, out var consumed, out var observed))
            {
                return false;
            }

            input = input.Slice(consumed);
            return true;
        }
    }
}
