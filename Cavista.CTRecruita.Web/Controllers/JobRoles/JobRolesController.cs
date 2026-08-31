using Cavista.CTRecruita.Commands.Roles;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Queries.Departments;
using Cavista.CTRecruita.Queries.JobRoles;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cavista.CTRecruita.Web.Controllers.JobRoles
{
    [Route("job-roles")]
    [ApiController]
    [Authorize]
    public class JobRolesController : BaseController
    {
        public JobRolesController(IMediator mediator) : base(mediator)
        {
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateJobCommand command)
        {
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(long id, UpdateJobCommand command)
        {
            command.JobRoleId = id;
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }
        [HttpPatch("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(long id, UpdateJobStatusCommand command)
        {
            command.JobRoleId = id;
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }

        [HttpGet("open-roles")]
        public async Task<IActionResult> GetOpenRoles([FromQuery] JobStatus? jobStatus, [FromQuery] long? departmentId, [FromQuery] string? searchString = null)
        {
            var roles = await _mediator.Send( new  GetOpenRolesQuery 
            {
                Status = jobStatus,
                DepartmentId = departmentId,
                Search = searchString
            });

            return PrepareResponse(roles);

        }

        [HttpGet("open-roles/{id}")]
        public async Task<IActionResult> GetAnOpenRoles(long id)
        {
            var departments = await _mediator.Send(new GetRoleDetailQuery
            {
                Id = id
            });

            return PrepareResponse(departments);
        }

    }
}
