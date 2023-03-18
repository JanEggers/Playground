namespace Playground.Client.GraphQL
{
    using GraphQLinq;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    public partial class QueryContext : GraphContext
    {
        public QueryContext() : this("http://localhost:5000/graphql")
        {
        }

        public QueryContext(string baseUrl) : base(baseUrl, "")
        {
        }

        public QueryContext(HttpClient httpClient) : base(httpClient)
        {
        }

        public GraphCollectionQuery<Company> Companies(CompanyFilterInput where, List<CompanySortInput> order)
        {
            var parameterValues = new object[] { where, order };
            return BuildCollectionQuery<Company>(parameterValues, "companies");
        }
    }
}