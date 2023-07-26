using Camunda.API.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Camunda.API.Controllers.API
{
    [Route("api/v1/")]
    [ApiController]
    public class EmployeeController : ApiControllerBase
    {
        public EmployeeController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("create")]
        public async void Post([FromBody]EmplyoeeCommand command)
        {
            await base.HandleAsync(command);
        }
        [HttpPatch("incrementSalary")]
        public async void Patch()
        {
            var command = new IncrementSalaryCommand();
            await base.HandleAsync(command);
        }
    }
}
