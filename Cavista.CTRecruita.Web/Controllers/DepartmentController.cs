using Cavista.CTRecruita.Commands.Departments;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cavista.CTRecruita.Web.Controllers
{
    [Route("department")]
    [ApiController]
    [Authorize]
    public class DepartmentController : BaseController
    {
        public DepartmentController(IMediator mediator) : base(mediator)
        {
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateDepartmentCommand command)
        {
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }
    }
}
