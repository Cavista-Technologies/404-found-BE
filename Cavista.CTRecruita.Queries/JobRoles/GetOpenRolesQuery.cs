using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Enums.EnumExtensions;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Extension;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Cavista.CTRecruita.Queries.JobRoles
{
    public class GetOpenRolesQuery : IRequest<ApiResponse>
    {
        public string Search { get; set; }
        public long? DepartmentId { get; set; }
        public JobStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageLength { get; set; } = 10;
    }
    public class GetOpenRolesModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public string EmploymentTypeStr { get; set; }
        public JobStatus Status { get; set; }
        public string StatusStr { get; set; }
        public JobPriority Priority { get; set; }
        public string PriorityStr { get; set; }
        public string RecruiterName { get; set; }
        public string HiringManagerName { get; set; }
        public string HiringManagerEmail { get; set; }
        public int NumberOfOpenings { get; set; }
        public int SlaTargetDays { get; set; }
        public DateTime? TargetHireDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int DaysOpen { get; set; }
        public int SlaPercent { get; set; }
        public bool IsSlaBreached { get; set; }
        public int PipelineCount { get; set; }
        public int ApplicantsCount { get; set; }
    }
    public class GetOpenRolesHandler : IRequestHandler<GetOpenRolesQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetOpenRolesHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetOpenRolesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.JobRoles
                .Include(x => x.Department)
                .Where(x => !x.IsDeleted)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(x => x.Title.Contains(request.Search));
            if (request.DepartmentId.HasValue)
                query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);
            var roles = await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new GetOpenRolesModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Department = x.Department.Name,
                    EmploymentType = x.EmploymentType,
                    EmploymentTypeStr = x.EmploymentType.GetDescription(),
                    Status = x.Status,
                    StatusStr = x.Status.GetDescription(),
                    Priority = x.Priority,
                    RecruiterName = x.RecruiterName,
                    HiringManagerName = x.HiringManagerName,
                    HiringManagerEmail = x.HiringManagerEmail,
                    PriorityStr = x.Priority.GetDescription(),
                    NumberOfOpenings = x.NumberOfOpenings,
                    SlaTargetDays = x.SlaTargetDays,
                    TargetHireDate = x.TargetHireDate,
                    CreatedAt = x.CreatedAt,
                    PublishedAt = x.PublishedAt,
                    ApplicantsCount = x.Applications.Count()
                })
                .PaginateAsync(request.Page, request.PageLength);
            var now = DateTime.UtcNow;
            foreach (var role in roles.Items)
            {
                var start = role.PublishedAt ?? role.CreatedAt;
                var daysOpen = (int)(now - start).TotalDays;
                if (daysOpen < 0) daysOpen = 0;
                role.DaysOpen = daysOpen;
                if (role.SlaTargetDays > 0)
                {
                    var pct = daysOpen * 100 / role.SlaTargetDays;
                    role.SlaPercent = pct > 100 ? 100 : pct;
                    role.IsSlaBreached = daysOpen > role.SlaTargetDays;
                }
            }
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Roles retrieved", roles);
        }
    }
}