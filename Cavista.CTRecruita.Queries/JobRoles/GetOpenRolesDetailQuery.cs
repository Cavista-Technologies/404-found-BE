using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Enums.EnumExtensions;
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

    public class GetRoleDetailQueryModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public string EmploymentTypeStr { get; set; }
        public JobStatus Status { get; set; }
        public string StatusStr { get; set; }
        public  JobPriority Priority { get; set; }
        public string PriorityStr { get; set; }
        public int NumberOfOpenings { get; set; }
        public int SlaTargetDays { get; set; }
        public DateTime? TargetHireDate { get; set; }
        public int SlaPercent { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterEmail { get; set; }
        public string HiringManagerName { get; set; }
        public string HiringManagerEmail { get; set; }
        public string SalaryRange { get; set; }
        public string Location { get; set; }
        public int ApplicantsCount { get; set; }
        public Dictionary<string, List<object>> Pipeline { get; set; } = new();
        public bool HasApplicationForm { get; set; }
        public long? ApplicationFormId { get; set; }
        public string? ApplicationFormSlug { get; set; }
        public FormStatus? ApplicationFormStatus { get; set; }
        public string? ApplicationFormStatusStr { get; set; }
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
            var form = await _context.ApplicationForms
                .Where(f => f.JobRoleId == role.Id)
                .Select(f => new { f.Id, f.Status, f.Slug })
                .FirstOrDefaultAsync(cancellationToken);


            var result = new GetRoleDetailQueryModel
            {
                Id = role.Id,
                Title = role.Title,
                Department = role.Department.Name,
                EmploymentType = role.EmploymentType,
                EmploymentTypeStr = role.EmploymentType.GetDescription(),
                Status = role.Status,
                StatusStr = role.Status.GetDescription(),
                Priority = role.Priority,
                PriorityStr = role.Priority.GetDescription(),
                NumberOfOpenings = role.NumberOfOpenings,
                SlaTargetDays = role.SlaTargetDays,
                TargetHireDate = role.TargetHireDate,
                SlaPercent = slaPercent,
                RecruiterName = role.RecruiterName,
                RecruiterEmail = role.RecruiterEmail,
                HiringManagerName = role.HiringManagerName,
                HiringManagerEmail = role.HiringManagerEmail,
                SalaryRange = role.SalaryRange,
                Location = role.Location,
                ApplicantsCount = role.Applications.Count,
                Pipeline = pipeline,
                HasApplicationForm = form != null,
                ApplicationFormId = form?.Id,
                ApplicationFormSlug = form?.Slug,
                ApplicationFormStatus = form?.Status,
                ApplicationFormStatusStr = form?.Status.GetDescription()
            };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Role retrieved", result);
        }
    }
}
