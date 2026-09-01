using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Commands.Roles
{
    public class PublishJobRoleCommand : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
    }
    public class PublishJobRoleHandler : IRequestHandler<PublishJobRoleCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public PublishJobRoleHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(PublishJobRoleCommand request, CancellationToken cancellationToken)
        {
            var job = await _context.JobRoles.FirstOrDefaultAsync(x => x.Id == request.JobRoleId, cancellationToken);
            if (job is null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Role not found");
            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(job.RecruiterName)) missing.Add("Recruiter");
            if (string.IsNullOrWhiteSpace(job.HiringManagerName)) missing.Add("Hiring Manager");
            if (job.TargetHireDate is null) missing.Add("Target Hire Date");
            if (missing.Count != 0)
                return new ApiResponse(true, (int)StatusCodes.Status400BadRequest,
                    $"Complete these to publish: {string.Join(", ", missing)}");
            job.Status = JobStatus.Open;
            job.PublishedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Role published");
        }
    }
}
