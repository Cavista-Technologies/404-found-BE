using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Forms;
using Cavista.CTRecruita.Data.Entities.Roles;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Cavista.CTRecruita.Commands.Applications
{
    public class SubmitApplicationCommand : IRequest<ApiResponse>
    {
        public string Slug { get; set; }
        public ApplicationSource Source { get; set; } = ApplicationSource.Direct;
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AnswersJson { get; set; }
        public List<IFormFile> Files { get; set; } = new();
        public List<long> FileFieldIds { get; set; } = new();
    }
    public class AnswerDto
    {
        public long FormFieldId { get; set; }
        public string Value { get; set; }
    }
    public class SubmitApplicationHandler : IRequestHandler<SubmitApplicationCommand, ApiResponse>
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;
        private const string UploadFolder = "uploads/applications";
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        public SubmitApplicationHandler(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<ApiResponse> Handle(SubmitApplicationCommand request, CancellationToken cancellationToken)
        {
            var form = await _context.ApplicationForms
                .Include(f => f.Fields)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Slug == request.Slug && f.Status == FormStatus.Published, cancellationToken);
            if (form is null)
                return new ApiResponse(true, StatusCodes.Status404NotFound, "Application form not found");
            if (request.Files.Count != request.FileFieldIds.Count)
                return new ApiResponse(true, StatusCodes.Status400BadRequest, "Each uploaded file must have a matching field id");
            if (!TryParseAnswers(request.AnswersJson, out var answers))
                return new ApiResponse(true, StatusCodes.Status400BadRequest, "Answers payload is not valid JSON");
            var fieldsById = form.Fields.ToDictionary(f => f.Id);
            var fileError = ValidateFiles(request, fieldsById);
            if (fileError != null)
                return new ApiResponse(true, StatusCodes.Status400BadRequest, fileError);
            var missing = GetMissingRequiredFields(form.Fields, request, answers);
            if (missing.Count != 0)
                return new ApiResponse(true, StatusCodes.Status400BadRequest, $"Missing required: {string.Join(", ", missing)}");
            var savedFiles = await SaveFilesAsync(request, cancellationToken);
            var application = new Application
            {
                JobRoleId = form.JobRoleId,
                Source = request.Source,
                Stage = ApplicationStage.Applied,
                Status = ApplicationStatus.Active,
                AppliedOn = DateTime.UtcNow,
                Candidate = new Candidate
                {
                    FirstName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.Phone
                },
                Answers = answers
                    .Where(a => fieldsById.TryGetValue(a.FormFieldId, out var f) && !f.IsStandard)
                    .Select(a => new ApplicationAnswer { FormFieldId = a.FormFieldId, Value = a.Value })
                    .Concat(savedFiles)
                    .ToList()
            };
            _context.Applications.Add(application);
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, StatusCodes.Status201Created, "Application submitted");
        }
        private static bool TryParseAnswers(string json, out List<AnswerDto> answers)
        {
            answers = new List<AnswerDto>();
            if (string.IsNullOrWhiteSpace(json))
                return true;
            try
            {
                answers = JsonSerializer.Deserialize<List<AnswerDto>>(json) ?? new List<AnswerDto>();
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
        private static string ValidateFiles(SubmitApplicationCommand request, Dictionary<long, FormField> fieldsById)
        {
            for (int i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                var fieldId = request.FileFieldIds[i];
                if (!fieldsById.TryGetValue(fieldId, out var field))
                    return $"Unknown field id {fieldId}";
                if (field.FieldType != FormFieldType.FileUpload)
                    return $"'{field.Label}' does not accept a file";
                if (file.Length == 0)
                    return $"'{field.Label}' file is empty";
                if (file.Length > MaxFileSizeBytes)
                    return $"'{field.Label}' exceeds the 5MB limit";
                if (!AllowedExtensions.Contains(Path.GetExtension(file.FileName)))
                    return $"'{field.Label}' must be one of: {string.Join(", ", AllowedExtensions)}";
            }
            return null;
        }
        private static List<string> GetMissingRequiredFields( ICollection<FormField> fields, SubmitApplicationCommand request, List<AnswerDto> answers)
        {
            var answered = answers
                .Where(a => !string.IsNullOrWhiteSpace(a.Value))
                .Select(a => a.FormFieldId)
                .Concat(request.FileFieldIds)
                .ToHashSet();
            var missing = new List<string>();
            foreach (var f in fields.Where(f => f.IsRequired).OrderBy(f => f.SortOrder))
            {
                bool satisfied = f.IsStandard
                    ? f.FieldType switch
                    {
                        FormFieldType.Email => !string.IsNullOrWhiteSpace(request.Email),
                        FormFieldType.Phone => !string.IsNullOrWhiteSpace(request.Phone),
                        FormFieldType.FileUpload => answered.Contains(f.Id),
                        _ => !string.IsNullOrWhiteSpace(request.FullName)
                    }
                    : answered.Contains(f.Id);
                if (!satisfied)
                    missing.Add(f.Label);
            }
            return missing;
        }
        private async Task<List<ApplicationAnswer>> SaveFilesAsync(SubmitApplicationCommand request, CancellationToken cancellationToken)
        {
            if (request.Files.Count == 0)
                return new List<ApplicationAnswer>();
            var uploadRoot = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "applications");
            Directory.CreateDirectory(uploadRoot);
            var saved = new List<ApplicationAnswer>(request.Files.Count);
            for (int i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                var storedName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
                await using var stream = new FileStream(Path.Combine(uploadRoot, storedName), FileMode.Create);
                await file.CopyToAsync(stream, cancellationToken);
                saved.Add(new ApplicationAnswer
                {
                    FormFieldId = request.FileFieldIds[i],
                    Value = $"/{UploadFolder}/{storedName}"
                });
            }
            return saved;
        }
    }
}
