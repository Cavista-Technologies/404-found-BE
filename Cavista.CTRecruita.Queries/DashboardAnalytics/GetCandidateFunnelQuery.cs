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
    public class GetCandidateFunnelQuery : IRequest<ApiResponse>
    {
    }

    public class CandidateFunnelModel
    {
        public int Applicants { get; set; }
        public int Screened { get; set; }
        public int Interviewed { get; set; }
        public int Offers { get; set; }
        public int Hires { get; set; }
    }

    public class GetCandidateFunnelHandler : IRequestHandler<GetCandidateFunnelQuery, ApiResponse>
    {
        private static readonly Dictionary<ApplicationStage, int> StageOrder = new()
        {
            [ApplicationStage.Applied] = 1,
            [ApplicationStage.Screen] = 2,
            [ApplicationStage.Interview] = 3,
            [ApplicationStage.Offer] = 4,
            [ApplicationStage.Hired] = 5
        };

        private readonly ApplicationReadOnlyContext _context;

        public GetCandidateFunnelHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }

        private class ApplicationCandidateStageInfo
        {
            public ApplicationStage Stage { get; set; }
            public List<StageTransition> History { get; set; } = new();
        }

        private class StageTransition
        {
            public ApplicationStage FromStage { get; set; }
            public ApplicationStage ToStage { get; set; }
        }

        private static int MaxReached(ApplicationCandidateStageInfo candidate)
        {
            int max = StageOrder.TryGetValue(candidate.Stage, out var current) ? current : 0;

            foreach (var history in candidate.History)
            {
                if (StageOrder.TryGetValue(history.FromStage, out var from))
                {
                    max = Math.Max(max, from);
                }

                if (StageOrder.TryGetValue(history.ToStage, out var to))
                {
                    max = Math.Max(max, to);
                }
            }
            return max;
        }

        public async Task<ApiResponse> Handle(
            GetCandidateFunnelQuery request,
            CancellationToken cancellationToken)
        {
            var candidates = await _context.ApplicationCandidates
                .AsNoTracking()
                .Include(x => x.StageHistory)
                .Select(x => new ApplicationCandidateStageInfo
                {
                    Stage = x.Stage,

                    History = x.StageHistory
                        .Select(h => new StageTransition
                        {
                            FromStage = h.FromStage,
                            ToStage = h.ToStage
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            var reached = candidates
                .Select(MaxReached)
                .ToList();

            var result = new CandidateFunnelModel
            {
                Applicants = reached.Count,

                Screened = reached.Count(x => x >= StageOrder[ApplicationStage.Screen]),
                Interviewed = reached.Count(x => x >= StageOrder[ApplicationStage.Interview]),
                Offers = reached.Count(x => x >= StageOrder[ApplicationStage.Offer]),
                Hires = reached.Count(x => x >= StageOrder[ApplicationStage.Hired])
            };

            return new ApiResponse( false, StatusCodes.Status200OK, "Funnel retrieved", result);
        }
    }
}
