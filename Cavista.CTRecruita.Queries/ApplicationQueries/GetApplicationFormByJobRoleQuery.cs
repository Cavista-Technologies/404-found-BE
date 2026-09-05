using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Enums.EnumExtensions;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Queries.ApplicationQueries
{
    public class GetApplicationFormByJobRoleQuery : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
    }
    public class ApplicationFormFieldModel
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string Placeholder { get; set; }
        public FormFieldType FieldType { get; set; }
        public string FieldTypeStr { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsStandard { get; set; }
        public List<string> Options { get; set; }
    }
    public class GetApplicationFormByJobRoleModel
    {
        public long Id { get; set; }
        public long JobRoleId { get; set; }
        public string JobRoleTitle { get; set; }
        public string Title { get; set; }
        public string IntroMessage { get; set; }
        public FormStatus Status { get; set; }
        public string StatusStr { get; set; }
        public string Slug { get; set; }
        public int TotalFields { get; set; }
        public int RequiredFields { get; set; }
        public int OptionalFields { get; set; }
        public List<ApplicationFormFieldModel> Fields { get; set; } = new();
    }
    public class GetApplicationFormByJobRoleHandler : IRequestHandler<GetApplicationFormByJobRoleQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetApplicationFormByJobRoleHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetApplicationFormByJobRoleQuery request, CancellationToken cancellationToken)
        {
            var form = await _context.ApplicationForms
                .Include(f => f.Fields)
                .Include(f => f.JobRole)
                .FirstOrDefaultAsync(f => f.JobRoleId == request.JobRoleId, cancellationToken);
            if (form is null)
                return new ApiResponse(true, StatusCodes.Status404NotFound, "No application form found for this role");
            var fields = form.Fields
                .OrderBy(x => x.SortOrder)
                .Select(x => new ApplicationFormFieldModel
                {
                    Id = x.Id,
                    Label = x.Label,
                    Placeholder = x.Placeholder,
                    FieldType = x.FieldType,
                    FieldTypeStr = x.FieldType.GetDescription(),
                    IsRequired = x.IsRequired,
                    SortOrder = x.SortOrder,
                    IsStandard = x.IsStandard,
                    Options = x.OptionsJson != null ? JsonSerializer.Deserialize<List<string>>(x.OptionsJson) : new List<string>()
                })
                .ToList();
            var result = new GetApplicationFormByJobRoleModel
            {
                Id = form.Id,
                JobRoleId = form.JobRoleId,
                JobRoleTitle = form.JobRole.Title,
                Title = form.Title,
                IntroMessage = form.IntroMessage,
                Status = form.Status,
                StatusStr = form.Status.GetDescription(),
                Slug = form.Slug,
                TotalFields = fields.Count,
                RequiredFields = fields.Count(f => f.IsRequired),
                OptionalFields = fields.Count(f => !f.IsRequired),
                Fields = fields
            };
            return new ApiResponse(false, StatusCodes.Status200OK, "Form retrieved", result);
        }
    }
}
