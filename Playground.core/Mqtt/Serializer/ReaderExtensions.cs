using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using MQTTnet.Exceptions;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace MQTTnet.Serializer
{
    public static class ReaderExtensions 
    {
        public static ushort ReadUInt16(this in ReadOnlySpan<byte> input)
        {
            return BinaryPrimitives.ReadUInt16BigEndian(input);
        }

        public static ushort ReadByte(this in ReadOnlySpan<byte> input, ref int position)
        {
            var result = input.Slice(position, 1)[0];
            position += 1;
            return result;
        }

        public static ushort ReadUInt16(this in ReadOnlySpan<byte> input, ref int position)
        {
            var result = BinaryPrimitives.ReadUInt16BigEndian(input.Slice(position, 2));
            position += 2;
            return result;
        }

        public static string ReadStringWithLengthPrefix(this in ReadOnlySpan<byte> input, ref int position)
        {
            var buffer = input.ReadWithLengthPrefix(ref position);
            if (buffer.Length == 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        public static byte[] ReadWithLengthPrefix(this in ReadOnlySpan<byte> input, ref int position)
        {
            var length = input.ReadUInt16(ref position);
            if (length == 0)
            {
                return new byte[0];
            }

            var result = input.Slice(position, length).ToArray();
            position += length;
            return result;
        }


        public static MqttPacketHeader ReadHeader(this ref ReadOnlySequence<byte> input)
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
