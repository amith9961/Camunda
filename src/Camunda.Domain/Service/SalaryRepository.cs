using Data.GraphQL.Repository;

namespace Camunda.Domain.Service
{
    public class SalaryRepository
    {
        private GraphQLRepository _repository;
        public SalaryRepository(GraphQLRepository repository)
        {
            _repository = repository;
        }
        public async Task<bool> UpdateSalaryEveryThirtyMinute()
        { 
            var query = @"mutation MyMutation {
  update_CounterTable(where: {id: {_eq: 1}}, _inc: {Salary: 50}) {
    affected_rows
  }
}";
           await _repository.Mutation<Response>(query);
            return true;
        }
    }
    public class Response
    {
        public ResponseData Data { get; set; }
    }

    public class ResponseData
    {
        public Details update_CounterTable { get; set; }
        
    }

    public class Details
    {
        public int affected_rows { get; set; }
    }
}
