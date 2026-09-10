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
        public long ApplicationCandidateId { get; set; }

        public long CurrentUserId { get; set; }

        public ApplicationStage ToStage { get; set; }

        public string? Reason { get; set; }
    }

    public class MoveApplicationStageHandler : IRequestHandler<MoveApplicationStageCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public MoveApplicationStageHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle( MoveApplicationStageCommand request, CancellationToken cancellationToken)
        {
            var applicationCandidate = await _context.ApplicationCandidates
                .Include(x => x.StageHistory)
                .FirstOrDefaultAsync( x => x.Id == request.ApplicationCandidateId, cancellationToken);

            if (applicationCandidate == null)
            {
                return new ApiResponse(
                    true,
                    StatusCodes.Status404NotFound,
                    "Candidate application not found");
            }

            var fromStage = applicationCandidate.Stage;

            applicationCandidate.Stage = request.ToStage;

            switch (request.ToStage)
            {
                case ApplicationStage.Hired:
                    applicationCandidate.Status = ApplicationStatus.Hired;
                    break;

                case ApplicationStage.Rejected:
                    applicationCandidate.Status = ApplicationStatus.Rejected;
                    break;

                case ApplicationStage.Withdrawn:
                    applicationCandidate.Status = ApplicationStatus.Withdrawn;
                    break;

                default:
                    applicationCandidate.Status = ApplicationStatus.Active;
                    break;
            }

            applicationCandidate.StageHistory.Add(
                new ApplicationCandidateStageHistory
                {
                    ApplicationCandidateId = applicationCandidate.Id,
                    FromStage = fromStage,
                    ToStage = request.ToStage,
                    Reason = request.Reason,
                    ChangedOn = DateTime.UtcNow,
                    ChangedById = request.CurrentUserId
                });

            await _context.SaveChangesAsync(cancellationToken);

            return new ApiResponse(
                false,
                StatusCodes.Status200OK,
                "Candidate moved successfully");
        }
    }
}