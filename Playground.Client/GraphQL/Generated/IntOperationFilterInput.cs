namespace Playground.Client.GraphQL
{
    using System;
    using System.Collections.Generic;

    public partial class IntOperationFilterInput
    {
        public int? Eq { get; set; }
        public int? Neq { get; set; }
        public List<int> In { get; set; }
        public List<int> Nin { get; set; }
        public int? Gt { get; set; }
        public int? Ngt { get; set; }
        public int? Gte { get; set; }
        public int? Ngte { get; set; }
        public int? Lt { get; set; }
        public int? Nlt { get; set; }
        public int? Lte { get; set; }
        public int? Nlte { get; set; }
    }
}