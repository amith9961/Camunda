using Camunda.API.Command;
using MediatR;

namespace Camunda.API.CommandHandler
{
    public class CreateUserCommandHandler : IRequestHandler<EmplyoeeCommand, EmployeeResponse>

    {
        public Task<EmployeeResponse> Handle(EmplyoeeCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
