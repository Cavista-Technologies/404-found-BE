using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Queries.DashboardAnalytics
{
    public class GetTimeToFillTrendsQuery : IRequest<ApiResponse> 
    {

    }
    public class MonthlyAverageModel
    {
        public string Month { get; set; }
        public double AverageDays { get; set; }
    }
    public class DepartmentAverageModel
    {
        public string Department { get; set; }
        public double AverageDays { get; set; }
    }
    public class TimeToFillTrendsModel
    {
        public List<MonthlyAverageModel> MonthlyTrend { get; set; }
        public List<DepartmentAverageModel> ByDepartment { get; set; }
    }
    public class GetTimeToFillTrendsHandler : IRequestHandler<GetTimeToFillTrendsQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetTimeToFillTrendsHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetTimeToFillTrendsQuery request, CancellationToken cancellationToken)
        {
            var filled = await _context.JobRoles
                .Include(x => x.Department)
                .Where(x => x.Status == JobStatus.Filled && x.FilledAt != null)
                .Select(x => new
                {
                    x.FilledAt,
                    StartedAt = x.PublishedAt ?? x.CreatedAt,
                    Department = x.Department.Name
                })
                .ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var monthlyTrend = new List<MonthlyAverageModel>();
            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var inMonth = filled.Where(x => x.FilledAt.Value.Month == month.Month && x.FilledAt.Value.Year == month.Year).ToList();
                monthlyTrend.Add(new MonthlyAverageModel
                {
                    Month = month.ToString("MMM"),
                    AverageDays = inMonth.Count == 0 ? 0 : Math.Round(inMonth.Average(x => (x.FilledAt.Value - x.StartedAt).TotalDays), 1)
                });
            }
            var byDepartment = filled
                .GroupBy(x => x.Department)
                .Select(g => new DepartmentAverageModel
                {
                    Department = g.Key,
                    AverageDays = Math.Round(g.Average(x => (x.FilledAt.Value - x.StartedAt).TotalDays), 1)
                })
                .OrderByDescending(x => x.AverageDays)
                .ToList();
            var result = new TimeToFillTrendsModel { MonthlyTrend = monthlyTrend, ByDepartment = byDepartment };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Trends retrieved", result);
        }
    }
}
