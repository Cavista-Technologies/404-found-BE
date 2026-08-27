using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Roles;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Commands.Roles
{
    public class CreateJobCommand : IRequest<ApiResponse>
    {
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
        public bool Activate { get; set; }   
    }
    public class CreateJobHandler : IRequestHandler<CreateJobCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public CreateJobHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
            if (department is null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Department not found");
            if (request.Activate)
            {
                var missing = new List<string>();
                if (string.IsNullOrWhiteSpace(request.RecruiterName)) missing.Add("Recruiter");
                if (string.IsNullOrWhiteSpace(request.HiringManagerName)) missing.Add("Hiring Manager");
                if (request.TargetHireDate is null) missing.Add("Target Hire Date");
                if (missing.Count != 0)
                    return new ApiResponse(true, (int)StatusCodes.Status400BadRequest, $"Complete these to activate: {string.Join(", ", missing)}");
            }
            var job = new JobRole
            {
                Title = request.Title,
                Description = request.Description,
                Location = request.Location,
                EmploymentType = request.EmploymentType,
                Priority = request.Priority,
                DepartmentId = request.DepartmentId,
                RecruiterName = request.RecruiterName,
                RecruiterEmail = request.RecruiterEmail,
                HiringManagerName = request.HiringManagerName,
                HiringManagerEmail = request.HiringManagerEmail,
                NumberOfOpenings = request.NumberOfOpenings,
                SlaTargetDays = request.SlaTargetDays,
                TargetHireDate = request.TargetHireDate,
                SalaryRange = request.SalaryRange,
                Reason = request.Reason,
                Status = request.Activate ? JobStatus.Open : JobStatus.Draft
            };
            _context.JobRoles.Add(job);
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status201Created, "Role created");
        }
    }
}
