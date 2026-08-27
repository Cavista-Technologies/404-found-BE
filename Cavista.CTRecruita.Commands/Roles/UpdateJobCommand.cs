using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Commands.Roles
{
    public class UpdateJobCommand : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public JobPriority Priority { get; set; }
        public long DepartmentId { get; set; }
        public string RecruiterName { get; set; }
        public string RecruiterEmail { get; set; }
        public string HiringManagerName { get; set; }
        public string HiringManagerEmail { get; set; }
        public int NumberOfOpenings { get; set; }
        public int SlaTargetDays { get; set; }
        public DateTime? TargetHireDate { get; set; }
        public string SalaryRange { get; set; }
        public string Reason { get; set; }
    }
    public class UpdateJobHandler : IRequestHandler<UpdateJobCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public UpdateJobHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _context.JobRoles.FirstOrDefaultAsync(x => x.Id == request.JobRoleId, cancellationToken);
            if (job is null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Role not found");
            var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
            if (department is null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Department not found");
            job.Title = request.Title;
            job.Description = request.Description;
            job.Location = request.Location;
            job.EmploymentType = request.EmploymentType;
            job.Priority = request.Priority;
            job.DepartmentId = request.DepartmentId;
            job.RecruiterName = request.RecruiterName;
            job.RecruiterEmail = request.RecruiterEmail;
            job.HiringManagerName = request.HiringManagerName;
            job.HiringManagerEmail = request.HiringManagerEmail;
            job.NumberOfOpenings = request.NumberOfOpenings;
            job.SlaTargetDays = request.SlaTargetDays;
            job.TargetHireDate = request.TargetHireDate;
            job.SalaryRange = request.SalaryRange;
            job.Reason = request.Reason;
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Role updated");
        }
    }
}