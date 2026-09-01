using Cavista.CTRecruita.Commands.Applications;
using Cavista.CTRecruita.Commands.Applications;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Cavista.CTRecruita.Web.RequestModels.ApplicationModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cavista.CTRecruita.Web.Controllers.Application
{
    [Route("application-form")]
    [ApiController]
    [Authorize]
    public class ApplicationFormsController : BaseController
    {
        public ApplicationFormsController(IMediator mediator) : base(mediator)
        {
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateApplicationFormCommand command)
        {
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }

        [HttpPost("submit-application")]
        public async Task<IActionResult> SubmitApplication(SubmitApplicationCommand command)
        {
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }

        [HttpPut("{id}/update-application-stage")]
        public async Task<IActionResult> UpdateApplicationStage(long id, UpdateApplicationStage request)
        {
            var response = await _mediator.Send(new MoveApplicationStageCommand
            {
                ApplicationId = id,
                CurrentUserId = CurrentUserId,
                ToStage = request.Stage,
                Reason = request.Reason,
            });
            return PrepareResponse(response);
        }
    }
}
