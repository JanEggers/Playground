using System;

namespace Playground.core.Hubs
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MqttAttribute : Attribute
    {
        public string Topic { get; set; }
    }
}
