using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Form;
using Cavista.CTRecruita.Data.Entities.Forms;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Cavista.CTRecruita.Commands.Applications
{
    public class CreateApplicationFormCommand : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
        public string Title { get; set; }
        public string IntroMessage { get; set; }
        public List<FormFieldDto> Fields { get; set; }
    }
    public class FormFieldDto
    {
        public string Label { get; set; }
        public string Placeholder { get; set; }
        public FormFieldType FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsStandard { get; set; }
        public List<string> Options { get; set; }
    }
    public class CreateApplicationFormHandler : IRequestHandler<CreateApplicationFormCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public CreateApplicationFormHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(CreateApplicationFormCommand request, CancellationToken cancellationToken)
        {
            var job = await _context.JobRoles.FirstOrDefaultAsync(x => x.Id == request.JobRoleId, cancellationToken);
            if (job is null)
                return new ApiResponse(true, StatusCodes.Status404NotFound, "Job not found");

            if (job.Status != JobStatus.Open)
                return new ApiResponse(true, (int)StatusCodes.Status400BadRequest, "Role must be active before an application form can be created");

            var exists = await _context.ApplicationForms.AnyAsync(f => f.JobRoleId == request.JobRoleId, cancellationToken);
            if (exists)
                return new ApiResponse(true, StatusCodes.Status409Conflict, "This job already has an application form");
            var form = new ApplicationForm
            {
                JobRoleId = request.JobRoleId,
                Title = request.Title,
                IntroMessage = request.IntroMessage,
                Status = FormStatus.Draft,
                Slug = BuildSlug(job.Title),
                Fields = request.Fields.Select(MapField).ToList()
            };
            _context.ApplicationForms.Add(form);
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, StatusCodes.Status201Created, "Form created successfully");
        }
        private FormField MapField(FormFieldDto f) => new FormField
        {
            Label = f.Label,
            Placeholder = f.Placeholder,
            FieldType = f.FieldType,
            IsRequired = f.IsRequired,
            SortOrder = f.SortOrder,
            IsStandard = f.IsStandard,
            OptionsJson = f.Options != null ? JsonSerializer.Serialize(f.Options) : null
        };
        private string BuildSlug(string title)
        {
            var baseSlug = title.ToLower().Replace(" ", "-");
            return $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";
        }
    }
}
