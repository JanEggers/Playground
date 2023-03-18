namespace Playground.Client.GraphQL
{
    using System;
    using System.Collections.Generic;

    public partial class ListFilterInputTypeOfSiteFilterInput
    {
        public SiteFilterInput All { get; set; }
        public SiteFilterInput None { get; set; }
        public SiteFilterInput Some { get; set; }
        public bool? Any { get; set; }
    }
}