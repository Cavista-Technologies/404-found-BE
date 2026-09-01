using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Queries.ApplicationQueries
{
    public class GetOpenApplicationsQuery : IRequest<ApiResponse>
    {
        public long? DepartmentId { get; set; }
    }
    public class OpenApplicationItemModel
    {
        public string Title { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public string Slug { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
    public class GetOpenApplicationsHandler : IRequestHandler<GetOpenApplicationsQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetOpenApplicationsHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetOpenApplicationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.JobRoles
                .Include(x => x.Department)
                .Where(x => !x.IsDeleted && x.Status == JobStatus.Open && x.PublishedAt != null && _context.ApplicationForms.Any(f => f.JobRoleId == x.Id && f.Status == FormStatus.Published));
            if (request.DepartmentId.HasValue)
                query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            var roles = await query
                .OrderByDescending(x => x.PublishedAt)
                .Select(x => new OpenApplicationItemModel
                {
                    Title = x.Title,
                    Department = x.Department.Name,
                    Location = x.Location,
                    EmploymentType = x.EmploymentType,
                    Slug = _context.ApplicationForms.Where(f => f.JobRoleId == x.Id).Select(f => f.Slug).FirstOrDefault(),
                    PublishedAt = x.PublishedAt
                })
                .ToListAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Open roles retrieved", roles);
        }
    }
}
