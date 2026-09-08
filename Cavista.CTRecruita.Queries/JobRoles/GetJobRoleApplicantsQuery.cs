using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Enums.EnumExtensions;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Queries.JobRoles
{
    public class GetJobRoleApplicantsQuery : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
    public class ApplicantItemModel
    {
        public long Id { get; set; }
        public string CandidateName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public ApplicationStage Stage { get; set; }
        public string StageStr { get; set; }
        public ApplicationStatus Status { get; set; }
        public string StatusStr { get; set; }
        public ApplicationSource Source { get; set; }
        public string SourceStr { get; set; }
        public DateTime AppliedOn { get; set; }
    }
    public class ApplicantsResultModel
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<ApplicantItemModel> Items { get; set; }
    }
    public class GetJobRoleApplicantsHandler : IRequestHandler<GetJobRoleApplicantsQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetJobRoleApplicantsHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetJobRoleApplicantsQuery request, CancellationToken cancellationToken)
        {
            var baseQuery = _context.Applications
                .Include(a => a.Candidate)
                .Where(a => a.JobRoleId == request.JobRoleId);
            var total = await baseQuery.CountAsync(cancellationToken);
            var items = await baseQuery
                .OrderByDescending(a => a.AppliedOn)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new ApplicantItemModel
                {
                    Id = a.Id,
                    CandidateName = a.Candidate.FirstName + " " + a.Candidate.LastName,
                    Email = a.Candidate.Email,
                    PhoneNumber = a.Candidate.PhoneNumber,
                    Stage = a.Stage,
                    StageStr = a.Stage.GetDescription(),
                    Status = a.Status,
                    StatusStr = a.Status.GetDescription(),
                    Source = a.Source,
                    SourceStr = a.Source.GetDescription(),
                    AppliedOn = a.AppliedOn
                })
                .ToListAsync(cancellationToken);
            var result = new ApplicantsResultModel
            {
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Items = items
            };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Applicants retrieved", result);
        }
    }
}
