namespace Playground.core.Odata;

public class PlaygroundModelBuilder : ODataConventionModelBuilder
{
    public PlaygroundModelBuilder()
    {
        EntitySet<Company>(nameof(PlaygroundContext.Companies));
        EntitySet<Site>(nameof(PlaygroundContext.Sites));
    }
}
