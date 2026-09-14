using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Roles;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Emailer;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cavista.CTRecruita.Commands.Applications
{
    public class MoveApplicationStageCommand : IRequest<ApiResponse>
    {
        public long ApplicationCandidateId { get; set; }
        public long CurrentUserId { get; set; }
        public ApplicationStage ToStage { get; set; }
        public string? Reason { get; set; }
    }
    public class MoveApplicationStageHandler : IRequestHandler<MoveApplicationStageCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<MoveApplicationStageHandler> _logger;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        public MoveApplicationStageHandler(ApplicationContext context, IBackgroundJobClient backgroundJobClient, ILogger<MoveApplicationStageHandler> logger, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
            _config = configuration;
            _emailService = emailService;
        }
        public async Task<ApiResponse> Handle(MoveApplicationStageCommand request, CancellationToken cancellationToken)
        {
            var applicationCandidate = await _context.ApplicationCandidates
                .Include(x => x.StageHistory)
                .Include(x => x.Application)
                    .ThenInclude(a => a.JobRole)
                .FirstOrDefaultAsync(x => x.Id == request.ApplicationCandidateId, cancellationToken);
            if (applicationCandidate == null)
            {
                return new ApiResponse(true, StatusCodes.Status404NotFound, "Candidate application not found");
            }
            var fromStage = applicationCandidate.Stage;
            if (fromStage == request.ToStage)
            {
                return new ApiResponse(true, StatusCodes.Status400BadRequest, $"Candidate is already in the {request.ToStage} stage");
            }
            applicationCandidate.Stage = request.ToStage;
            var jobRole = applicationCandidate.Application?.JobRole;
            switch (request.ToStage)
            {
                case ApplicationStage.Hired:
                    applicationCandidate.Status = ApplicationStatus.Hired;
                    if (jobRole != null)
                    {
                        jobRole.Status = JobStatus.Filled;
                        jobRole.FilledAt = DateTime.UtcNow;
                        jobRole.UpdatedAt = DateTime.UtcNow;
                    }
                    break;
                case ApplicationStage.Rejected:
                    applicationCandidate.Status = ApplicationStatus.Rejected;
                    break;
                case ApplicationStage.Withdrawn:
                    applicationCandidate.Status = ApplicationStatus.Withdrawn;
                    break;
                default:
                    applicationCandidate.Status = ApplicationStatus.Active;
                    if (fromStage == ApplicationStage.Hired && jobRole != null)
                    {
                        jobRole.Status = JobStatus.Open;
                        jobRole.FilledAt = null;
                        jobRole.UpdatedAt = DateTime.UtcNow;
                    }
                    break;
            }
            applicationCandidate.StageHistory.Add(
                new ApplicationCandidateStageHistory
                {
                    ApplicationCandidateId = applicationCandidate.Id,
                    FromStage = fromStage,
                    ToStage = request.ToStage,
                    Reason = request.Reason,
                    ChangedOn = DateTime.UtcNow,
                    ChangedById = request.CurrentUserId
                });
            await _context.SaveChangesAsync(cancellationToken);
            _backgroundJobClient.Enqueue<MoveApplicationStageHandler>(
                handler => handler.SendApplicationStageMovedEmail(applicationCandidate.Id, fromStage, request.ToStage, request.Reason));
            return new ApiResponse(false, StatusCodes.Status200OK, "Candidate moved successfully");
        }
        public async Task SendApplicationStageMovedEmail(long applicationCandidateId, ApplicationStage fromStage, ApplicationStage toStage, string? reason)
        {
            var applicationCandidate = await _context.ApplicationCandidates
                .Include(x => x.Candidate)
                .Include(x => x.Application)
                    .ThenInclude(a => a.JobRole)
                .FirstOrDefaultAsync(x => x.Id == applicationCandidateId);
            if (applicationCandidate?.Candidate == null)
            {
                _logger.LogWarning(
                    "SendApplicationStageMovedEmail skipped - candidate application {ApplicationCandidateId} not found",
                    applicationCandidateId);
                return;
            }
            var candidate = applicationCandidate.Candidate;
            var (templateKey, emailSubject) = ResolveEmailTemplate(toStage);
            var templatePath = _config[templateKey] ?? _config["EmailTemplates:ApplicationStageMovedEmail"];
            if (string.IsNullOrWhiteSpace(templatePath))
            {
                _logger.LogWarning(
                    "SendApplicationStageMovedEmail skipped - template key {TemplateKey} not configured for stage {ToStage}",
                    templateKey,
                    toStage);
                return;
            }
            var fullPath = Path.IsPathRooted(templatePath)
               ? templatePath
               : Path.Combine(AppContext.BaseDirectory, templatePath);
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning(
                    "SendApplicationStageMovedEmail skipped - template file not found at {FullPath} for stage {ToStage}",
                    fullPath,
                    toStage);
                return;
            }
            var hostUrl = _config["FileStorage:BaseUrl"];
            var appLink = _config["SPAURL"];
            var emailTemplate = await File.ReadAllTextAsync(fullPath);
            var emailBody = emailTemplate
                .Replace("{firstName}", candidate.FirstName)
                .Replace("{role}", applicationCandidate.Application?.JobRole?.Title ?? string.Empty)
                .Replace("{fromStage}", fromStage.ToString())
                .Replace("{toStage}", toStage.ToString())
                .Replace("{reason}", reason ?? "N/A")
                .Replace("{loginLink}", appLink)
                .Replace("{hostUrl}", hostUrl)
                .Replace("{year}", DateTime.Now.Year.ToString());
            await _emailService.SendMailAsync(emailSubject, emailBody, candidate.Email, false, null, null, null, null, null, null);
        }
        private static (string TemplateKey, string Subject) ResolveEmailTemplate(ApplicationStage stage) => stage switch
        {
            ApplicationStage.Interview => ("EmailTemplates:ApplicationInterviewEmail", "You have been shortlisted for an interview"),
            ApplicationStage.Offer => ("EmailTemplates:ApplicationOfferEmail", "You have an offer"),
            ApplicationStage.Hired => ("EmailTemplates:ApplicationHiredEmail", "Welcome aboard"),
            ApplicationStage.Rejected => ("EmailTemplates:ApplicationRejectedEmail", "Update on your application"),
            ApplicationStage.Withdrawn => ("EmailTemplates:ApplicationWithdrawnEmail", "Your application has been withdrawn"),
            _ => ("EmailTemplates:ApplicationStageMovedEmail", $"Your application has been moved to {stage}")
        };
    }
}
