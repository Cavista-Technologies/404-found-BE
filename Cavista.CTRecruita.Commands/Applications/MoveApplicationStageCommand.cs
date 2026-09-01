using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Roles;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Commands.Applications
{
    public class MoveApplicationStageCommand : IRequest<ApiResponse>
    {
        public long ApplicationId { get; set; }
        public long CurrentUserId { get; set; }
        public ApplicationStage ToStage { get; set; }
        public string Reason { get; set; }
    }
    public class MoveApplicationStageHandler : IRequestHandler<MoveApplicationStageCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public MoveApplicationStageHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(MoveApplicationStageCommand request, CancellationToken cancellationToken)
        {
            var application = await _context.Applications
                .Include(x => x.StageHistory)
                .FirstOrDefaultAsync(x => x.Id == request.ApplicationId, cancellationToken);
            if (application == null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Application not found");

            var fromStage = application.Stage;
            application.Stage = request.ToStage;
            if (request.ToStage == ApplicationStage.Hired)
                application.Status = ApplicationStatus.Hired;
            else if (request.ToStage == ApplicationStage.Rejected)
                application.Status = ApplicationStatus.Rejected;
            else if (request.ToStage == ApplicationStage.Withdrawn)
                application.Status = ApplicationStatus.Withdrawn;

            application.StageHistory.Add(new ApplicationStageHistory
            {
                ApplicationId = application.Id,
                FromStage = fromStage,
                ToStage = request.ToStage,
                Reason = request.Reason,
                ChangedOn = DateTime.UtcNow,
                ChangedById = request.CurrentUserId
            });
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Application moved");
        }
    }
}