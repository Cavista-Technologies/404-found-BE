using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Queries.DashboardAnalytics
{
    public class GetHiringSnapshotQuery : IRequest<ApiResponse> { }
    public class HiringSnapshotModel
    {
        public int OpenRoles { get; set; }
        public int RolesFilledThisQuarter { get; set; }
        public double AverageTimeToFillDays { get; set; }
        public double TimeToFillDeltaVsLastMonth { get; set; }
        public int AtRiskCount { get; set; }
    }
    public class GetHiringSnapshotHandler : IRequestHandler<GetHiringSnapshotQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetHiringSnapshotHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetHiringSnapshotQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var quarterStartMonth = ((now.Month - 1) / 3) * 3 + 1;
            var quarterStart = new DateTime(now.Year, quarterStartMonth, 1);
            var openRoles = await _context.JobRoles.CountAsync(x => !x.IsDeleted && x.Status == JobStatus.Open, cancellationToken);

            var filledThisQuarter = await _context.JobRoles.CountAsync(
                x => x.Status == JobStatus.Filled && x.FilledAt >= quarterStart, cancellationToken);

            var filledRoles = await _context.JobRoles
                .Where(x => x.Status == JobStatus.Filled && x.FilledAt != null)
                .Select(x => new { x.FilledAt, StartedAt = x.PublishedAt ?? x.CreatedAt })
                .ToListAsync(cancellationToken);

            var thisMonth = filledRoles.Where(x => x.FilledAt.Value.Month == now.Month && x.FilledAt.Value.Year == now.Year).ToList();
            var lastMonthDate = now.AddMonths(-1);
            var lastMonth = filledRoles.Where(x => x.FilledAt.Value.Month == lastMonthDate.Month && x.FilledAt.Value.Year == lastMonthDate.Year).ToList();
            var avgThisMonth = thisMonth.Count == 0 ? 0 : thisMonth.Average(x => (x.FilledAt.Value - x.StartedAt).TotalDays);
            var avgLastMonth = lastMonth.Count == 0 ? 0 : lastMonth.Average(x => (x.FilledAt.Value - x.StartedAt).TotalDays);
            var overallAvg = filledRoles.Count == 0 ? 0 : filledRoles.Average(x => (x.FilledAt.Value - x.StartedAt).TotalDays);

            var atRisk = await _context.JobRoles
                .Where(x => x.Status == JobStatus.Open)
                .Select(x => new { x.TargetHireDate, x.SlaTargetDays, x.PublishedAt, x.CreatedAt })
                .ToListAsync(cancellationToken);
            var atRiskCount = atRisk.Count(x =>
                (x.TargetHireDate.HasValue && x.TargetHireDate.Value.Date < now.Date) ||
                (x.SlaTargetDays > 0 && (now - (x.PublishedAt ?? x.CreatedAt)).TotalDays / x.SlaTargetDays >= 0.9));
            var result = new HiringSnapshotModel
            {
                OpenRoles = openRoles,
                RolesFilledThisQuarter = filledThisQuarter,
                AverageTimeToFillDays = Math.Round(thisMonth.Count > 0 ? avgThisMonth : overallAvg, 1),
                TimeToFillDeltaVsLastMonth = Math.Round(avgThisMonth - avgLastMonth, 1),
                AtRiskCount = atRiskCount
            };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Snapshot retrieved", result);
        }
    }
}
