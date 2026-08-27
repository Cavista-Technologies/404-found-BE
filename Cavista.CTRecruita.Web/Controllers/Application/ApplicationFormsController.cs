using Cavista.CTRecruita.Commands.Forms;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
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
    }
}
