using Cavista.CTRecruita.Commands.Departments;
using Cavista.CTRecruita.Queries.Departments;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cavista.CTRecruita.Web.Controllers.Lookups
{
    [Route("lookups")]
    [ApiController]
    public class LookupController : BaseController
    {
        public LookupController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _mediator.Send(new GetDepartmentsQuery {});

            return PrepareResponse(departments);
        }
    }
}
