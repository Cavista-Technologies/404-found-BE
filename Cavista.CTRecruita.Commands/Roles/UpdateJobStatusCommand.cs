using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Commands.Roles
{
    public class UpdateJobStatusCommand : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
        public JobStatus Status { get; set; }
    }
    public class UpdateJobStatusHandler : IRequestHandler<UpdateJobStatusCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public UpdateJobStatusHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(UpdateJobStatusCommand request, CancellationToken cancellationToken)
        {
            var job = await _context.JobRoles.FirstOrDefaultAsync(x => x.Id == request.JobRoleId, cancellationToken);
            if (job is null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Job not found");
            job.Status = request.Status;
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status200OK, $"Job status updated to {request.Status}");
        }
    }
}