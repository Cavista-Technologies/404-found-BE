using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Forms;
using Cavista.CTRecruita.Data.Entities.Roles;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Commands.Applications
{
    public class SubmitApplicationCommand : IRequest<ApiResponse>
    {
        public string Slug { get; set; }
        public ApplicationSource Source { get; set; } = ApplicationSource.Direct;
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<AnswerDto> Answers { get; set; }
    }
    public class AnswerDto
    {
        public long FormFieldId { get; set; }
        public string Value { get; set; }
    }
    public class SubmitApplicationHandler : IRequestHandler<SubmitApplicationCommand, ApiResponse>
    {
        private readonly ApplicationContext _context;
        public SubmitApplicationHandler(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(SubmitApplicationCommand request, CancellationToken cancellationToken)
        {
            var form = await _context.ApplicationForms
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Slug == request.Slug && f.Status == FormStatus.Published, cancellationToken);

            if (form == null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Application form not found");

            var answered = request.Answers.Select(a => a.FormFieldId).ToHashSet();
            var missing = form.Fields.Where(f => f.IsRequired && !answered.Contains(f.Id)).Select(f => f.Label).ToList();
            if (missing.Count != 0)
                return new ApiResponse(true, (int)StatusCodes.Status400BadRequest, $"Missing required: {string.Join(", ", missing)}");

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
                Answers = request.Answers
                    .Where(a => form.Fields.Any(f => f.Id == a.FormFieldId && !f.IsStandard))
                    .Select(a => new ApplicationAnswer { FormFieldId = a.FormFieldId, Value = a.Value })
                    .ToList()
            };
            _context.Applications.Add(application);
            await _context.SaveChangesAsync(cancellationToken);
            return new ApiResponse(false, (int)StatusCodes.Status201Created, "Application submitted");
        }
    }
}
