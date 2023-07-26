using Camunda.API.Command;
using Camunda.Domain.Service;
using MediatR;

namespace Camunda.API.CommandHandler
{

    public class IncrementSalaryCommandHandler : IRequestHandler<IncrementSalaryCommand, bool>
    {
        private readonly SalaryRepository _salaryRepository;

        public IncrementSalaryCommandHandler(SalaryRepository salaryRepository)
        {
            _salaryRepository = salaryRepository;
        }

        public async Task<bool> Handle(IncrementSalaryCommand request, CancellationToken cancellationToken)
        {
            return await _salaryRepository.UpdateSalaryEveryThirtyMinute();
        }
    }
}
