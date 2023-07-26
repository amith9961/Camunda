using MediatR;

namespace Camunda.API.Command
{
    public class IncrementSalaryCommand:IRequest<bool>
    {
    }
}
