using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Queries.JobRoles
{
    public class GetRoleDetailQuery : IRequest<ApiResponse>
    {
        public long Id { get; set; }
    }
    public class GetRoleDetailHandler : IRequestHandler<GetRoleDetailQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetRoleDetailHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetRoleDetailQuery request, CancellationToken cancellationToken)
        {
            var role = await _context.JobRoles
                .Include(x => x.Department)
                .Include(x => x.Applications)
                    .ThenInclude(a => a.Candidate)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (role is null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Role not found");
            var slaPercent = role.SlaTargetDays > 0
                ? Math.Min(100, (int)((DateTime.UtcNow - role.CreatedAt).TotalDays * 100.0 / role.SlaTargetDays))
                : 0;
            var pipeline = role.Applications
                .Where(a => a.Status == ApplicationStatus.Active)
                .GroupBy(a => a.Stage)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Select(a => new
                    {
                        a.Id,
                        CandidateName = $"{a.Candidate.FirstName} {a.Candidate.LastName}",
                        a.Candidate.Email,
                        a.Source
                    }).ToList<object>()
                );
            var result = new
            {
                role.Id,
                role.Title,
                Department = role.Department.Name,
                role.EmploymentType,
                role.Status,
                role.Priority,
                role.NumberOfOpenings,
                role.SlaTargetDays,
                role.TargetHireDate,
                SlaPercent = slaPercent,
                role.RecruiterName,
                role.RecruiterEmail,
                role.HiringManagerName,
                role.HiringManagerEmail,
                role.SalaryRange,
                role.Location,
                ApplicantsCount = role.Applications.Count,
                Pipeline = pipeline
            };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Role retrieved", result);
        }
    }
}
