using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Enums.EnumExtensions;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Queries.DashboardAnalytics
{
    public class GetConversionByChannelQuery : IRequest<ApiResponse>
    {
    }

    public class ChannelConversionModel
    {
        public ApplicationSource Source { get; set; }
        public string SourceStr { get; set; }
        public int Applied { get; set; }
        public int Hired { get; set; }
        public double ConversionRate { get; set; }
    }

    public class ConversionByChannelResultModel
    {
        public List<ChannelConversionModel> Channels { get; set; } = new();
        public string? Insight { get; set; }
    }

    public class GetConversionByChannelHandler : IRequestHandler<GetConversionByChannelQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;

        public GetConversionByChannelHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> Handle(
            GetConversionByChannelQuery request,
            CancellationToken cancellationToken)
        {
            var raw = await _context.ApplicationCandidates
                .AsNoTracking()
                .GroupBy(ac => ac.Source)
                .Select(g => new
                {
                    Source = g.Key,
                    Applied = g.Count(),
                    Hired = g.Count(ac => ac.Status == ApplicationStatus.Hired)
                })
                .ToListAsync(cancellationToken);

            var channels = raw
                .Select(x => new ChannelConversionModel
                {
                    Source = x.Source,
                    SourceStr = x.Source.GetDescription(),
                    Applied = x.Applied,
                    Hired = x.Hired,
                    ConversionRate = x.Applied == 0
                        ? 0
                        : Math.Round((x.Hired * 100.0) / x.Applied, 1)
                })
                .OrderByDescending(x => x.ConversionRate)
                .ToList();

            var ranked = channels
                .Where(x => x.Applied > 0)
                .OrderByDescending(x => x.ConversionRate)
                .ToList();

            string? insight = null;

            if (ranked.Count >= 2 && ranked[1].ConversionRate > 0)
            {
                var multiplier = Math.Round(
                    ranked[0].ConversionRate / ranked[1].ConversionRate,
                    1);

                insight =
                    $"{ranked[0].Source} converts at {ranked[0].ConversionRate}% — " +
                    $"{multiplier}x better than {ranked[1].Source}. " +
                    $"Invest in the {ranked[0].Source} program.";
            }
            else if (ranked.Count == 1)
            {
                insight =
                    $"{ranked[0].Source} is your only converting channel so far, " +
                    $"at {ranked[0].ConversionRate}%.";
            }

            var result = new ConversionByChannelResultModel
            {
                Channels = channels,
                Insight = insight
            };

            return new ApiResponse( false, StatusCodes.Status200OK, "Conversion retrieved", result);
        }
    }
}
