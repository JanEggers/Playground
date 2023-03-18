namespace Playground.Client.GraphQL
{
    using System;
    using System.Collections.Generic;

    public partial class CompanyFilterInput
    {
        public List<CompanyFilterInput> And { get; set; }
        public List<CompanyFilterInput> Or { get; set; }
        public IntOperationFilterInput Id { get; set; }
        public StringOperationFilterInput Name { get; set; }
        public ListFilterInputTypeOfSiteFilterInput Sites { get; set; }
    }
}