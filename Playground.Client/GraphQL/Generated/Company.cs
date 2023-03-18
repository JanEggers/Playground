namespace Playground.Client.GraphQL
{
    using System;
    using System.Collections.Generic;

    public partial class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Site> Sites { get; set; }
    }
}