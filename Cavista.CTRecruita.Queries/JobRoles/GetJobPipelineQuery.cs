using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
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
    public class GetJobRolePipelineQuery : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
    public class PipelineItemModel
    {
        public long Id { get; set; }
        public string CandidateName { get; set; }
        public string Email { get; set; }
        public ApplicationStage Stage { get; set; }
        public ApplicationSource Source { get; set; }
        public int DaysInStage { get; set; }
    }
    public class PipelineResultModel
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<PipelineItemModel> Items { get; set; }
    }
    public class GetJobRolePipelineHandler : IRequestHandler<GetJobRolePipelineQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetJobRolePipelineHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetJobRolePipelineQuery request, CancellationToken cancellationToken)
        {
            var baseQuery = _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.StageHistory)
                .Where(a => a.JobRoleId == request.JobRoleId && a.Status != ApplicationStatus.Rejected && a.Status != ApplicationStatus.Withdrawn);
            var total = await baseQuery.CountAsync(cancellationToken);
            var raw = await baseQuery
                .OrderByDescending(a => a.AppliedOn)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new
                {
                    a.Id,
                    CandidateName = a.Candidate.FirstName + " " + a.Candidate.LastName,
                    a.Candidate.Email,
                    a.Stage,
                    a.Source,
                    LastMovedOn = a.StageHistory.OrderByDescending(h => h.ChangedOn).Select(h => h.ChangedOn).FirstOrDefault()
                })
                .ToListAsync(cancellationToken);
            var items = raw.Select(x => new PipelineItemModel
            {
                Id = x.Id,
                CandidateName = x.CandidateName,
                Email = x.Email,
                Stage = x.Stage,
                Source = x.Source,
                DaysInStage = (int)(DateTime.UtcNow - (x.LastMovedOn == default ? DateTime.UtcNow : x.LastMovedOn)).TotalDays
            }).ToList();
            var result = new PipelineResultModel
            {
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Items = items
            };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Pipeline retrieved", result);
        }
    }
}
