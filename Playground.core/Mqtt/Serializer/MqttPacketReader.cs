using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Exceptions;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace MQTTnet.Serializer
{
    public sealed class MqttPacketReader : BinaryReader
    {
        private readonly MqttPacketHeader _header;
        
        public MqttPacketReader(MqttPacketHeader header, Stream bodyStream)
            : base(bodyStream, Encoding.UTF8, true)
        {
            _header = header;
        }

        public bool EndOfRemainingData => BaseStream.Position == _header.BodyLength;

        public static MqttPacketHeader ReadHeader(ref ReadOnlySequence<byte> input)
        {
            if (input.Length < 2)
            {
                return null;
            }

            var fixedHeader = input.First.Span[0];
            var controlPacketType = (MqttControlPacketType)(fixedHeader >> 4);
            var bodyLength = ReadBodyLength(ref input);

            return new MqttPacketHeader
            {
                FixedHeader = fixedHeader,
                ControlPacketType = controlPacketType,
                BodyLength = bodyLength
            };
        }

        public override ushort ReadUInt16()
        {
            var buffer = ReadBytes(2);

            var temp = buffer[0];
            buffer[0] = buffer[1];
            buffer[1] = temp;

            return BitConverter.ToUInt16(buffer, 0);
        }

        public string ReadStringWithLengthPrefix()
        {
            var buffer = ReadWithLengthPrefix();
            if (buffer.Length == 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        public byte[] ReadWithLengthPrefix()
        {
            var length = ReadUInt16();
            if (length == 0)
            {
                return new byte[0];
            }

            return ReadBytes(length);
        }

        public byte[] ReadRemainingData()
        {
            return ReadBytes(_header.BodyLength - (int)BaseStream.Position);
        }

        private static int ReadBodyLength(ref ReadOnlySequence<byte> input)
        {
            // Alorithm taken from https://docs.oasis-open.org/mqtt/mqtt/v3.1.1/errata01/os/mqtt-v3.1.1-errata01-os-complete.html.
            var multiplier = 1;
            var value = 0;
            byte encodedByte;
            var index = 1;

            var readBytes = new List<byte>();
            do
            {
                encodedByte = input.First.Span[index];
                readBytes.Add(encodedByte);
                index++;

                value += (byte)(encodedByte & 127) * multiplier;
                if (multiplier > 128 * 128 * 128)
                {
                    throw new MqttProtocolViolationException($"Remaining length is invalid (Data={string.Join(",", readBytes)}).");
                }

                multiplier *= 128;
            } while ((encodedByte & 128) != 0);

            input = input.Slice(index);

            return value;
        }
    }
}
