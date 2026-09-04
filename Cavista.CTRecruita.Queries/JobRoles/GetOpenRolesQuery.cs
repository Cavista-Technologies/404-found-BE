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
        public int NumberOfOpenings { get; set; }
        public int SlaTargetDays { get; set; }
        public DateTime? TargetHireDate { get; set; }
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
                .Select(x => new GetRoleDetailQueryModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Department = x.Department.Name,
                    EmploymentType = x.EmploymentType,
                    EmploymentTypeStr = x.EmploymentType.GetDescription(),
                    Status = x.Status,
                    StatusStr = x.Status.GetDescription(),
                    Priority = x.Priority,
                    PriorityStr = x.Priority.GetDescription(),
                    NumberOfOpenings = x.NumberOfOpenings,
                    SlaTargetDays = x.SlaTargetDays,
                    RecruiterName = x.RecruiterName,
                    HiringManagerName = x.HiringManagerName,
                    HiringManagerEmail = x.HiringManagerEmail,
                    //SlaPercent = x.SlaTargetDays > 0
                    //    ? Math.Min(100, (int)(EF.Functions.DateDiffDay(x.CreatedAt, DateTime.UtcNow) * 100.0 / x.SlaTargetDays))
                    //    : 0,
                    TargetHireDate = x.TargetHireDate,
                    //PipelineCount = x.Applications.Count(a => a.Status == ApplicationStatus.Active),
                    ApplicantsCount = x.Applications.Count()
                })
                .PaginateAsync(request.Page, request.PageLength);
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Roles retrieved", roles);
        }
    }
}
