using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Forms;
using Cavista.CTRecruita.Data.Entities.Roles;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Cavista.CTRecruita.Commands.Applications
{
    public class SubmitApplicationCommand : IRequest<ApiResponse>
    {
        public string Slug { get; set; }
        public ApplicationSource Source { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> AnswersJson { get; set; } = new();
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
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
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
                .FirstOrDefaultAsync(
                    f => f.Slug == request.Slug &&
                         f.Status == FormStatus.Published,
                    cancellationToken);
            if (form is null)
                return new ApiResponse(
                    true,
                    (int)StatusCodes.Status404NotFound,
                    "Application form not found");
            if (request.Files.Count != request.FileFieldIds.Count)
                return new ApiResponse(
                    true,
                    (int)StatusCodes.Status400BadRequest,
                    "Each uploaded file must have a matching field id");
            if (!TryParseAnswers(request.AnswersJson, out var answers, out var jsonError))
                return new ApiResponse(
                    true,
                    (int)StatusCodes.Status400BadRequest,
                    $"Answers payload is not valid JSON: {jsonError}");
            var fieldsById = form.Fields.ToDictionary(f => f.Id);
            var unknown = answers
                .Select(a => a.FormFieldId)
                .Where(id => !fieldsById.ContainsKey(id))
                .Distinct()
                .ToList();
            if (unknown.Count != 0)
                return new ApiResponse(
                    true,
                    (int)StatusCodes.Status400BadRequest,
                    $"Unknown field ids: {string.Join(", ", unknown)}");
            var fileError = ValidateFiles(request, fieldsById);
            if (fileError != null)
                return new ApiResponse(
                    true,
                    (int)StatusCodes.Status400BadRequest,
                    fileError);
            var missing = GetMissingRequiredFields(
                form.Fields,
                request,
                answers);
            if (missing.Count != 0)
                return new ApiResponse(
                    true,
                    (int)StatusCodes.Status400BadRequest,
                    $"Missing required: {string.Join(", ", missing)}");
            var savedFiles = await SaveFilesAsync(
                request,
                cancellationToken);
            var (firstName, lastName) = SplitFullName(request.FullName);
            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(
                    x => x.Email == request.Email,
                    cancellationToken);
            if (candidate == null)
            {
                candidate = new Candidate
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = request.Email,
                    PhoneNumber = request.Phone
                };
                _context.Candidates.Add(candidate);
                await _context.SaveChangesAsync(cancellationToken);
            }
            var application = await _context.Applications
                .FirstOrDefaultAsync(
                    x => x.JobRoleId == form.JobRoleId,
                    cancellationToken);
            if (application == null)
            {
                application = new Application
                {
                    JobRoleId = form.JobRoleId,
                    Name = form.Title,
                    IsActive = true
                };
                _context.Applications.Add(application);
                await _context.SaveChangesAsync(cancellationToken);
            }
            var existingApplication = await _context.ApplicationCandidates
                .AnyAsync(
                    x => x.ApplicationId == application.Id &&
                         x.CandidateId == candidate.Id,
                    cancellationToken);
            if (existingApplication)
                return new ApiResponse(
                    true,
                    (int)StatusCodes.Status409Conflict,
                    "Candidate has already applied");
            var applicationCandidate = new ApplicationCandidate
            {
                ApplicationId = application.Id,
                CandidateId = candidate.Id,
                Stage = ApplicationStage.Applied,
                Status = ApplicationStatus.Active,
                AppliedOn = DateTime.UtcNow,
                Source = request.Source,
                Answers = answers
                    .Where(a => fieldsById.TryGetValue(a.FormFieldId, out var field) && !field.IsStandard)
                    .Select(a => new ApplicationAnswer
                    {
                        FormFieldId = a.FormFieldId,
                        Value = a.Value
                    })
                    .Concat(savedFiles)
                    .ToList(),
                StageHistory = new List<ApplicationCandidateStageHistory>
           {
               new ApplicationCandidateStageHistory
               {
                   FromStage = ApplicationStage.Applied,
                   ToStage = ApplicationStage.Applied,
                   ChangedOn = DateTime.UtcNow
               }
           }
            };
            _context.ApplicationCandidates.Add(applicationCandidate);
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(
                false,
                (int)StatusCodes.Status201Created,
                "Application submitted");
        }
        private static bool TryParseAnswers(List<string> parts, out List<AnswerDto> answers, out string? error)
        {
            answers = new List<AnswerDto>();
            error = null;
            var values = parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToList();
            if (values.Count == 0)
                return true;
            var payload = values.Count == 1 && values[0].StartsWith('[')
                ? values[0]
                : $"[{string.Join(",", values.Select(v => v.Trim('[', ']')))}]";
            try
            {
                answers = JsonSerializer.Deserialize<List<AnswerDto>>(payload, JsonOptions) ?? new List<AnswerDto>();
                return true;
            }
            catch (JsonException ex)
            {
                error = ex.Message;
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
        private static List<string> GetMissingRequiredFields(
            ICollection<FormField> fields,
            SubmitApplicationCommand request,
            List<AnswerDto> answers)
        {
            var answered = answers
                .Where(a => !string.IsNullOrWhiteSpace(a.Value))
                .Select(a => a.FormFieldId)
                .Concat(request.FileFieldIds)
                .ToHashSet();
            bool StandardSatisfied(FormField f) => f.FieldType switch
            {
                FormFieldType.Email => !string.IsNullOrWhiteSpace(request.Email),
                FormFieldType.Phone => !string.IsNullOrWhiteSpace(request.Phone),
                FormFieldType.FileUpload => answered.Contains(f.Id),
                _ => !string.IsNullOrWhiteSpace(request.FullName)
            };
            return fields
                .Where(f => f.IsRequired)
                .OrderBy(f => f.SortOrder)
                .Where(f => !(f.IsStandard ? StandardSatisfied(f) : answered.Contains(f.Id)))
                .Select(f => f.Label)
                .ToList();
        }
        private static (string? FirstName, string? LastName) SplitFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (null, null);
            var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => (null, null),
                1 => (parts[0], null),
                _ => (parts[0], parts[1])
            };
        }
        private async Task<List<ApplicationAnswer>> SaveFilesAsync(SubmitApplicationCommand request, CancellationToken cancellationToken)
        {
            if (request.Files.Count == 0)
                return new List<ApplicationAnswer>();
            var configuredPath = _configuration["FileStorage:UploadPath"] ?? UploadFolder;
            var uploadRoot = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(AppContext.BaseDirectory, configuredPath);
            Directory.CreateDirectory(uploadRoot);
            var publicPath = _configuration["FileStorage:PublicPath"] ?? $"/{UploadFolder}";
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
                    Value = $"{publicPath}/{storedName}"
                });
            }
            return saved;
        }
    }
}