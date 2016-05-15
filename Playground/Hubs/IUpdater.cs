using System.Collections.Generic;

namespace Playground.Hubs
{
    public interface IUpdater
    {
        void Update(string selfLink, Dictionary<string, object> changes);
    }
}
