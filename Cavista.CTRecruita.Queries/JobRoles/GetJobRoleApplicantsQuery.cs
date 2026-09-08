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

        public async Task<ApiResponse> Handle(
            GetJobRoleApplicantsQuery request,
            CancellationToken cancellationToken)
        {
            var baseQuery = _context.ApplicationCandidates
                .AsNoTracking()
                .Include(ac => ac.Candidate)
                .Include(ac => ac.Application)
                .Where(ac => ac.Application.JobRoleId == request.JobRoleId);

            var total = await baseQuery.CountAsync(cancellationToken);

            var items = await baseQuery
                .OrderByDescending(ac => ac.AppliedOn)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ac => new ApplicantItemModel
                {
                    Id = ac.Id,
                    CandidateName =
                        ((ac.Candidate.FirstName ?? string.Empty) + " " +
                         (ac.Candidate.LastName ?? string.Empty)).Trim(),
                    Email = ac.Candidate.Email,
                    PhoneNumber = ac.Candidate.PhoneNumber,
                    Stage = ac.Stage,
                    StageStr = ac.Stage.GetDescription(),
                    Status = ac.Status,
                    StatusStr = ac.Status.GetDescription(),
                    Source = ac.Source,
                    SourceStr = ac.Source.GetDescription(),
                    AppliedOn = ac.AppliedOn
                })
                .ToListAsync(cancellationToken);

            var result = new ApplicantsResultModel
            {
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Items = items
            };

            return new ApiResponse( false, StatusCodes.Status200OK, "Applicants retrieved", result);
        }
    }
}
