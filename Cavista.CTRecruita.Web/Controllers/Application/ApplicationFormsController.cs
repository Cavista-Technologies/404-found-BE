using Cavista.CTRecruita.Commands.Applications;
using Cavista.CTRecruita.Queries.ApplicationQueries;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Authorization;
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
        [HttpPost("{jobRoleId}/save-application-form")]
        public async Task<IActionResult> SaveApplicationForm(long jobRoleId, CreateApplicationFormCommand command)
        {
            command.JobRoleId = jobRoleId;
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }
        [HttpPost("{jobRoleId}/publish-application-form")]
        public async Task<IActionResult> PublishApplicationForm(long jobRoleId, PublishApplicationFormCommand command)
        {
            command.JobRoleId = jobRoleId;
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }

        [HttpPost("submit-application")]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitApplication([FromForm] SubmitApplicationCommand command)
        {
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }

        [HttpGet("open")]
        public async Task<IActionResult> GetOpenApplications([FromQuery] long? departmentId)
        {
            var response = await _mediator.Send(new GetOpenApplicationsQuery { DepartmentId = departmentId });
            return PrepareResponse(response);
        }
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var response = await _mediator.Send(new GetRoleDetailBySlugQuery { Slug = slug });
            return PrepareResponse(response);
        }

        [HttpGet("public/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublic(string slug)
        {
            var response = await _mediator.Send(new GetRoleApplicationFormQuery { Slug = slug });
            return PrepareResponse(response);
        }

        [HttpGet("{jobRoleId}/application-form")]
        public async Task<IActionResult> GetApplicationFormByJobRole(long jobRoleId)
        {
            var response = await _mediator.Send(new GetApplicationFormByJobRoleQuery { JobRoleId = jobRoleId });
            return PrepareResponse(response);

        }
    }
}
