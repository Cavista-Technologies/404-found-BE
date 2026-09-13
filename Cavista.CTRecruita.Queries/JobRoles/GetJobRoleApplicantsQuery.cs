using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Enums.EnumExtensions;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Helper;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Cavista.CTRecruita.Queries.JobRoles
{
    public class GetJobRoleApplicantsQuery : IRequest<ApiResponse>
    {
        public long JobRoleId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool Export { get; set; } = false;
    }
    public class ApplicantItemModel
    {
        public long Id { get; set; }
        public string CandidateName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public ApplicationStage Stage { get; set; }
        public string StageStr { get; set; }
        public ApplicationStatus Status { get; set; }
        public string StatusStr { get; set; }
        public ApplicationSource Source { get; set; }
        public string SourceStr { get; set; }
        public DateTime AppliedOn { get; set; }
        public List<ApplicantFileModel> Files { get; set; } = new();
    }
    public class ApplicantFileModel
    {
        public string FieldName { get; set; }
        public string FileUrl { get; set; }
    }
    public class ApplicantsResultModel
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<ApplicantItemModel> Items { get; set; }
    }
    public class GetJobRoleApplicantsHandler : IRequestHandler<GetJobRoleApplicantsQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        private readonly IConfiguration _configuration;
        public GetJobRoleApplicantsHandler(ApplicationReadOnlyContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<ApiResponse> Handle(
            GetJobRoleApplicantsQuery request,
            CancellationToken cancellationToken)
        {
            var baseQuery = _context.ApplicationCandidates
                .AsNoTracking()
                .Include(ac => ac.Candidate)
                .Include(ac => ac.Application)
                .Include(ac => ac.Answers)
                    .ThenInclude(a => a.FormField)
                .Where(ac => ac.Application.JobRoleId == request.JobRoleId);
            if (request.Export)
            {
                var fields = await _context.FormFields
                    .AsNoTracking()
                    .Where(f => f.ApplicationForm.JobRoleId == request.JobRoleId && !f.IsStandard)
                    .OrderBy(f => f.SortOrder)
                    .Select(f => new { f.Id, f.Label, f.FieldType })
                    .ToListAsync(cancellationToken);
                var records = await baseQuery
                    .OrderByDescending(ac => ac.AppliedOn)
                    .Select(ac => new
                    {
                        CandidateName = ((ac.Candidate.FirstName ?? string.Empty) + " " + (ac.Candidate.LastName ?? string.Empty)).Trim(),
                        ac.Candidate.Email,
                        ac.Candidate.PhoneNumber,
                        ac.Stage,
                        ac.Status,
                        ac.Source,
                        ac.AppliedOn,
                        Answers = ac.Answers.Select(a => new { a.FormFieldId, a.Value }).ToList()
                    })
                    .ToListAsync(cancellationToken);
                var headers = new List<string>
               {
                   "Candidate Name", "Email", "Phone Number", "Stage", "Status", "Source", "Applied On"
               };
                var fieldColumns = new Dictionary<long, string>();
                var fileColumns = new HashSet<string>();
                foreach (var f in fields)
                {
                    var label = string.IsNullOrWhiteSpace(f.Label) ? $"Field {f.Id}" : f.Label.Trim();
                    var name = label;
                    var suffix = 2;
                    while (headers.Contains(name)) name = $"{label} ({suffix++})";
                    headers.Add(name);
                    fieldColumns[f.Id] = name;
                    if (f.FieldType == FormFieldType.FileUpload) fileColumns.Add(name);
                }
                var exportBaseUrl = (_configuration["FileStorage:BaseUrl"] ?? string.Empty).TrimEnd('/');
                var rows = records.Select(r =>
                {
                    var row = new Dictionary<string, object>
                    {
                        ["Candidate Name"] = r.CandidateName,
                        ["Email"] = r.Email,
                        ["Phone Number"] = r.PhoneNumber,
                        ["Stage"] = r.Stage.GetDescription(),
                        ["Status"] = r.Status.GetDescription(),
                        ["Source"] = r.Source.GetDescription(),
                        ["Applied On"] = r.AppliedOn.ToString("yyyy-MM-dd HH:mm")
                    };
                    foreach (var f in fields)
                    {
                        var values = r.Answers
                            .Where(a => a.FormFieldId == f.Id && !string.IsNullOrWhiteSpace(a.Value))
                            .Select(a =>
                            {
                                var v = a.Value.Trim();
                                if (f.FieldType == FormFieldType.FileUpload && !string.IsNullOrEmpty(exportBaseUrl) && v.StartsWith("/"))
                                    v = exportBaseUrl + v;
                                return v;
                            })
                            .ToList();
                        row[fieldColumns[f.Id]] = values.Count == 1
                            ? values[0]
                            : string.Join(", ", values);
                    }
                    return (IDictionary<string, object>)row;
                }).ToList();
                var fileBytes = ExcelGenerator.GenerateExcelBytes(
                    rows, headers, sheetName: "Applicants", hyperlinkColumns: fileColumns);
                return new ApiResponse(false, StatusCodes.Status200OK, "Export ready", new
                {
                    FileName = $"applicants_{request.JobRoleId}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileBytes = Convert.ToBase64String(fileBytes)
                });
            }
            var total = await baseQuery.CountAsync(cancellationToken);
            var items = await baseQuery
                .OrderByDescending(ac => ac.AppliedOn)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ac => new ApplicantItemModel
                {
                    Id = ac.Id,
                    CandidateName = ((ac.Candidate.FirstName ?? string.Empty) + " " + (ac.Candidate.LastName ?? string.Empty)).Trim(),
                    Email = ac.Candidate.Email,
                    PhoneNumber = ac.Candidate.PhoneNumber,
                    Stage = ac.Stage,
                    StageStr = ac.Stage.GetDescription(),
                    Status = ac.Status,
                    StatusStr = ac.Status.GetDescription(),
                    Source = ac.Source,
                    SourceStr = ac.Source.GetDescription(),
                    AppliedOn = ac.AppliedOn,
                    Files = ac.Answers
                        .Where(a => a.FormField.FieldType == FormFieldType.FileUpload)
                        .Select(a => new ApplicantFileModel
                        {
                            FieldName = a.FormField.Label,
                            FileUrl = a.Value
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);
            var baseUrl = (_configuration["FileStorage:BaseUrl"] ?? string.Empty).TrimEnd('/');
            if (!string.IsNullOrEmpty(baseUrl))
            {
                foreach (var item in items)
                    foreach (var file in item.Files)
                        if (!string.IsNullOrWhiteSpace(file.FileUrl) && file.FileUrl.StartsWith("/"))
                            file.FileUrl = baseUrl + file.FileUrl;
            }
            var result = new ApplicantsResultModel
            {
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Items = items
            };
            return new ApiResponse(false, StatusCodes.Status200OK, "Applicants retrieved", result);
        }
    }
}
