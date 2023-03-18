namespace Playground.Client.GraphQL
{
    using System;
    using System.Collections.Generic;

    public partial class SiteFilterInput
    {
        public List<SiteFilterInput> And { get; set; }
        public List<SiteFilterInput> Or { get; set; }
        public IntOperationFilterInput Id { get; set; }
        public StringOperationFilterInput Name { get; set; }
        public IntOperationFilterInput CompanyId { get; set; }
        public CompanyFilterInput Company { get; set; }
    }
}