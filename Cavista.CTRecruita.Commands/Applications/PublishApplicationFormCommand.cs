using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Form;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Commands.Applications
{
    public class PublishApplicationFormCommand : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
        public string Title { get; set; }
        public string IntroMessage { get; set; }
        public List<FormFieldDto> Fields { get; set; }
    }
    public class PublishApplicationFormHandler : IRequestHandler<PublishApplicationFormCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public PublishApplicationFormHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(PublishApplicationFormCommand request, CancellationToken cancellationToken)
        {
            var job = await _context.JobRoles.FirstOrDefaultAsync(x => x.Id == request.JobRoleId, cancellationToken);
            if (job is null)
                return new ApiResponse(true, StatusCodes.Status404NotFound, "Role not found");
            if (job.Status != JobStatus.Open)
                return new ApiResponse(true, StatusCodes.Status400BadRequest, "Role must be active before its form can be published");
            if (request.Fields == null || request.Fields.Count == 0)
                return new ApiResponse(true, StatusCodes.Status400BadRequest, "Cannot publish a form with no fields");
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(job.RecruiterName)) missing.Add("Recruiter");
            if (string.IsNullOrWhiteSpace(job.HiringManagerName)) missing.Add("Hiring Manager");
            if (job.TargetHireDate is null) missing.Add("Target Hire Date");
            if (missing.Count != 0)
                return new ApiResponse(true, StatusCodes.Status400BadRequest,
                    $"Complete these on the role to publish: {string.Join(", ", missing)}");
            var form = await _context.ApplicationForms
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.JobRoleId == request.JobRoleId, cancellationToken);
            if (form is null)
            {
                form = new ApplicationForm
                {
                    JobRoleId = request.JobRoleId,
                    Slug = ApplicationFormMapper.BuildSlug(job.Title)
                };
                _context.ApplicationForms.Add(form);
            }
            else
            {
                if (form.Status == FormStatus.Published)
                    return new ApiResponse(true, StatusCodes.Status400BadRequest, "This form is already published");
                _context.FormFields.RemoveRange(form.Fields);
            }
            form.Title = request.Title;
            form.IntroMessage = request.IntroMessage;
            form.Status = FormStatus.Published;
            form.Fields = request.Fields.Select(ApplicationFormMapper.MapField).ToList();
            form.UpdatedAt = DateTime.UtcNow;
            job.PublishedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, StatusCodes.Status200OK, "Form and role published successfully");
        }
    }
}
