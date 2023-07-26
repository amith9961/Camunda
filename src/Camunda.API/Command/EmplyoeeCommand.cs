using MediatR;

namespace Camunda.API.Command
{
    public class EmplyoeeCommand:IRequest<EmployeeResponse>
    {
        public int Salary { get; set; }
        public string Name { get; set; }
    }

    public class EmployeeResponse
    {
        public int Id { get; set; }
    }
}
