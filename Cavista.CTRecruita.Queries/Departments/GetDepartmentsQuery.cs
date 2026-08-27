using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Queries.Departments
{
    public class GetDepartmentsQuery : IRequest<ApiResponse>
    {
    }
    public class GetDepartmentsHandler : IRequestHandler<GetDepartmentsQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetDepartmentsHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var departments = await _context.Departments
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name })
                .ToListAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Departments retrieved", departments);
        }
    }
}
