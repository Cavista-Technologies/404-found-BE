using Cavista.CTRecruita.Commands.Applications;
using Cavista.CTRecruita.Commands.Roles;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Queries.JobRoles;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Cavista.CTRecruita.Web.RequestModels.ApplicationModel;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("{id}/pipeline")]
        public async Task<IActionResult> GetJobPipeline(long id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var response = await _mediator.Send(new GetJobRolePipelineQuery
            {
                JobRoleId= id,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            return PrepareResponse(response);
        }
        [HttpGet("{id}/applicants")]
        public async Task<IActionResult> GetJobApplicants(long id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var response = await _mediator.Send(new GetJobRoleApplicantsQuery
            {
                JobRoleId= id,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            return PrepareResponse(response);
        }

        [HttpGet("{id}/timeline")]
        public async Task<IActionResult> GetJobTimeline(long id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var response = await _mediator.Send(new GetJobRoleTimelineQuery
            {
                JobRoleId= id,
            });
            return PrepareResponse(response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(long id, UpdateJobCommand command)
        {
            command.JobRoleId = id;
            var response = await _mediator.Send(command);
            return PrepareResponse(response);
        }

        [HttpPut("candidates/{candidateId}/stage")]
        public async Task<IActionResult> UpdateApplicationStage( long candidateId, MoveApplicationStageCommand request)
        {
            var response = await _mediator.Send(
                new MoveApplicationStageCommand
                {
                    ApplicationId = request.ApplicationId,
                    CandidateId = candidateId,
                    CurrentUserId = CurrentUserId,
                    ToStage = request.ToStage,
                    Reason = request.Reason
                });

            return PrepareResponse(response);
        }

        [HttpDelete("archive/{id}")]
        public async Task<IActionResult> DeleteJobRole(long id)
        {
            var response = await _mediator.Send(new DeleteJobRoleCommand
            {
                JobRoleId = id
            });
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
        public async Task<IActionResult> GetOpenRoles([FromQuery] JobStatus? jobStatus, [FromQuery] long? departmentId, int? pageLength, int? page, [FromQuery] string? searchString = null)
        {
            var roles = await _mediator.Send( new  GetOpenRolesQuery 
            {
                Status = jobStatus,
                DepartmentId = departmentId,
                Search = searchString,
                Page = page ?? 1,
                PageLength = pageLength ?? 10,
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
