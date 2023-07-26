using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.GraphQL.Repository
{
    public class GraphQLRepository
    {
        private readonly GraphQLHttpClient _client;

        public GraphQLRepository(GraphQLHttpClient client)
        {
            _client = client;
        }
        public async Task<T> Mutation<T>(string query)
        {
            var response =  await _client.SendMutationAsync<JObject>(query);
            return JsonConvert.DeserializeObject<T>(response?.Data?.ToString());
        }
    }
}
