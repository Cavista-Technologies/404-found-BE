using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Commands.Roles
{
    public class DeleteJobRoleCommand : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
    }
    public class DeleteJobRoleHandler : IRequestHandler<DeleteJobRoleCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public DeleteJobRoleHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(DeleteJobRoleCommand request, CancellationToken cancellationToken)
        {
            var job = await _context.JobRoles.FirstOrDefaultAsync(x => x.Id == request.JobRoleId, cancellationToken);
            if (job == null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Role not found");
            job.IsDeleted = true;
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Role deleted");
        }
    }
}
