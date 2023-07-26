using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Camunda.API.Controllers.API
{
    public abstract class ApiControllerBase : ControllerBase
    {
        private readonly IMediator _mediator;

        protected ApiControllerBase(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected async Task<TResponse> HandleAsync<TResponse>(IRequest<TResponse> request)
        {
            return await _mediator.Send(request);
        }
    }
}
