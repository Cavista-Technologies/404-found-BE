using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Queries.JobRoles
{
    public class GetJobRoleTimelineQuery : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
    }
    public class TimelineEventModel
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string CandidateName { get; set; }
        public string Actor { get; set; }
        public DateTime Date { get; set; }
    }
    public class GetJobRoleTimelineHandler : IRequestHandler<GetJobRoleTimelineQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetJobRoleTimelineHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetJobRoleTimelineQuery request, CancellationToken cancellationToken)
        {
            var job = await _context.JobRoles.FirstOrDefaultAsync(x => x.Id == request.JobRoleId, cancellationToken);
            if (job == null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Role not found");
            var events = new List<TimelineEventModel>();

            if (job.PublishedAt.HasValue)
            {
                events.Add(new TimelineEventModel
                {
                    Type = "RolePublished",
                    Description = "Role published",
                    CandidateName = null,
                    Actor = job.RecruiterName,
                    Date = job.PublishedAt.Value
                });
            }
            var stageChanges = await _context.ApplicationStageHistories
                .Include(h => h.Application).ThenInclude(a => a.Candidate)
                .Include(h => h.ChangedBy)
                .Where(h => h.Application.JobRoleId == request.JobRoleId)
                .OrderBy(h => h.ChangedOn)
                .Select(h => new TimelineEventModel
                {
                    Type = "StageChange",
                    Description = $"Candidate moved {h.FromStage} to {h.ToStage}",
                    CandidateName = h.Application.Candidate.FirstName + " " + h.Application.Candidate.LastName,
                    Actor = h.ChangedBy != null ? h.ChangedBy.FirstName + " " + h.ChangedBy.LastName : "System",
                    Date = h.ChangedOn
                })
                .ToListAsync(cancellationToken);
            events.AddRange(stageChanges);
            var ordered = events.OrderBy(e => e.Date).ToList();
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Timeline retrieved", ordered);
        }
    }
}
