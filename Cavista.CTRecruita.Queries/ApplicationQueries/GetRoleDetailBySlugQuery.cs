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

namespace Cavista.CTRecruita.Queries.ApplicationQueries
{
    public class GetRoleDetailBySlugQuery : IRequest<ApiResponse>
    {
        public string Slug { get; set; }
    }
    public class RoleDetailBySlugModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public string Slug { get; set; }
    }
    public class GetRoleDetailBySlugHandler : IRequestHandler<GetRoleDetailBySlugQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetRoleDetailBySlugHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetRoleDetailBySlugQuery request, CancellationToken cancellationToken)
        {
            var form = await _context.ApplicationForms
                .Include(f => f.JobRole).ThenInclude(j => j.Department)
                .FirstOrDefaultAsync(f => f.Slug == request.Slug && f.Status == FormStatus.Published, cancellationToken);
            if (form == null || form.JobRole.Status != JobStatus.Open)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Role not found or no longer open");
            var result = new RoleDetailBySlugModel
            {
                Title = form.JobRole.Title,
                Description = form.JobRole.Description,
                Location = form.JobRole.Location,
                Department = form.JobRole.Department.Name,
                EmploymentType = form.JobRole.EmploymentType,
                Slug = form.Slug
            };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Role retrieved", result);
        }
    }
}
