using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Roles;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Commands.Departments
{
    public class CreateDepartmentCommand : IRequest<ApiResponse>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public CreateDepartmentHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var exists = await _context.Departments.AnyAsync(x => x.Name == request.Name, cancellationToken);
            if (exists)
                return new ApiResponse(true, (int)StatusCodes.Status409Conflict, "A department with this name already exists");
            var department = new Department
            {
                Name = request.Name,
                Description = request.Description
            };
            _context.Departments.Add(department);
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status201Created, "Department created");
        }
    }
}
