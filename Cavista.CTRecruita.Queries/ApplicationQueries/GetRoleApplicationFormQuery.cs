using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Enums.EnumExtensions;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Cavista.CTRecruita.Queries.ApplicationQueries
{
    public class GetRoleApplicationFormQuery : IRequest<ApiResponse>
    {
        public string Slug { get; set; }
    }
    public class GetRoleApplicationFormFieldModel
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string Placeholder { get; set; }
        public FormFieldType FieldType { get; set; }
        public string FieldTypeStr { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public List<string> Options { get; set; } = new();
    }
    public class GetRoleApplicationFormModel
    {
        public long JobRoleId { get; set; }
        public string JobRoleTitle { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public string EmploymentTypeStr { get; set; }
        public int NumberOfOpenings { get; set; }
        public string SalaryRange { get; set; }
        public string Title { get; set; }
        public string IntroMessage { get; set; }
        public string Slug { get; set; }
        public List<GetRoleApplicationFormFieldModel> Fields { get; set; } = new();
    }
    public class GetRoleApplicationFormHandler : IRequestHandler<GetRoleApplicationFormQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetRoleApplicationFormHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetRoleApplicationFormQuery request, CancellationToken cancellationToken)
        {
            var form = await _context.ApplicationForms
                .Include(f => f.Fields)
                .Include(f => f.JobRole)
                    .ThenInclude(j => j.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Slug == request.Slug && f.Status == FormStatus.Published, cancellationToken);
            if (form is null || form.JobRole.Status != JobStatus.Open)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Form not found or no longer accepting applications");
            var result = new GetRoleApplicationFormModel
            {
                JobRoleId = form.JobRoleId,
                JobRoleTitle = form.JobRole.Title,
                Description = form.JobRole.Description,
                Department = form.JobRole.Department.Name,
                Location = form.JobRole.Location,
                EmploymentType = form.JobRole.EmploymentType,
                EmploymentTypeStr = form.JobRole.EmploymentType.GetDescription(),
                NumberOfOpenings = form.JobRole.NumberOfOpenings,
                SalaryRange = form.JobRole.SalaryRange,
                Title = form.Title,
                IntroMessage = form.IntroMessage,
                Slug = form.Slug,
                Fields = form.Fields
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new GetRoleApplicationFormFieldModel
                    {
                        Id = x.Id,
                        Label = x.Label,
                        Placeholder = x.Placeholder,
                        FieldType = x.FieldType,
                        FieldTypeStr = x.FieldType.GetDescription(),
                        IsRequired = x.IsRequired,
                        SortOrder = x.SortOrder,
                        Options = x.OptionsJson != null
                            ? JsonSerializer.Deserialize<List<string>>(x.OptionsJson)
                            : new List<string>()
                    })
                    .ToList()
            };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Form retrieved", result);
        }
    }
}
