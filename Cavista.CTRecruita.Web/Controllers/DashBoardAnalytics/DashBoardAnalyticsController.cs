using Cavista.CTRecruita.Queries.DashboardAnalytics;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cavista.CTRecruita.Web.Controllers.DashBoardAnalytics
{
    [Route("dashboard")]
    [ApiController]
    [Authorize]
    public class AnalyticsController : BaseController
    {
        public AnalyticsController(IMediator mediator) : base(mediator)
        {
        }
        [HttpGet("snapshot")]
        public async Task<IActionResult> GetSnapshot()
        {
            var response = await _mediator.Send(new GetHiringSnapshotQuery());
            return PrepareResponse(response);
        }
        [HttpGet("time-to-fill")]
        public async Task<IActionResult> GetTimeToFillTrends()
        {
            var response = await _mediator.Send(new GetTimeToFillTrendsQuery());
            return PrepareResponse(response);
        }
        [HttpGet("funnel")]
        public async Task<IActionResult> GetFunnel()
        {
            var response = await _mediator.Send(new GetCandidateFunnelQuery());
            return PrepareResponse(response);
        }
        [HttpGet("conversion-by-channel")]
        public async Task<IActionResult> GetConversionByChannel()
        {
            var response = await _mediator.Send(new GetConversionByChannelQuery());
            return PrepareResponse(response);
        }
    }
}
