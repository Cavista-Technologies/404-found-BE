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
    public class GetCandidateFunnelQuery : IRequest<ApiResponse> { }
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
        private class ApplicationStageInfo
        {
            public ApplicationStage Stage { get; set; }
            public List<StageTransition> History { get; set; }
        }
        private class StageTransition
        {
            public ApplicationStage FromStage { get; set; }
            public ApplicationStage ToStage { get; set; }
        }
        private static int MaxReached(ApplicationStageInfo app)
        {
            int max = StageOrder.TryGetValue(app.Stage, out int s) ? s : 0;
            foreach (var h in app.History)
            {
                if (StageOrder.TryGetValue(h.FromStage, out int f)) max = Math.Max(max, f);
                if (StageOrder.TryGetValue(h.ToStage, out int t)) max = Math.Max(max, t);
            }
            return max;
        }
        public async Task<ApiResponse> Handle(GetCandidateFunnelQuery request, CancellationToken cancellationToken)
        {
            var applications = await _context.Applications
                .Include(a => a.StageHistory)
                .Select(a => new ApplicationStageInfo
                {
                    Stage = a.Stage,
                    History = a.StageHistory.Select(h => new StageTransition
                    {
                        FromStage = h.FromStage,
                        ToStage = h.ToStage
                    }).ToList()
                })
                .ToListAsync(cancellationToken);
            var reached = applications.Select(MaxReached).ToList();
            var result = new CandidateFunnelModel
            {
                Applicants = reached.Count,
                Screened = reached.Count(r => r >= StageOrder[ApplicationStage.Screen]),
                Interviewed = reached.Count(r => r >= StageOrder[ApplicationStage.Interview]),
                Offers = reached.Count(r => r >= StageOrder[ApplicationStage.Offer]),
                Hires = reached.Count(r => r >= StageOrder[ApplicationStage.Hired])
            };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Funnel retrieved", result);
        }
    }
}
