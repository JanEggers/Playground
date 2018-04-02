using MQTTnet.Exceptions;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MQTTnet.Serializer
{
    public sealed class MqttPacketSerializer
    {
        private static byte[] ProtocolVersionV311Name { get; } = Encoding.UTF8.GetBytes("MQTT");
        private static byte[] ProtocolVersionV310Name { get; } = Encoding.UTF8.GetBytes("MQIs");

        public MqttProtocolVersion ProtocolVersion { get; set; } = MqttProtocolVersion.V311;

        public ICollection<ArraySegment<byte>> Serialize(MqttBasePacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            using (var stream = new MemoryStream(128))
            using (var writer = new MqttPacketWriter(stream))
            {
                var fixedHeader = SerializePacket(packet, writer);
                var remainingLength = (int)stream.Length;
                writer.Write(fixedHeader);
                MqttPacketWriter.WriteRemainingLength(remainingLength, writer);
                var headerLength = (int)stream.Length - remainingLength;

#if NET461 || NET452 || NETSTANDARD2_0
                var buffer = stream.GetBuffer();
#else
                var buffer = stream.ToArray();
#endif
                return new List<ArraySegment<byte>>
                {
                    new ArraySegment<byte>(buffer, remainingLength, headerLength),
                    new ArraySegment<byte>(buffer, 0, remainingLength)
                };
            }
        }

        public MqttBasePacket Deserialize(ref ReadOnlySequence<byte> input)
        {
            var copy = input;
            var header = copy.ReadHeader();
            if (header == null)
            {
                return null;
            }

            if (copy.Length < header.BodyLength) 
            {
                return null;
            }

            input = copy.Slice(header.BodyLength);
            return Deserialize(header, copy.Slice(0, header.BodyLength).First.Span);
        }

        private byte SerializePacket(MqttBasePacket packet, MqttPacketWriter writer)
        {
            switch (packet)
            {
                case MqttConnectPacket connectPacket: return Serialize(connectPacket, writer);
                case MqttConnAckPacket connAckPacket: return Serialize(connAckPacket, writer);
                case MqttDisconnectPacket _: return SerializeEmptyPacket(MqttControlPacketType.Disconnect);
                case MqttPingReqPacket _: return SerializeEmptyPacket(MqttControlPacketType.PingReq);
                case MqttPingRespPacket _: return SerializeEmptyPacket(MqttControlPacketType.PingResp);
                case MqttPublishPacket publishPacket: return Serialize(publishPacket, writer);
                case MqttPubAckPacket pubAckPacket: return Serialize(pubAckPacket, writer);
                case MqttPubRecPacket pubRecPacket: return Serialize(pubRecPacket, writer);
                case MqttPubRelPacket pubRelPacket: return Serialize(pubRelPacket, writer);
                case MqttPubCompPacket pubCompPacket: return Serialize(pubCompPacket, writer);
                case MqttSubscribePacket subscribePacket: return Serialize(subscribePacket, writer);
                case MqttSubAckPacket subAckPacket: return Serialize(subAckPacket, writer);
                case MqttUnsubscribePacket unsubscribePacket: return Serialize(unsubscribePacket, writer);
                case MqttUnsubAckPacket unsubAckPacket: return Serialize(unsubAckPacket, writer);
                default: throw new MqttProtocolViolationException("Packet type invalid.");
            }
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
        
        private static void ValidateConnectPacket(MqttConnectPacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            if (string.IsNullOrEmpty(packet.ClientId) && !packet.CleanSession)
            {
                throw new MqttProtocolViolationException("CleanSession must be set if ClientId is empty [MQTT-3.1.3-7].");
            }
        }

        private static void ValidatePublishPacket(MqttPublishPacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            if (packet.QualityOfServiceLevel == 0 && packet.Dup)
            {
                throw new MqttProtocolViolationException("Dup flag must be false for QoS 0 packets [MQTT-3.3.1-2].");
            }
        }

        private byte Serialize(MqttConnectPacket packet, MqttPacketWriter writer)
        {
            ValidateConnectPacket(packet);

            // Write variable header
            writer.Write(0x00, 0x04); // 3.1.2.1 Protocol Name
            if (ProtocolVersion == MqttProtocolVersion.V311)
            {
                writer.Write(ProtocolVersionV311Name);
                writer.Write(0x04); // 3.1.2.2 Protocol Level (4)
            }
            else
            {
                writer.Write(ProtocolVersionV310Name);
                writer.Write(0x64, 0x70, 0x03); // Protocol Level (0x03)
            }

            var connectFlags = new ByteWriter(); // 3.1.2.3 Connect Flags
            connectFlags.Write(false); // Reserved
            connectFlags.Write(packet.CleanSession);
            connectFlags.Write(packet.WillMessage != null);

            if (packet.WillMessage != null)
            {
                connectFlags.Write((int)packet.WillMessage.QualityOfServiceLevel, 2);
                connectFlags.Write(packet.WillMessage.Retain);
            }
            else
            {
                connectFlags.Write(0, 2);
                connectFlags.Write(false);
            }

            connectFlags.Write(packet.Password != null);
            connectFlags.Write(packet.Username != null);

            writer.Write(connectFlags);
            writer.Write(packet.KeepAlivePeriod);
            writer.WriteWithLengthPrefix(packet.ClientId);

            if (packet.WillMessage != null)
            {
                writer.WriteWithLengthPrefix(packet.WillMessage.Topic);
                writer.WriteWithLengthPrefix(packet.WillMessage.Payload);
            }

            if (packet.Username != null)
            {
                writer.WriteWithLengthPrefix(packet.Username);
            }

            if (packet.Password != null)
            {
                writer.WriteWithLengthPrefix(packet.Password);
            }

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.Connect);
        }

        private byte Serialize(MqttConnAckPacket packet, MqttPacketWriter writer)
        {
            if (ProtocolVersion == MqttProtocolVersion.V310)
            {
                writer.Write(0);
            }
            else if (ProtocolVersion == MqttProtocolVersion.V311)
            {
                var connectAcknowledgeFlags = new ByteWriter();
                connectAcknowledgeFlags.Write(packet.IsSessionPresent);
                writer.Write(connectAcknowledgeFlags);
            }
            else
            {
                throw new MqttProtocolViolationException("Protocol version not supported.");
            }

            writer.Write((byte)packet.ConnectReturnCode);

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.ConnAck);
        }

        private static byte Serialize(MqttPubRelPacket packet, MqttPacketWriter writer)
        {
            if (!packet.PacketIdentifier.HasValue)
            {
                throw new MqttProtocolViolationException("PubRel packet has no packet identifier.");
            }

            writer.Write(packet.PacketIdentifier.Value);

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.PubRel, 0x02);
        }

        private static byte Serialize(MqttPublishPacket packet, MqttPacketWriter writer)
        {
            ValidatePublishPacket(packet);

            writer.WriteWithLengthPrefix(packet.Topic);

            if (packet.QualityOfServiceLevel > MqttQualityOfServiceLevel.AtMostOnce)
            {
                if (!packet.PacketIdentifier.HasValue)
                {
                    throw new MqttProtocolViolationException("Publish packet has no packet identifier.");
                }

                writer.Write(packet.PacketIdentifier.Value);
            }
            else
            {
                if (packet.PacketIdentifier > 0)
                {
                    throw new MqttProtocolViolationException("Packet identifier must be empty if QoS == 0 [MQTT-2.3.1-5].");
                }
            }

            if (packet.Payload?.Length > 0)
            {
                writer.Write(packet.Payload);
            }

            byte fixedHeader = 0;

            if (packet.Retain)
            {
                fixedHeader |= 0x01;
            }

            fixedHeader |= (byte)((byte)packet.QualityOfServiceLevel << 1);

            if (packet.Dup)
            {
                fixedHeader |= 0x08;
            }

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.Publish, fixedHeader);
        }

        private static byte Serialize(MqttPubAckPacket packet, MqttPacketWriter writer)
        {
            if (!packet.PacketIdentifier.HasValue)
            {
                throw new MqttProtocolViolationException("PubAck packet has no packet identifier.");
            }

            writer.Write(packet.PacketIdentifier.Value);

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.PubAck);
        }

        private static byte Serialize(MqttPubRecPacket packet, MqttPacketWriter writer)
        {
            if (!packet.PacketIdentifier.HasValue)
            {
                throw new MqttProtocolViolationException("PubRec packet has no packet identifier.");
            }

            writer.Write(packet.PacketIdentifier.Value);

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.PubRec);
        }

        private static byte Serialize(MqttPubCompPacket packet, MqttPacketWriter writer)
        {
            if (!packet.PacketIdentifier.HasValue)
            {
                throw new MqttProtocolViolationException("PubComp packet has no packet identifier.");
            }

            writer.Write(packet.PacketIdentifier.Value);

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.PubComp);
        }

        private static byte Serialize(MqttSubscribePacket packet, MqttPacketWriter writer)
        {
            if (!packet.TopicFilters.Any()) throw new MqttProtocolViolationException("At least one topic filter must be set [MQTT-3.8.3-3].");

            if (!packet.PacketIdentifier.HasValue)
            {
                throw new MqttProtocolViolationException("Subscribe packet has no packet identifier.");
            }

            writer.Write(packet.PacketIdentifier.Value);

            if (packet.TopicFilters?.Count > 0)
            {
                foreach (var topicFilter in packet.TopicFilters)
                {
                    writer.WriteWithLengthPrefix(topicFilter.Topic);
                    writer.Write((byte)topicFilter.QualityOfServiceLevel);
                }
            }

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.Subscribe, 0x02);
        }

        private static byte Serialize(MqttSubAckPacket packet, MqttPacketWriter writer)
        {
            if (!packet.PacketIdentifier.HasValue)
            {
                throw new MqttProtocolViolationException("SubAck packet has no packet identifier.");
            }

            writer.Write(packet.PacketIdentifier.Value);

            if (packet.SubscribeReturnCodes?.Any() == true)
            {
                foreach (var packetSubscribeReturnCode in packet.SubscribeReturnCodes)
                {
                    writer.Write((byte)packetSubscribeReturnCode);
                }
            }

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.SubAck);
        }

        private static byte Serialize(MqttUnsubscribePacket packet, MqttPacketWriter writer)
        {
            if (!packet.TopicFilters.Any()) throw new MqttProtocolViolationException("At least one topic filter must be set [MQTT-3.10.3-2].");

            if (!packet.PacketIdentifier.HasValue)
            {
                throw new MqttProtocolViolationException("Unsubscribe packet has no packet identifier.");
            }

            writer.Write(packet.PacketIdentifier.Value);

            if (packet.TopicFilters?.Any() == true)
            {
                foreach (var topicFilter in packet.TopicFilters)
                {
                    writer.WriteWithLengthPrefix(topicFilter);
                }
            }

            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.Unsubscibe, 0x02);
        }

        private static byte Serialize(MqttUnsubAckPacket packet, BinaryWriter writer)
        {
            if (!packet.PacketIdentifier.HasValue)
            {
                throw new MqttProtocolViolationException("UnsubAck packet has no packet identifier.");
            }

            writer.Write(packet.PacketIdentifier.Value);
            return MqttPacketWriter.BuildFixedHeader(MqttControlPacketType.UnsubAck);
        }

        private static byte SerializeEmptyPacket(MqttControlPacketType type)
        {
            return MqttPacketWriter.BuildFixedHeader(type);
        }
    }
}
