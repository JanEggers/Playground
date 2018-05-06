using System;

namespace Playground.core.Mqtt.Signalr
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MqttAttribute : Attribute
    {
        public string Topic { get; set; }
    }
}
