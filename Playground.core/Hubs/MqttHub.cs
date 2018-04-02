using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Playground.core.Hubs
{
    public class MqttHub : Hub
    {
        private readonly ILogger<MqttHub> logger;

        public MqttHub(ILogger<MqttHub> logger)
        {
            this.logger = logger;
        }

        public MqttConnAckPacket Connect(MqttConnectPacket connect)
        {
            return new MqttConnAckPacket()
            {
                ConnectReturnCode = MQTTnet.Protocol.MqttConnectReturnCode.ConnectionAccepted
            };
        }


        public MqttBasePacket OnPublish(MqttPublishPacket publish)
        {
            return new MqttPubAckPacket()
            {
                PacketIdentifier = publish.PacketIdentifier,

            };
        }

        public MqttBasePacket OnPing(MqttPingReqPacket ping)
        {
            return new MqttPingRespPacket()
            {
            };
        }

        public IObservable<MqttBasePacket> OnSubscribe(IObservable<MqttPublishPacket> packets, MqttSubscribePacket sub)
        {
            var ack = new MqttSubAckPacket()
            {
                PacketIdentifier = sub.PacketIdentifier,
            };

            foreach (var topic in sub.TopicFilters)
            {
                ack.SubscribeReturnCodes.Add(MqttSubscribeReturnCode.SuccessMaximumQoS0);
            }


            var filteredPublishPackets = packets
                //.Do(p => logger.LogInformation($"publish {p.Topic}"))
                .Where(p =>
                {
                    var result = sub.TopicFilters.Any(f => MqttTopicFilterComparer.IsMatch(p.Topic, f.Topic));

                    // logger.LogInformation($"filter {p.Topic}: {result}");

                    return result;
                });

            return Observable.Return<MqttBasePacket>(ack)
                .Concat(filteredPublishPackets);
        }

        public override Task OnConnectedAsync()
        {
            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception ex)
        {
            return Task.CompletedTask;
        }
    }
}
