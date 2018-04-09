using MQTTnet;
using MQTTnet.Exceptions;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Serializer;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MQTTnet.Serializer
{
    public sealed partial class MqttPacketSerializer
    {
        public bool Deserialize(ref ReadOnlySequence<byte> input, out MqttBasePacket packet)
        {
            packet = null;
            var copy = input;
            var header = copy.ReadHeader();
            if (header == null || copy.Length < header.BodyLength)
            {
                return false;
            }

            input = copy.Slice(header.BodyLength);
            packet = Deserialize(header, copy.Slice(0, header.BodyLength).First.Span);
            return true;
        }

        private MqttBasePacket Deserialize(MqttPacketHeader header, in ReadOnlySpan<byte> body)
        {
            switch (header.ControlPacketType)
            {
                case MqttControlPacketType.Connect: return DeserializeConnect(body);
                case MqttControlPacketType.ConnAck: return DeserializeConnAck(body);
                case MqttControlPacketType.Disconnect: return new MqttDisconnectPacket();
                case MqttControlPacketType.Publish: return DeserializePublish(body, header);
                case MqttControlPacketType.PubAck: return DeserializePubAck(body);
                case MqttControlPacketType.PubRec: return DeserializePubRec(body);
                case MqttControlPacketType.PubRel: return DeserializePubRel(body);
                case MqttControlPacketType.PubComp: return DeserializePubComp(body);
                case MqttControlPacketType.PingReq: return new MqttPingReqPacket();
                case MqttControlPacketType.PingResp: return new MqttPingRespPacket();
                case MqttControlPacketType.Subscribe: return DeserializeSubscribe(body);
                case MqttControlPacketType.SubAck: return DeserializeSubAck(body);
                case MqttControlPacketType.Unsubscibe: return DeserializeUnsubscribe(body);
                case MqttControlPacketType.UnsubAck: return DeserializeUnsubAck(body);
                default: throw new MqttProtocolViolationException($"Packet type ({(int)header.ControlPacketType}) not supported.");
            }
        }

        private static MqttBasePacket DeserializeUnsubAck(in ReadOnlySpan<byte> body)
        {
            return new MqttUnsubAckPacket
            {
                PacketIdentifier = body.ReadUInt16()
            };
        }

        private static MqttBasePacket DeserializePubComp(in ReadOnlySpan<byte> body)
        {
            return new MqttPubCompPacket
            {
                PacketIdentifier = body.ReadUInt16()
            };
        }

        private static MqttBasePacket DeserializePubRel(in ReadOnlySpan<byte> body)
        {
            return new MqttPubRelPacket
            {
                PacketIdentifier = body.ReadUInt16()
            };
        }

        private static MqttBasePacket DeserializePubRec(in ReadOnlySpan<byte> body)
        {
            return new MqttPubRecPacket
            {
                PacketIdentifier = body.ReadUInt16()
            };
        }

        private static MqttBasePacket DeserializePubAck(in ReadOnlySpan<byte> body)
        {
            return new MqttPubAckPacket
            {
                PacketIdentifier = body.ReadUInt16()
            };
        }

        private static MqttBasePacket DeserializeUnsubscribe(in ReadOnlySpan<byte> body)
        {
            var position = 0;
            var packet = new MqttUnsubscribePacket
            {
                PacketIdentifier = body.ReadUInt16(ref position),
            };

            while (position < body.Length)
            {
                packet.TopicFilters.Add(body.ReadStringWithLengthPrefix(ref position));
            }

            return packet;
        }

        private static MqttBasePacket DeserializeSubscribe(in ReadOnlySpan<byte> body)
        {
            var position = 0;
            var packet = new MqttSubscribePacket
            {
                PacketIdentifier = body.ReadUInt16(ref position)
            };

            while (position < body.Length)
            {
                packet.TopicFilters.Add(new TopicFilter(
                    body.ReadStringWithLengthPrefix(ref position),
                    (MqttQualityOfServiceLevel)body.ReadByte(ref position)));
            }

            return packet;
        }

        private static MqttBasePacket DeserializePublish(in ReadOnlySpan<byte> body, MqttPacketHeader mqttPacketHeader)
        {
            var fixedHeader = new ByteReader(mqttPacketHeader.FixedHeader);
            var retain = fixedHeader.Read();
            var qualityOfServiceLevel = (MqttQualityOfServiceLevel)fixedHeader.Read(2);
            var dup = fixedHeader.Read();

            var position = 0;
            var topic = body.ReadStringWithLengthPrefix(ref position);

            ushort packetIdentifier = 0;
            if (qualityOfServiceLevel > MqttQualityOfServiceLevel.AtMostOnce)
            {
                packetIdentifier = body.ReadUInt16(ref position);
            }

            var packet = new MqttPublishPacket
            {
                Retain = retain,
                QualityOfServiceLevel = qualityOfServiceLevel,
                Dup = dup,
                Topic = topic,
                Payload = body.Slice(position).ToArray(),
                PacketIdentifier = packetIdentifier
            };

            return packet;
        }

        private static MqttBasePacket DeserializeConnect(in ReadOnlySpan<byte> body)
        {
            var position = 2; // Skip 2 bytes

            MqttProtocolVersion protocolVersion;
            var protocolName = body.Slice(position, 4);
            position += 4;
            if (protocolName.SequenceEqual(ProtocolVersionV310Name))
            {
                position += 2;
                protocolVersion = MqttProtocolVersion.V310;
            }
            else if (protocolName.SequenceEqual(ProtocolVersionV311Name))
            {
                protocolVersion = MqttProtocolVersion.V311;
            }
            else
            {
                throw new MqttProtocolViolationException("Protocol name is not supported.");
            }

            position += 1; // Skip protocol level
            var connectFlags = body.ReadByte(ref position);

            var connectFlagsReader = new ByteReader(connectFlags);
            connectFlagsReader.Read(); // Reserved.

            var packet = new MqttConnectPacket
            {
                ProtocolVersion = protocolVersion,
                CleanSession = connectFlagsReader.Read()
            };

            var willFlag = connectFlagsReader.Read();
            var willQoS = connectFlagsReader.Read(2);
            var willRetain = connectFlagsReader.Read();
            var passwordFlag = connectFlagsReader.Read();
            var usernameFlag = connectFlagsReader.Read();

            packet.KeepAlivePeriod = body.ReadUInt16(ref position);
            packet.ClientId = body.ReadStringWithLengthPrefix(ref position);

            if (willFlag)
            {
                packet.WillMessage = new MqttPublishPacket
                {
                    Topic = body.ReadStringWithLengthPrefix(ref position),
                    Payload = body.ReadWithLengthPrefix(ref position),
                    QualityOfServiceLevel = (MqttQualityOfServiceLevel)willQoS,
                    Retain = willRetain
                };
            }

            if (usernameFlag)
            {
                packet.Username = body.ReadStringWithLengthPrefix(ref position);
            }

            if (passwordFlag)
            {
                packet.Password = body.ReadStringWithLengthPrefix(ref position);
            }

            ValidateConnectPacket(packet);
            return packet;
        }

        private static MqttBasePacket DeserializeSubAck(in ReadOnlySpan<byte> body)
        {
            var position = 0;
            var packet = new MqttSubAckPacket
            {
                PacketIdentifier = body.ReadUInt16(ref position)
            };

            while (position < body.Length)
            {
                packet.SubscribeReturnCodes.Add((MqttSubscribeReturnCode)body.ReadByte(ref position));
            }

            return packet;
        }

        private MqttBasePacket DeserializeConnAck(in ReadOnlySpan<byte> body)
        {
            var position = 0;
            var packet = new MqttConnAckPacket();

            var firstByteReader = new ByteReader(body.ReadByte(ref position));

            if (ProtocolVersion == MqttProtocolVersion.V311)
            {
                packet.IsSessionPresent = firstByteReader.Read();
            }

            packet.ConnectReturnCode = (MqttConnectReturnCode)body.ReadByte(ref position);

            return packet;
        }
    }
}
