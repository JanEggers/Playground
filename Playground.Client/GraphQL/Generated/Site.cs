namespace Playground.Client.GraphQL
{
    using System;
    using System.Collections.Generic;

    public partial class Site
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}